using System.Collections.Immutable;

namespace CronBlocks.SerialPortInterface.Interfaces;

public interface ISerialModbusDataCalibrationService : IDisposable
{
    event Action<List<double>>? NewValuesReceived;
    
    void LoadValuesFromFile();
    void SaveValuesToFile();
    
    ImmutableDictionary<double,double> GetCalibrationValues();
    ImmutableDictionary<double,double> SetCalibrationValue(double input, double output);
}