using CronBlocks.SerialPortInterface.Interfaces;
using CronBlocks.UTM.InternalServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace CronBlocks.UTM.Windows;

public partial class MeasurementSettingsWindow : Window, INotifyPropertyChanged
{
    private readonly ISerialModbusClientService _modbus;
    private readonly DataExchangeService _dataExchange;

    private double _acquisitionIntervalMinimum;
    private double _acquisitionIntervalMaximum;
    private double _acquisitionIntervalValue;

    public MeasurementSettingsWindow(
        ISerialModbusClientService modbus,
        DataExchangeService dataExchange)
    {
        InitializeComponent();

        _modbus = modbus;
        _dataExchange = dataExchange;

        AcquisitionIntervalMinimum = CronBlocks.SerialPortInterface.Configuration.Constants.MinimumDataAcquisitionIntervalMS;
        AcquisitionIntervalMaximum = CronBlocks.SerialPortInterface.Configuration.Constants.MaximumDataAcquisitionIntervalMS;
        AcquisitionIntervalValue = _modbus.GetDataAcquisitionInterval();

        DataContext = this;
    }

    public double AcquisitionIntervalMinimum
    {
        get => _acquisitionIntervalMinimum;
        set
        {
            if (_acquisitionIntervalMinimum != value)
            {
                _acquisitionIntervalMinimum = value;
                NotifyPropertyChanged();
            }
        }
    }
    public double AcquisitionIntervalMaximum
    {
        get => _acquisitionIntervalMaximum;
        set
        {
            if (_acquisitionIntervalMaximum != value)
            {
                _acquisitionIntervalMaximum = value;
                NotifyPropertyChanged();
            }
        }
    }
    public double AcquisitionIntervalValue
    {
        get => _acquisitionIntervalValue;
        set
        {
            if (_acquisitionIntervalValue != value)
            {
                _acquisitionIntervalValue = value;
                NotifyPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is Slider s)
        {
            if (s == AcquisitionInterval)
            {
                _modbus.SetDataAcquisitionInterval(s.Value);
            }
        }
    }
}
