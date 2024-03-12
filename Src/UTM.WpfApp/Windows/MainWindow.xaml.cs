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

    private bool _isSetZeroDistanceRequested = false;
    private bool _isSetZeroLoadRequested = false;
    private double _zeroDistanceValue = 0.0;
    private double _zeroLoadValue = 0.0;

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
            //- Fuel Cell

            double currentDistance = values[0];
            double currentLoad = values[1];

            double fcTotalCurrent = Math.Abs(values[1] - values[2]) / _dataExchange.FuelCellCurrentMeasurementResistance;

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
                    MessageBar.Text = "Device connected";

                    FuelCellStartButton.IsEnabled = true;
                    FuelCellSeriesStartButton.IsEnabled = true;
                    ElectrolyzerStartButton.IsEnabled = true;
                    break;

                case OperationState.Stopped:
                    MessageBar.Text = "Device not connected";

                    FuelCellStartButton.IsEnabled = false;
                    FuelCellSeriesStartButton.IsEnabled = false;
                    ElectrolyzerStartButton.IsEnabled = false;
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
                case PlottingState.FuelCell: break;
                case PlottingState.FuelCellSeries: break;
                case PlottingState.Electrolyzer: break;
            }

            //- Current state setup

            switch (state)
            {
                case PlottingState.None:
                    DataProgress.Visibility = Visibility.Hidden;
                    DataProgressMessage.Visibility = Visibility.Hidden;

                    FuelCellTabItem.IsEnabled = true;
                    FuelCellSeriesTabItem.IsEnabled = true;
                    ElectrolyzerTabItem.IsEnabled = true;

                    FuelCellStartButton.Background = _fuelCellStartButtonInitialColor;
                    FuelCellSeriesStartButton.Background = _fuelCellSeriesStartButtonInitialColor;
                    ElectrolyzerStartButton.Background = _electrolyzerStartButtonInitialColor;

                    FuelCellStartButton.Content = _fuelCellStartButtonInitialText;
                    FuelCellSeriesStartButton.Content = _fuelCellSeriesStartButtonInitialText;
                    ElectrolyzerStartButton.Content = _electrolyzerStartButtonInitialText;

                    if (_csvDumpFile != null)
                    {
                        _csvDumpFile.Flush();
                        _csvDumpFile.Dispose();
                        _csvDumpFile = null!;

                        SaveCsvMenuItem.IsEnabled = true;
                    }

                    break;

                case PlottingState.FuelCell:
                    FuelCellTabItem.IsEnabled = true;
                    FuelCellSeriesTabItem.IsEnabled = false;
                    ElectrolyzerTabItem.IsEnabled = false;

                    FuelCellStartButton.Background = DisplayColors.DisconnectButtonBg;
                    FuelCellStartButton.Content = _stopText;

                    FuelCellVIPlot.ClearData();
                    FuelCellPTPlot.ClearData();
                    FuelCellPIPlot.ClearData();
                    FuelCellPVPlot.ClearData();

                    FuelCellVoltageGauge.Dial2Value = 0;
                    FuelCellVoltageGauge.Dial3Value = 0;
                    FuelCellCurrentGauge.Dial2Value = 0;
                    FuelCellCurrentGauge.Dial3Value = 0;
                    FuelCellPowerGauge.Dial2Value = 0;
                    FuelCellPowerGauge.Dial3Value = 0;
                    break;

                case PlottingState.FuelCellSeries:
                    FuelCellTabItem.IsEnabled = false;
                    FuelCellSeriesTabItem.IsEnabled = true;
                    ElectrolyzerTabItem.IsEnabled = false;

                    FuelCellSeriesStartButton.Background = DisplayColors.DisconnectButtonBg;
                    FuelCellSeriesStartButton.Content = _stopText;

                    FuelCellSeriesVIPlot.ClearData();
                    FuelCellSeriesPTPlot.ClearData();

                    FuelCellSeriesVoltageGauge.Dial2Value = 0;
                    FuelCellSeriesVoltageGauge.Dial3Value = 0;
                    FuelCellSeriesCurrentGauge.Dial2Value = 0;
                    FuelCellSeriesCurrentGauge.Dial3Value = 0;
                    FuelCellSeriesPowerGauge.Dial2Value = 0;
                    FuelCellSeriesPowerGauge.Dial3Value = 0;
                    break;

                case PlottingState.Electrolyzer:
                    FuelCellTabItem.IsEnabled = false;
                    FuelCellSeriesTabItem.IsEnabled = false;
                    ElectrolyzerTabItem.IsEnabled = true;

                    ElectrolyzerStartButton.Background = DisplayColors.DisconnectButtonBg;
                    ElectrolyzerStartButton.Content = _stopText;

                    ElectrolyzerIVPlot.ClearData();

                    ElectrolyzerVoltageGauge.Dial2Value = 0;
                    ElectrolyzerVoltageGauge.Dial3Value = 0;
                    ElectrolyzerCurrentGauge.Dial2Value = 0;
                    ElectrolyzerCurrentGauge.Dial3Value = 0;
                    ElectrolyzerPowerGauge.Dial2Value = 0;
                    ElectrolyzerPowerGauge.Dial3Value = 0;
                    break;
            }

            if (state != PlottingState.None)
            {
                SaveCsvMenuItem.IsEnabled = false;

                DataProgress.Visibility = Visibility.Visible;
                DataProgressMessage.Visibility = Visibility.Visible;

                DataProgress.Value = 0;

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

    private void OnCurrentOverrideTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (tb == FuelCellCurrentOverrideValue ||
                tb == FuelCellSeriesCurrentOverrideValue ||
                tb == ElectrolyzerCurrentOverrideValue)
            {
                if (double.TryParse(tb.Text, out double _))
                {
                    tb.Background = _currentOverrideValueInitialColor;
                }
                else
                {
                    tb.Background = DisplayColors.ErrorBg;
                }
            }
        }
    }

    private void OnCurrentOverrideTextKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            if (sender is TextBox tb)
            {
                if (tb == FuelCellCurrentOverrideValue ||
                    tb == FuelCellSeriesCurrentOverrideValue)
                {
                    if (double.TryParse(tb.Text, out double value))
                    {
                        _fuelCellCurrentOverrideValue = value;

                        FuelCellCurrentOverrideValue.Text = value.ToString();
                        FuelCellSeriesCurrentOverrideValue.Text = value.ToString();

                        FuelCellCurrentOverrideValue.Background = _currentOverrideValueInitialColor;
                        FuelCellSeriesCurrentOverrideValue.Background = _currentOverrideValueInitialColor;
                    }
                    else
                    {
                        tb.Background = DisplayColors.ErrorBg;
                    }
                }
                else if (tb == ElectrolyzerCurrentOverrideValue)
                {
                    if (double.TryParse(tb.Text, out double value))
                    {
                        _electrolyzerCurrentOverrideValue = value;
                        tb.Text = value.ToString();
                        tb.Background = _currentOverrideValueInitialColor;
                    }
                    else
                    {
                        tb.Background = DisplayColors.ErrorBg;
                    }
                }
            }
        }
    }

    private void OnButtonClicked(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        bool doStop = ((string)button.Content) == _stopText;

        if (doStop)
        {
            ChangePlottingState(PlottingState.None);
        }
        else
        {
            if (button == CompressionTestStartButton)
            {
                ChangePlottingState(PlottingState.FuelCell);
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