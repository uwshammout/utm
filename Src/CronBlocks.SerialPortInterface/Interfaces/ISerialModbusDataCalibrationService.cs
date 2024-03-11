using System.Collections.Immutable;

namespace CronBlocks.SerialPortInterface.Interfaces;

public interface ISerialModbusDataCalibrationService : IDisposable
{
    event Action<List<double>>? NewValuesReceived;
    
    void LoadCalibrationDataFromFile();
    void SaveCalibrationDataToFile();
    
    ImmutableDictionary<double,double> GetCalibrationValues(int dataIndex);
    ImmutableDictionary<double,double> SetCalibrationValue(int dataIndex, int calibrationPoint, double input, double output);
}