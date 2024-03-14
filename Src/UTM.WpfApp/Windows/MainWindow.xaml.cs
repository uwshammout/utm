using CronBlocks.UTM.InternalServices;
using CronBlocks.UTM.Settings;
using CronBlocks.UTM.Startup;
using CronBlocks.Helpers.Extensions;
using CronBlocks.SerialPortInterface.Entities;
using CronBlocks.SerialPortInterface.Interfaces;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CronBlocks.SerialPortInterface.Services;
using CronBlocks.UTM.UserControls;

namespace CronBlocks.UTM.Windows;

public partial class MainWindow : Window
{
    private enum PlottingState
    {
        None, StressStrain
    }

    private readonly App _app;
    private readonly DataExchangeService _dataExchange;
    private readonly ISerialModbusClientService _device;
    private readonly ISerialModbusDataCalibrationService _deviceData;

    private Mutex _computationMutex = new Mutex(false);
    private double _area = double.NaN;
    private double _length = double.NaN;

    private readonly Timer _experimentTimer;
    private DateTime _experimentStartTime = DateTime.MinValue;

    private bool _isSetZeroDistanceRequested = false, _isSetZeroLoadRequested = false;
    private double _lastDistance = 0.0, _lastLoad = 0.0;
    private double _zeroDistanceValue = 0.0, _zeroLoadValue = 0.0;

    private readonly string _csvDumpFilename;
    private StreamWriter _csvDumpFile = null!;

    private PlottingState _plottingState;
    private PlottingState _lastPlottingState;

    private readonly string _startText, _stopText = "STOP";
    private readonly Brush _initialBgColor = null!;

    public MainWindow(
        App app,
        DataExchangeService dataExchange,
        ISerialModbusClientService device,
        ISerialModbusDataCalibrationService dataService)
    {
        InitializeComponent();

        _startText = (string)CompressionTestStartButton.Content;
        _initialBgColor = AreaOverrideValue.Background;

        _app = app;
        _dataExchange = dataExchange;
        _device = device;
        _deviceData = dataService;

        _experimentTimer = new Timer(OnExperimentTimerTick, null, 400, 400);

        _csvDumpFilename = FilePaths.CsvDumpFilename;
        _csvDumpFilename.CreateFoldersForRelativeFilename();

        _plottingState = PlottingState.None;
        _lastPlottingState = PlottingState.None;
        ChangePlottingState(_plottingState);

        _device.OperationStateChanged += OnDeviceOperationStateChanged;
        OnDeviceOperationStateChanged(_device.OperationState);

        _deviceData.NewValuesReceived += OnNewValuesReceived;
    }

    protected override void OnClosed(EventArgs e)
    {
        _experimentTimer.Dispose();

        _dataExchange.Dispose();
        _deviceData.Dispose();
        _device.Dispose();

        base.OnClosed(e);
    }
    
    private void OnExperimentTimerTick(object? state)
    {
        Dispatcher.Invoke(() =>
        {
            switch (_plottingState)
            {
                case PlottingState.None: break;

                case PlottingState.StressStrain:

                    double secondsElapsed = (DateTime.Now - _experimentStartTime).TotalSeconds;
                    TimeDisplay.Value = secondsElapsed;
                    break;
            }
        });
    }

    private void OnNewValuesReceived(List<double> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        if (values.Count < CronBlocks.SerialPortInterface.Configuration.Constants.TotalRegisters)
        {
            throw new InvalidOperationException(
                $"Expected number of values is {CronBlocks.SerialPortInterface.Configuration.Constants.TotalRegisters}" +
                $" whereas, {values.Count} values are received");
        }

        Dispatcher.Invoke(() =>
        {
            double time = (DateTime.Now - _experimentStartTime).TotalSeconds;

            double sensorDistance = values[0];
            double sensorLoad = values[1];

            if (_isSetZeroDistanceRequested)
            {
                _zeroDistanceValue = sensorDistance;
                _isSetZeroDistanceRequested = false;
                DistanceDisplay.Reset();
            }
            if (_isSetZeroLoadRequested)
            {
                _zeroLoadValue = sensorLoad;
                _isSetZeroLoadRequested = false;
                LoadDisplay.Reset();
            }

            double currentDistance = sensorDistance - _zeroDistanceValue;
            double currentLoad = sensorLoad - _zeroLoadValue;

            DistanceDisplay.Value = currentDistance;
            LoadDisplay.Value = currentLoad;

            //- Plotting

            switch (_plottingState)
            {
                case PlottingState.None: break;
                case PlottingState.StressStrain:
                    {
                        double stress = (currentLoad * 1000) / _area;
                        double strain = (currentDistance - _lastDistance) / _length;
                        _length = _length - currentDistance - _lastDistance;
                        LengthOverrideValue.Text = _length.ToString();

                        //- Writing to file
                        WriteCsvDumpFileValues(_plottingState, time, currentDistance, currentLoad);

                        _lastDistance = currentDistance;
                        _lastLoad = currentLoad;
                    }
                    break;
            }
        });
    }

