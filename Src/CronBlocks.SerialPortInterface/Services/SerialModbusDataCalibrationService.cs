using CronBlocks.Helpers;
using CronBlocks.SerialPortInterface.Configuration;
using CronBlocks.SerialPortInterface.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace CronBlocks.SerialPortInterface.Services;

public class SerialModbusDataCalibrationService : ISerialModbusDataCalibrationService
{
    public event Action<List<double>>? NewValuesReceived;

    private readonly ISerialModbusDataScalingService _scalingService;
    private readonly ILogger<SerialModbusDataCalibrationService> _logger;
    private readonly string _filename;
    private readonly uint _totalCalibrationPoints;
    private readonly double _inputRangeMin;
    private readonly double _inputRangeMax;
    private readonly IniConfigIO _iniConfig;

    private double[] _calibratedValues;
    private List<Dictionary<double,double>> _calibrationData;

    public SerialModbusDataCalibrationService(
        ISerialModbusDataScalingService scalingService,
        ILogger<SerialModbusDataCalibrationService> logger,
        string filename = null!,
        ILogger<IniConfigIO> iniLogger = null!,
        uint totalCalibrationPoints = Constants.TotalCalibrationPoints,
        double inputRangeMin = Constants.InputRangeMin, double inputRangeMax = Constants.InputRangeMax)
    {
        _scalingService = scalingService;
        _logger = logger;
        _filename = filename;
        _totalCalibrationPoints = totalCalibrationPoints;
        _inputRangeMin = inputRangeMin;
        _inputRangeMax = inputRangeMax;
        _iniConfig = null!;

        _calibratedValues = new double[Constants.TotalRegisters];
        _calibrationData = new();

        for (int i = 0; i < Constants.TotalRegisters; i++)
        {
            _calibrationData.Add([]);
        }

        if (string.IsNullOrEmpty(_filename))
        {
            _logger.LogInformation($"No filename is provided, so we are using defaults of no calibration.");
        }
        else
        {
            _iniConfig = new(_filename, iniLogger);
            LoadCalibrationDataFromFile();
        }

        _scalingService.NewValuesReceived += OnNewScaledValuesReceived;
    }

    public void LoadCalibrationDataFromFile()
    {
        if (_iniConfig != null)
        {
            // TODO: Load calibration data
        }
    }

    public void SaveCalibrationDataToFile()
    {
        if (_iniConfig != null)
        {
            // TODO: Save calibration data

            _iniConfig.SaveFile();
        }
    }

    public void Dispose()
    {
        try
        {
            _scalingService.NewValuesReceived -= OnNewScaledValuesReceived;
        }
        catch { }

        SaveCalibrationDataToFile();
    }

    private void OnNewScaledValuesReceived(List<double> scaledValues)
    {
        if (scaledValues == null)
        {
            _logger.LogError($"The list of 'values' received from scaling service is null.");
            return;
        }
        else if (scaledValues.Count < Constants.TotalRegisters)
        {
            _logger.LogError(
                $"The list of 'values' received from scaling service has {scaledValues.Count} values," +
                $" whereas {Constants.TotalRegisters} values are expected.");
            return;
        }

        for (int i = 0; i < Constants.TotalRegisters; i++)
        {
            _calibratedValues[i] = scaledValues[i]; // TODO: Apply calibration data
        }

        NewValuesReceived?.Invoke(_calibratedValues.ToList());
    }

    public ImmutableDictionary<double, double> GetCalibrationValues(int portIndex)
    {
        throw new NotImplementedException();
    }

    public ImmutableDictionary<double, double> SetCalibrationValues(int portIndex, ImmutableDictionary<double, double> calibrationPoints)
    {
        throw new NotImplementedException();
    }
}
