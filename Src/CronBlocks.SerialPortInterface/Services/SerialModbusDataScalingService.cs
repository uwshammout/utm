using CronBlocks.Helpers;
using CronBlocks.SerialPortInterface.Configuration;
using CronBlocks.SerialPortInterface.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace CronBlocks.SerialPortInterface.Services;

public class SerialModbusDataScalingService : ISerialModbusDataScalingService
{
    public event Action<List<double>>? NewValuesReceived;

    private readonly ILogger<SerialModbusDataScalingService> _logger;
    private readonly ISerialModbusClientService _service;
    private readonly IniConfigIO _iniConfig;
    private readonly string _filename;

    private double[] _scaledValues;
    private double[] _multiplicationFactors;
    private double[] _offsets;

    public SerialModbusDataScalingService(
        ILogger<SerialModbusDataScalingService> logger,
        ISerialModbusClientService service,
        string filename = null!,
        ILogger<IniConfigIO> iniLogger = null!)
    {
        _logger = logger;
        _service = service;
        _filename = filename;
        _iniConfig = null!;

        _scaledValues = new double[Constants.TotalRegisters];
        _multiplicationFactors = new double[Constants.TotalRegisters];
        _offsets = new double[Constants.TotalRegisters];

        for (int i = 0; i < Constants.TotalRegisters; i++)
        {
            _multiplicationFactors[i] = 1;
            _offsets[i] = 0;
        }

        if (string.IsNullOrEmpty(_filename))
        {
            _logger.LogInformation(
                $"No filename is provided, so we are using defaults.");
        }
        else
        {
            _iniConfig = new(_filename, iniLogger);
            LoadValuesFromFile();
        }

        _service.NewValuesReceived += OnNewModbusValuesReceived;
    }

    public ImmutableList<double> GetMultiplicationFactors()
    {
        return _multiplicationFactors.ToImmutableList();
    }

    public ImmutableList<double> GetOffsets()
    {
        return _offsets.ToImmutableList();
    }

    public void SetMultiplicationFactor(int index, double value)
    {
        if (index < 0 || index >= Constants.TotalRegisters)
        {
            throw new ArgumentOutOfRangeException("index");
        }

        _multiplicationFactors[index] = value;
    }

    public void SetOffset(int index, double value)
    {
        if (index < 0 || index >= Constants.TotalRegisters)
        {
            throw new ArgumentOutOfRangeException("index");
        }

        _offsets[index] = value;
    }

    public void LoadValuesFromFile()
    {
        if (_iniConfig != null)
        {
            for (int i = 0; i < Constants.TotalRegisters; i++)
            {
                _multiplicationFactors[i] = _iniConfig.GetDouble($"FACTORS/MultiplicationFactor_{i}", 1);
                _offsets[i] = _iniConfig.GetDouble($"FACTORS/Offset_{i}", 0);
            }
        }
    }

    public void SaveValuesToFile()
    {
        if (_iniConfig != null)
        {
            for (int i = 0; i < Constants.TotalRegisters; i++)
            {
                _iniConfig.SetDouble($"FACTORS/MultiplicationFactor_{i}", _multiplicationFactors[i]);
                _iniConfig.SetDouble($"FACTORS/Offset_{i}", _offsets[i]);
            }

            _iniConfig.SaveFile();
        }
    }

    public void Dispose()
    {
        try
        {
            _service.NewValuesReceived -= OnNewModbusValuesReceived;
        }
        catch { }

        SaveValuesToFile();
    }

    private void OnNewModbusValuesReceived(List<double> values)
    {
        if (values == null)
        {
            _logger.LogError(
                $"The list of 'values' received from MODBUS service is null.");

            return;
        }
        else if (values.Count < Constants.TotalRegisters)
        {
            _logger.LogError(
                $"The list of 'values' received from MODBUS" +
                $" has {values.Count} values, whereas" +
                $" {Constants.TotalRegisters} values are expected.");

            return;
        }

        for (int i = 0; i < Constants.TotalRegisters; i++)
        {
            _scaledValues[i] =
                (values[i] * _multiplicationFactors[i]) + _offsets[i];
        }

        NewValuesReceived?.Invoke(_scaledValues.ToList());
    }
}
