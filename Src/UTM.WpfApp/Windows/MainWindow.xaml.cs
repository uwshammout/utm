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

    private readonly Timer _experimentTimer;
    private DateTime _experimentStartTime = DateTime.MinValue;

    private bool _isSetZeroDistanceRequested = false, _isSetZeroLoadRequested = false;
    private double _lastDistance = 0.0, _lastLoad = 0.0;
    private double _zeroDistanceValue = 0.0, _zeroLoadValue = 0.0;

    private readonly string _csvDumpFilename;
    private StreamWriter _csvDumpFile = null!;

    private PlottingState _plottingState;
    private PlottingState _lastPlottingState;

    private readonly string _stopText = "STOP";

    public MainWindow(
        App app,
        DataExchangeService dataExchange,
        ISerialModbusClientService device,
        ISerialModbusDataCalibrationService dataService)
    {
        InitializeComponent();

        _app = app;
        _dataExchange = dataExchange;
        _device = device;
        _deviceData = dataService;

        _experimentTimer = new Timer(OnExperimentTimerTick, null, 500, 500);

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
            double currentDistance = values[0];
            double currentLoad = values[1];

            if (_isSetZeroDistanceRequested)
            {
                _zeroDistanceValue = currentDistance;
                _isSetZeroDistanceRequested = false;
            }
            if (_isSetZeroLoadRequested)
            {
                _zeroLoadValue = currentLoad;
                _isSetZeroLoadRequested = false;
            }

            //- Plotting

            switch (_plottingState)
            {
                case PlottingState.None: break;
                case PlottingState.StressStrain: break;
            }

            //- Writing to file

            WriteCsvDumpFileValues(_plottingState, currentDistance, currentLoad);

            _lastDistance = currentDistance;
            _lastLoad = currentLoad;
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
                    if (_csvDumpFile != null)
                    {
                        _csvDumpFile.Flush();
                        _csvDumpFile.Dispose();
                        _csvDumpFile = null!;
                        SaveCsvMenuItem.IsEnabled = true;
                    }
                    break;

                case PlottingState.StressStrain:
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
                _csvDumpFile.WriteLine($"Distance (mm), Load (kN)");
                break;
        }
    }
    private void WriteCsvDumpFileValues(PlottingState state, double currentDistance, double currentLoad)
    {
        switch (state)
        {
            case PlottingState.None: break;
            case PlottingState.StressStrain:
                _csvDumpFile.WriteLine($"{currentDistance:0.0000}, {currentLoad:0.0000}");
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
            if (tb == AreaOverrideValue)
            {
                if (double.TryParse(tb.Text, out double _))
                {
                    //tb.Background = _currentOverrideValueInitialColor;
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
            if (tb == AreaOverrideValue)
            {
                if (double.TryParse(tb.Text, out double value))
                {
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
                else if (button == SetZeroDistanceButton)
                {
                    _isSetZeroDistanceRequested = true;
                }
                else if (button == SetZeroLoadButton)
                {
                    _isSetZeroLoadRequested = true;
                }
            }
        }
    }
}