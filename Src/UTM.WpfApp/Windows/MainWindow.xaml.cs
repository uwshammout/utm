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
    private double _area_sqm = double.NaN;
    private double _length_mm = double.NaN;

    private readonly Timer _experimentTimer;
    private DateTime _experimentStartTime = DateTime.MinValue;

    private bool _isSetZeroDisplacementRequested = false, _isSetZeroLoadRequested = false;
    private double _lastDisplacement_mm = 0.0, _lastLoad_kN = 0.0;
    private double _zeroDisplacementValue_mm = 0.0, _zeroLoadValue_kN = 0.0;

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
            double sensorDisplacement_mm = values[0];
            double sensorLoad_kN = values[1];

            if (_isSetZeroDisplacementRequested)
            {
                _zeroDisplacementValue_mm = sensorDisplacement_mm;
                _isSetZeroDisplacementRequested = false;
                DisplacementDisplay.Reset();
            }
            if (_isSetZeroLoadRequested)
            {
                _zeroLoadValue_kN = sensorLoad_kN;
                _isSetZeroLoadRequested = false;
                LoadDisplay.Reset();
            }

            double displacement_mm = sensorDisplacement_mm - _zeroDisplacementValue_mm;
            double load_kN = sensorLoad_kN - _zeroLoadValue_kN;

            DisplacementDisplay.Value = displacement_mm;
            LoadDisplay.Value = load_kN;

            //- Plotting

            switch (_plottingState)
            {
                case PlottingState.None: break;
                case PlottingState.StressStrain:
                    {
                        double time_sec = (DateTime.Now - _experimentStartTime).TotalSeconds;
                        double stress = (load_kN * 1000) / _area_sqm;
                        double strain = displacement_mm / _length_mm;

                        LoadDisplacementPlot.Update(displacement_mm, load_kN * 1000, 0, 0);
                        StressStrainPlot.Update(strain, stress, 0, 0);

                        //- Let's rescale the charts if values go out of bounds
                        if (displacement_mm >= 15 && LoadDisplacementPlot.XAxisStep == 1) LoadDisplacementPlot.XAxisStep = 2;
                        if (displacement_mm >= 30 && LoadDisplacementPlot.XAxisStep == 2) LoadDisplacementPlot.XAxisStep = 5;
                        if (displacement_mm >= 75 && LoadDisplacementPlot.XAxisStep == 5) LoadDisplacementPlot.XAxisStep = 10;
                        if (displacement_mm >= 150 && LoadDisplacementPlot.XAxisStep == 10) LoadDisplacementPlot.XAxisStep = 20;
                        if (displacement_mm >= 300 && LoadDisplacementPlot.XAxisStep == 20) LoadDisplacementPlot.XAxisStep = 40;
                        if (displacement_mm >= 600 && LoadDisplacementPlot.XAxisStep == 40) LoadDisplacementPlot.XAxisStep = 60;
                        if (displacement_mm >= 900 && LoadDisplacementPlot.XAxisStep == 60) LoadDisplacementPlot.XAxisStep = 100;
                        if (displacement_mm >= 1500 && LoadDisplacementPlot.XAxisStep == 100) LoadDisplacementPlot.XAxisStep = 1000;

                        if (strain >= 15 && StressStrainPlot.XAxisStep == 1) StressStrainPlot.XAxisStep = 2;
                        if (strain >= 30 && StressStrainPlot.XAxisStep == 2) StressStrainPlot.XAxisStep = 5;
                        if (strain >= 75 && StressStrainPlot.XAxisStep == 5) StressStrainPlot.XAxisStep = 10;
                        if (strain >= 150 && StressStrainPlot.XAxisStep == 10) StressStrainPlot.XAxisStep = 20;
                        if (strain >= 300 && StressStrainPlot.XAxisStep == 20) StressStrainPlot.XAxisStep = 40;
                        if (strain >= 600 && StressStrainPlot.XAxisStep == 40) StressStrainPlot.XAxisStep = 60;
                        if (strain >= 900 && StressStrainPlot.XAxisStep == 60) StressStrainPlot.XAxisStep = 100;
                        if (strain >= 1500 && StressStrainPlot.XAxisStep == 100) StressStrainPlot.XAxisStep = 1000;

                        //- Writing to file
                        WriteCsvDumpFileValues(
                            _plottingState,
                            time_sec,
                            displacement_mm, load_kN,
                            _area_sqm, stress, _length_mm, strain);
                    }
                    break;
            }

            //- Storing for next iteration
            _lastDisplacement_mm = displacement_mm;
            _lastLoad_kN = load_kN;
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
                        if (_device.OperationState != OperationState.Running)
                        {
                            MessageBox.Show("Connect the device for starting measurements", "Device Not Connected");
                            OnMenuItemClicked(ConnectMenuItem, null!);
                            return;
                        }
                        if (_area_sqm > 0) {  /* Ok */ }
                        else
                        {
                            MessageBox.Show("Input the cross-sectional area of sample and press ENTER", "Area Invalid");
                            AreaOverrideValue.Focus();
                            return;
                        }
                        if (_length_mm > 0) {  /* Ok */ }
                        else
                        {
                            MessageBox.Show("Input the length of sample and press ENTER", "Length Invalid");
                            LengthOverrideValue.Focus();
                            return;
                        }
                    }

                    CompressionTestStartButton.Content = _stopText;

                    _lastDisplacement_mm = 0.0;
                    _lastLoad_kN = 0.0;
                    LoadDisplacementPlot.ClearData();
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
                _csvDumpFile.WriteLine(
                    $"Time (sec), Displacement (mm), Load (kN), Area (m²), Stress, Length (mm), Strain");
                break;
        }
    }
    private void WriteCsvDumpFileValues(PlottingState state,
        double time_sec,
        double displacement_mm, double load_kN,
        double area_sqm, double stress, double length_mm, double strain)
    {
        switch (state)
        {
            case PlottingState.None: break;
            case PlottingState.StressStrain:
                _csvDumpFile.WriteLine(
                    $"{time_sec:0.000}, {displacement_mm:0.000}, {load_kN:0.000}," +
                    $" {area_sqm:0.000}, {stress:0.000}, {length_mm:0.000}, {strain:0.000}");
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
                        if (tb == AreaOverrideValue) _area_sqm = value;
                        else if (tb == LengthOverrideValue) _length_mm = value;
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
            if (dv == DisplacementDisplay)
            {
                _isSetZeroDisplacementRequested = true;
            }
            else if (dv == LoadDisplay)
            {
                _isSetZeroLoadRequested = true;
            }
        }
    }
}