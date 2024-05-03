using CronBlocks.Helpers;
using CronBlocks.SerialPortInterface.Configuration;
using CronBlocks.SerialPortInterface.Entities;
using CronBlocks.SerialPortInterface.Extensions;
using CronBlocks.SerialPortInterface.Interfaces;
using FluentModbus;
using Microsoft.Extensions.Logging;

namespace CronBlocks.SerialPortInterface.Services;

public class SerialModbusClientService : ISerialModbusClientService
{
    public event Action<List<double>>? NewValuesReceived;
    public event Action<OperationState>? OperationStateChanged;

    public OperationState OperationState { get { return _operationState; } }

    private readonly object _lockObject = new();

    private readonly ILogger<SerialModbusClientService> _logger;
    private readonly IniConfigIO _configFile;

    //- Timer

    private bool _isRunning = false;
    private OperationState _operationState;
    private Timer _timer;

    //- Com Settings

    private bool _settingsProvided = false;

    private string _comPort = null!;

    private int _deviceAddress = Constants.DefaultDeviceAddress;
    private int _registersStartAddress = Convert.ToInt32(Constants.DefaultRegistersStartAddressHexStr, 16);

    private BaudRate _baudRate = Constants.DefaultBaudRate;
    private DataBits _dataBits = Constants.DefaultDataBits;
    private Parity _parity = Constants.DefaultParity;
    private StopBits _stopBits = Constants.DefaultStopBits;

    private double _dataAcquisitionInterval;

    //- MODBUS Client

    private ModbusRtuClient _client;

    //- Received Values

    private double[] _valuesReceived;

    public SerialModbusClientService(
        ILogger<SerialModbusClientService> logger,
        IniConfigIO configFile = null!)
    {
        _isRunning = false;
        _operationState = OperationState.Stopped;
        _timer = new Timer(AcquireData, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

        _logger = logger;
        _configFile = configFile;

        _settingsProvided = false;
        _client = null!;

        _valuesReceived = new double[Constants.TotalRegisters];

        _dataAcquisitionInterval = Constants.DefaultDataAcquisitionIntervalMS;

        LoadSettings();
    }

    private void LoadSettings()
    {
        if (_configFile == null) return;

        _deviceAddress = _configFile.GetInteger("MODBUS/DeviceAddress", Constants.DefaultDeviceAddress);
        _registersStartAddress = _configFile.GetInteger("MODBUS/RegAddress", Convert.ToInt32(Constants.DefaultRegistersStartAddressHexStr, 16));
        _baudRate = _configFile.GetString("MODBUS/BR", Constants.DefaultBaudRate.ToDisplayString()).FromDisplayString<BaudRate>();
        _dataBits = _configFile.GetString("MODBUS/DB", Constants.DefaultDataBits.ToDisplayString()).FromDisplayString<DataBits>();
        _parity = _configFile.GetString("MODBUS/PR", Constants.DefaultParity.ToDisplayString()).FromDisplayString<Parity>();
        _stopBits = _configFile.GetString("MODBUS/SB", Constants.DefaultStopBits.ToDisplayString()).FromDisplayString<StopBits>();
        _dataAcquisitionInterval = _configFile.GetDouble("MODBUS/Interval", Constants.DefaultDataAcquisitionIntervalMS);
    }

    private void SaveSettings()
    {
        if (_configFile == null) return;

        _configFile.SetInteger("MODBUS/DeviceAddress", _deviceAddress);
        _configFile.SetInteger("MODBUS/RegAddress", _registersStartAddress);
        _configFile.SetString("MODBUS/BR", _baudRate.ToDisplayString());
        _configFile.SetString("MODBUS/DB", _dataBits.ToDisplayString());
        _configFile.SetString("MODBUS/PR", _parity.ToDisplayString());
        _configFile.SetString("MODBUS/SB", _stopBits.ToDisplayString());
        _configFile.SetDouble("MODBUS/Interval", _dataAcquisitionInterval);

        _configFile.SaveFile();
    }

    public void SetComSettings(SerialModbusClientSettings portSettings)
    {
        lock (_lockObject)
        {
            if (_isRunning || _client != null)
            {
                throw new InvalidOperationException(
                    $"MODBUS communication settings cannot be updated while running");
            }
            else
            {
                _settingsProvided = true;

                _comPort = portSettings.ComPort;

                _deviceAddress = portSettings.DeviceAddress;
                _registersStartAddress = Convert.ToInt32(portSettings.RegistersStartAddressHexStr, 16);

                _baudRate = portSettings.BaudRate;
                _dataBits = portSettings.DataBits;
                _parity = portSettings.Parity;
                _stopBits = portSettings.StopBits;
            }
        }

        SaveSettings();
    }

    public SerialModbusClientSettings GetComSettings()
    {
        return new()
        {
            ComPort = _comPort,
            DeviceAddress = _deviceAddress,
            RegistersStartAddressHexStr =
                  $"0x00{_registersStartAddress:X}",
            BaudRate = _baudRate,
            DataBits = _dataBits,
            Parity = _parity,
            StopBits = _stopBits
        };
    }

    public void SetDataAcquisitionInterval(double milliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            milliseconds,
            Constants.MaximumDataAcquisitionIntervalMS,
            nameof(milliseconds));

        ArgumentOutOfRangeException.ThrowIfLessThan(
            milliseconds,
            Constants.MinimumDataAcquisitionIntervalMS,
            nameof(milliseconds));

        _dataAcquisitionInterval = milliseconds;

        SaveSettings();
    }

