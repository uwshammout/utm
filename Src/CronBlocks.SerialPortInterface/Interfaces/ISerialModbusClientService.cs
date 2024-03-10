using CronBlocks.SerialPortInterface.Entities;

namespace CronBlocks.SerialPortInterface.Interfaces;

public interface ISerialModbusClientService : IDisposable
{
    event Action<List<double>>? NewValuesReceived;
    event Action<OperationState>? OperationStateChanged;

    void SetComSettings(SerialModbusClientSettings portSettings);
    SerialModbusClientSettings GetComSettings();

    void SetDataAcquisitionInterval(double milliseconds);
    double GetDataAcquisitionInterval();

    OperationState OperationState { get; }

    void StartAcquisition();
    void StopAcquisition();
}