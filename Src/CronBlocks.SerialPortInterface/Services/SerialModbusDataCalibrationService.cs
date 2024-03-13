﻿using CronBlocks.Helpers;
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
	private readonly IniConfigIO _iniConfig;

	private double[] _calibratedValues;
	private List<Dictionary<double, double>> _calibrationData;

	public SerialModbusDataCalibrationService(
			ISerialModbusDataScalingService scalingService,
			ILogger<SerialModbusDataCalibrationService> logger,
			string filename = null!,
			ILogger<IniConfigIO> iniLogger = null!)
	{
		_scalingService = scalingService;
		_logger = logger;
		_filename = filename;
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
			for (int portIndex = 0; portIndex < _calibrationData.Count; portIndex++)
			{
				_calibrationData[portIndex].Clear();

				string section = $"Cal__{portIndex}__";

				for (int pointIndex = 0; pointIndex < _iniConfig.GetInteger($"{section}/Total", 0); pointIndex++)
				{
					double key = _iniConfig.GetDouble($"{section}/Inp__{pointIndex}__", double.NegativeInfinity);
					double value = _iniConfig.GetDouble($"{section}/Out__{pointIndex}__", double.NegativeInfinity);

					if (key != double.NegativeInfinity && value != double.NegativeInfinity)
					{
						_calibrationData[portIndex].Add(key, value);
					}
				}
			}
		}
	}

	public void SaveCalibrationDataToFile()
	{
		if (_iniConfig != null)
		{
			int portIndex = 0;
			foreach (Dictionary<double, double> portCalibrationData in _calibrationData)
			{
				string section = $"Cal__{portIndex}__";
				_iniConfig.SetInteger($"{section}/Total", portCalibrationData.Count);

				int pointIndex = 0;
				foreach (KeyValuePair<double, double> calibrationPoint in portCalibrationData)
				{
					_iniConfig.SetDouble($"{section}/Inp__{pointIndex}__", calibrationPoint.Key);
					_iniConfig.SetDouble($"{section}/Out__{pointIndex}__", calibrationPoint.Value);
					pointIndex++;
				}

				portIndex++;
			}

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
			_calibratedValues[i] = ApplyCalibration(i, scaledValues[i]);
		}

		NewValuesReceived?.Invoke(_calibratedValues.ToList());
	}

	private double ApplyCalibration(int portIndex, double scaledValue)
	{
		lock (this)
		{

		}

		return scaledValue;
	}

	public ImmutableDictionary<double, double> GetCalibrationValues(int portIndex)
	{
		lock (this)
		{
			return _calibrationData[portIndex].ToImmutableDictionary();
		}
	}

	public ImmutableDictionary<double, double> SetCalibrationValues(int portIndex, ImmutableDictionary<double, double> calibrationPoints)
	{
		lock (this)
		{
			_calibrationData[portIndex].Clear();
			if (calibrationPoints != null && calibrationPoints.Count > 0)
			{
				List<double> values = calibrationPoints.Keys.ToList();
				values.Sort();
				foreach (double p in values)
				{
					_calibrationData[portIndex].Add(p, calibrationPoints[p]);
				}
			}
		}

		return _calibrationData[portIndex].ToImmutableDictionary();
	}
}
