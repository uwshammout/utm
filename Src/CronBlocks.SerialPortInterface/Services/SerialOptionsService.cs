using CronBlocks.SerialPortInterface.Entities;
using CronBlocks.SerialPortInterface.Extensions;
using CronBlocks.SerialPortInterface.Interfaces;

namespace CronBlocks.SerialPortInterface.Services;

public class SerialOptionsService : ISerialOptionsService
{
    public IEnumerable<string> GetAllOptions<T>()
    {
        if (typeof(T).IsEnum)
        {
            if (typeof(T) == typeof(BaudRate))
            {
                foreach (BaudRate baudRate in Enum.GetValues(typeof(BaudRate)))
                {
                    yield return baudRate.ToDisplayString();
                }
            }
            else if (typeof(T) == typeof(DataBits))
            {
                foreach (DataBits dataBits in Enum.GetValues(typeof(DataBits)))
                {
                    yield return dataBits.ToDisplayString();
                }
            }
            else if (typeof(T) == typeof(Parity))
            {
                foreach (Parity parity in Enum.GetValues(typeof(Parity)))
                {
                    yield return parity.ToDisplayString();
                }
            }
            else if (typeof(T) == typeof(StopBits))
            {
                foreach (StopBits stopBits in Enum.GetValues(typeof(StopBits)))
                {
                    yield return stopBits.ToDisplayString();
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid use of {nameof(GetAllOptions)} for Enum type {typeof(T)}");
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"{nameof(GetAllOptions)} is only valid for Enum types");
        }
    }

    public T ConvertOption<T>(string option)
    {
        try
        {
            T t = option.FromDisplayString<T>();
            return t;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"{nameof(ConvertOption)} cannot convert '{option}' to {typeof(T)}", ex);
        }
    }
}
