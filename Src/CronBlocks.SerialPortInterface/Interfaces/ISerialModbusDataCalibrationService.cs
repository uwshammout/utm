using System.Collections.Immutable;

namespace CronBlocks.SerialPortInterface.Interfaces;

public interface ISerialModbusDataCalibrationService : IDisposable
{
	event Action<List<double>>? NewValuesReceived;

	void LoadCalibrationDataFromFile();
	void SaveCalibrationDataToFile();

	ImmutableDictionary<double, double> GetCalibrationValues(int portIndex);
	ImmutableDictionary<double, double> SetCalibrationValues(int portIndex, ImmutableDictionary<double, double> calibrationPoints);
}