    public double GetDataAcquisitionInterval()
    {
        return _dataAcquisitionInterval;
    }

    #region Start / Stop Acquisition
    public void StartAcquisition()
    {
        lock (_lockObject)
        {
            if (_isRunning || _client != null)
            {
                throw new InvalidOperationException(
                    $"{nameof(StartAcquisition)} cannot be invoked while operation" +
                    $" is in progress.");
            }
            else
            {
                _logger.LogInformation(
                    $"Starting data acquisition from {_comPort}" +
                    $" @{_dataAcquisitionInterval}ms interval");

                CreateComClientObject();

                _client?.Connect(_comPort, Constants.ModbusEndianness);

                _isRunning = true;

                _timer.Change(
                    TimeSpan.FromMilliseconds(_dataAcquisitionInterval),
                    TimeSpan.FromMilliseconds(_dataAcquisitionInterval));
            }
        }
    }

    public void StopAcquisition()
    {
        lock (_lockObject)
        {
            if (_isRunning)
            {
                _logger.LogInformation($"Stopping data acquisition from {_comPort}");

                _isRunning = false;
            }
        }
    }

    private void CreateComClientObject()
    {
        if (_settingsProvided == false)
        {
            throw new InvalidOperationException(
                "MODBUS cannot be initialized without providing settings");
        }
        else
        {
            System.IO.Ports.Parity parity = System.IO.Ports.Parity.None;
            System.IO.Ports.StopBits stopBits = System.IO.Ports.StopBits.None;

            switch (_parity)
            {
                case Entities.Parity.None:
                    parity = System.IO.Ports.Parity.None;
                    break;

                case Entities.Parity.Odd:
                    parity = System.IO.Ports.Parity.Odd;
                    break;

                case Entities.Parity.Even:
                    parity = System.IO.Ports.Parity.Even;
                    break;
            }

            switch (_stopBits)
            {
                case Entities.StopBits.One:
                    stopBits = System.IO.Ports.StopBits.One;
                    break;

                case Entities.StopBits.Two:
                    stopBits = System.IO.Ports.StopBits.Two;
                    break;

                case Entities.StopBits.OnePointFive:
                    stopBits = System.IO.Ports.StopBits.OnePointFive;
                    break;
            }

            _client = new ModbusRtuClient()
            {
                BaudRate = (int)_baudRate,
                Parity = parity,
                StopBits = stopBits
            };
        }
    }
    #endregion

    #region Acquire Data
    private void AcquireData(object? obj)
    {
        _timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

        if (_operationState == OperationState.Stopped)
        {
            _operationState = OperationState.Running;
            OperationStateChanged?.Invoke(_operationState);
        }

        short[] shortData = null!;

        try
        {
            shortData = _client.ReadHoldingRegisters<short>(
                _deviceAddress,
                _registersStartAddress,
                Constants.TotalRegisters).ToArray();
        }
        catch (TimeoutException)
        {
            //- Not doing anything here
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred while reading at {_comPort}: {ex.Message}");
            _logger.LogInformation($"Stopping acquisition due to error");

            _isRunning = false;
            shortData = null!;
        }

        if (shortData != null &&
            shortData.Length <= Constants.TotalRegisters &&
            shortData.Length >= 1)
        {
            for (int i = 0; i < shortData.Length; i++)
            {
                _valuesReceived[i] = ((double)shortData[i]) / 1000.0;
            }

            NewValuesReceived?.Invoke(_valuesReceived.ToList());
        }

        bool stillRunning = false;

        lock (_lockObject)
        {
            stillRunning = _isRunning;

            if (_isRunning)
            {
                _timer.Change(
                    TimeSpan.FromMilliseconds(_dataAcquisitionInterval),
                    TimeSpan.FromMilliseconds(_dataAcquisitionInterval));
            }
            else
            {
                _client.Dispose();
                _client = null!;
            }
        }

        if (!stillRunning)
        {
            _logger.LogInformation($"Stopped data acquisition from {_comPort}");

            _operationState = OperationState.Stopped;
            OperationStateChanged?.Invoke(_operationState);
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
    #endregion
}