    private void OnDeviceOperationStateChanged(OperationState state)
    {
        ChangePlottingState(PlottingState.None);

        Dispatcher.Invoke(() =>
        {
            switch (state)
            {
                case OperationState.Running:
                    MessageBar.Text = "✓ Device connected";
                    break;

                case OperationState.Stopped:
                    MessageBar.Text = "❌ Device disconnected";
                    break;
            }
        });
    }

    private void ChangePlottingState(PlottingState state)
    {
        Dispatcher.Invoke(() =>
        {
            //- Last state cleanup - if any

            switch (_lastPlottingState)
            {
                case PlottingState.None: break;
                case PlottingState.StressStrain: break;
            }

            //- Current state setup

            switch (state)
            {
                case PlottingState.None:
                    CompressionTestStartButton.Content = _startText;
                    if (_csvDumpFile != null)
                    {
                        _csvDumpFile.Flush();
                        _csvDumpFile.Dispose();
                        _csvDumpFile = null!;
                        SaveCsvMenuItem.IsEnabled = true;
                    }
                    break;

                case PlottingState.StressStrain:
                    lock (_computationMutex)
                    {
                        if (_area > 0) {  /* Ok */ }
                        else
                        {
                            MessageBox.Show("Invalid cross-sectional area of sample is specified");
                            return;
                        }
                        if (_length > 0) {  /* Ok */ }
                        else
                        {
                            MessageBox.Show("Invalid length of sample is specified");
                            return;
                        }
                    }

                    CompressionTestStartButton.Content = _stopText;

                    _lastDistance = 0.0;
                    _lastLoad = 0.0;
                    StressStrainPlot.ClearData();
                    break;
            }

            if (state != PlottingState.None)
            {
                SaveCsvMenuItem.IsEnabled = false;

                _experimentStartTime = DateTime.Now;

                if (_csvDumpFile != null)
                {
                    _csvDumpFile.Flush();
                    _csvDumpFile.Dispose();
                    _csvDumpFile = null!;
                }

                _csvDumpFile = File.CreateText(_csvDumpFilename);
                WriteCsvDumpFileHeader(state);
            }

            _lastPlottingState = _plottingState;
            _plottingState = state;
        });
    }

    //- 
    //- Dump file
    //- 
    private void WriteCsvDumpFileHeader(PlottingState state)
    {
        switch (state)
        {
            case PlottingState.None: break;

            case PlottingState.StressStrain:
                _csvDumpFile.WriteLine($"Time (sec), Distance (mm), Load (kN)");
                break;
        }
    }
    private void WriteCsvDumpFileValues(PlottingState state, double time, double currentDistance, double currentLoad)
    {
        switch (state)
        {
            case PlottingState.None: break;
            case PlottingState.StressStrain:
                _csvDumpFile.WriteLine($"{time:0.000}, {currentDistance:0.000}, {currentLoad:0.000}");
                break;
        }
    }

    private void OnMenuItemClicked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem mi && mi.Header is string header && !string.IsNullOrEmpty(header))
        {
            Window window = null!;

            switch (header)
            {
                case "Save CSV":
                    FileDialog dialog = new SaveFileDialog()
                    {
                        DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        Filter = "CSV File|*.csv",
                        OverwritePrompt = true,
                        Title = header
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        try
                        {
                            File.Copy(_csvDumpFilename, dialog.FileName, true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error");
                        }
                    }
                    break;

                case "Connect":
                    window = _app.GetInstance<DeviceConnectionWindow>();
                    break;

                case "Measurement Settings":
                    window = _app.GetInstance<MeasurementSettingsWindow>();
                    break;

                case "Calibration":
                    window = _app.GetInstance<DeviceCalibrationWindow>();
                    break;

                case "About":
                    window = _app.GetInstance<AboutWindow>();
                    break;
            }

            if (window != null)
            {
                window.ShowDialog();
            }
        }
        else if (sender is RadioButton rb)
        {
        }
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (tb == AreaOverrideValue || tb == LengthOverrideValue)
            {
                if (double.TryParse(tb.Text, out double v) && v > 0)
                {
                    tb.Background = _initialBgColor;
                }
                else
                {
                    tb.Background = DisplayColors.ErrorBg;
                }
            }
        }
    }
    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter && sender is TextBox tb)
        {
            if (tb == AreaOverrideValue || tb == LengthOverrideValue)
            {
                if (double.TryParse(tb.Text, out double value) && value > 0)
                {
                    lock (_computationMutex)
                    {
                        if (tb == AreaOverrideValue) _area = value;
                        else if (tb == LengthOverrideValue) _length = value;
                    }

                    tb.Background = _initialBgColor;
                }
                else
                {
                    tb.Background = DisplayColors.ErrorBg;
                }
            }
        }
    }

    private void OnButtonClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            bool doStop = ((string)button.Content) == _stopText;

            if (doStop)
            {
                ChangePlottingState(PlottingState.None);
            }
            else
            {
                if (button == CompressionTestStartButton)
                {
                    ChangePlottingState(PlottingState.StressStrain);
                }
            }
        }
        else if (sender is DigitalValueDisplay dv)
        {
            if (dv == DistanceDisplay)
            {
                _isSetZeroDistanceRequested = true;
            }
            else if (dv == LoadDisplay)
            {
                _isSetZeroLoadRequested = true;
            }
        }
    }
}