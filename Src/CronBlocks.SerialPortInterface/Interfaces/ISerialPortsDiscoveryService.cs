using CronBlocks.SerialPortInterface.Entities;

namespace CronBlocks.SerialPortInterface.Interfaces;

public interface ISerialPortsDiscoveryService
{
    event Action<string>? NewPortFound;
    event Action<string>? ExistingPortRemoved;
    event Action<OperationState>? OperationStateChanged;

    OperationState OperationState { get; }

    void StartPortsDiscovery();
    void StopPortsDiscovery();
}