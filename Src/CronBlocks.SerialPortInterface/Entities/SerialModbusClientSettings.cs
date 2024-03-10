using CronBlocks.SerialPortInterface.Configuration;

namespace CronBlocks.SerialPortInterface.Entities;

/// <summary>
/// Class <c>SerialModbusClientSettings</c> represents overall port settings.
/// Including the COM port and the device registers.
/// </summary>
public class SerialModbusClientSettings
{
    public string ComPort { get; set; } = "";
    public int DeviceAddress { get; set; } = Constants.DefaultDeviceAddress;

    public BaudRate BaudRate { get; set; } = Constants.DefaultBaudRate;
    public DataBits DataBits { get; set; } = Constants.DefaultDataBits;
    public Parity Parity { get; set; } = Constants.DefaultParity;
    public StopBits StopBits { get; set; } = Constants.DefaultStopBits;

    public string RegistersStartAddressHexStr { get; set; } = Constants.DefaultRegistersStartAddressHexStr;
}
