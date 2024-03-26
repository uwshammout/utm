using CronBlocks.SerialPortInterface.Entities;
using FluentModbus;

namespace CronBlocks.SerialPortInterface.Configuration;

public class Constants
{
    // Time Constants
    internal const double PortDiscoveryIntervalMS = 100;
    public const double DefaultDataAcquisitionIntervalMS = 500;
    public const double MinimumDataAcquisitionIntervalMS = 50;
    public const double MaximumDataAcquisitionIntervalMS = 1500;

    // Device Constants
    internal const ModbusEndianness ModbusEndianness = FluentModbus.ModbusEndianness.BigEndian;
    public const int TotalRegisters = 16;

    // Defaults
    public const int DefaultDeviceAddress = 1;
    public const string DefaultRegistersStartAddressHexStr = "0x0020";

    public const BaudRate DefaultBaudRate = BaudRate._115200;
    public const DataBits DefaultDataBits = DataBits._8;
    public const Parity DefaultParity = Parity.None;
    public const StopBits DefaultStopBits = StopBits.One;
}
