using CronBlocks.Helpers.Extensions;
using CronBlocks.SerialPortInterface.Entities;

namespace CronBlocks.SerialPortInterface.Extensions;

public static class ValidityCheckExtensions
{
    public static bool IsValid(this SerialModbusClientSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ComPort) ||
            settings.DeviceAddress.ToString().IsValidDeviceAddress() == false ||
            settings.RegistersStartAddressHexStr.IsValidHex() == false)
        {
            return false;
        }

        return true;
    }
}
