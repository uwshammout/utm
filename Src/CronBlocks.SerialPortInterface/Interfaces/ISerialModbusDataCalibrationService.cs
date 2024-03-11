using System.Collections.Immutable;

namespace CronBlocks.SerialPortInterface.Interfaces;

public interface ISerialModbusDataCalibrationService : IDisposable
{
    event Action<List<double>>? NewValuesReceived;
    
    void LoadValuesFromFile();
    void SaveValuesToFile();
    
    void SetInputRange(double rangeMin, double rangeMax);
    ImmutableDictionary<double,double> GetCalibrationValues();
    ImmutableDictionary<double,double> SetCalibrationValues(double input, double output);
}