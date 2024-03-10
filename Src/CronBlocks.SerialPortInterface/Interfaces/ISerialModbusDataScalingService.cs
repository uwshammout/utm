using System.Collections.Immutable;

namespace CronBlocks.SerialPortInterface.Interfaces;

public interface ISerialModbusDataScalingService : IDisposable
{
    event Action<List<double>>? NewValuesReceived;
    
    void LoadValuesFromFile();
    void SaveValuesToFile();
    
    ImmutableList<double> GetMultiplicationFactors();
    ImmutableList<double> GetOffsets();

    void SetMultiplicationFactor(int index, double value);
    void SetOffset(int index, double value);
}