using CronBlocks.SerialPortInterface.Entities;

namespace CronBlocks.SerialPortInterface.Extensions;

public static class EntitiesConversionExtensions
{
    public static string ToDisplayString(this BaudRate baudRate)
    {
        return baudRate.ToString().Replace("_", "");
    }

    public static string ToDisplayString(this DataBits dataBits)
    {
        return dataBits.ToString().Replace("_", "");
    }

    public static string ToDisplayString(this Parity parity)
    {
        return parity.ToString();
    }

    public static string ToDisplayString(this StopBits stopBits)
    {
        switch (stopBits)
        {
            case StopBits.One: return "1";
            case StopBits.Two: return "2";
            case StopBits.OnePointFive: return "1.5";
        }

        throw new NotImplementedException($"Conversion of StopBits for {stopBits} is not implemented");
    }

    public static T FromDisplayString<T>(this string option)
    {
        if (typeof(T).IsEnum &&
            !string.IsNullOrEmpty(option))
        {
            if (typeof(T) == typeof(BaudRate))
            {
                string enumStr = $"_{option}";

                if (Enum.IsDefined(typeof(BaudRate), enumStr))
                {
                    if (Enum.TryParse(typeof(BaudRate), enumStr, out object? baudRate))
                    {
                        return (T)baudRate!;
                    }
                }
            }
            else if (typeof(T) == typeof(DataBits))
            {
                string enumStr = $"_{option}";

                if (Enum.IsDefined(typeof(DataBits), enumStr))
                {
                    if (Enum.TryParse(typeof(DataBits), enumStr, out object? dataBits))
                    {
                        return (T)dataBits!;
                    }
                }
            }
            else if (typeof(T) == typeof(Parity))
            {
                string enumStr = $"{option}";

                if (Enum.IsDefined(typeof(Parity), enumStr))
                {
                    if (Enum.TryParse(typeof(Parity), enumStr, out object? parity))
                    {
                        return (T)parity!;
                    }
                }
            }
            else if (typeof(T) == typeof(StopBits))
            {
                return (option switch
                {
                    "1" => (T)(object)StopBits.One,
                    "2" => (T)(object)StopBits.Two,
                    "1.5" => (T)(object)StopBits.OnePointFive,
                    _ => throw new InvalidOperationException(
                        $"{nameof(FromDisplayString)} cannot convert '{option}' to {typeof(T)}")
                });
            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid use of {nameof(FromDisplayString)} for Enum type {typeof(T)}");
            }
        }

        throw new InvalidOperationException(
                $"{nameof(FromDisplayString)} cannot convert '{option}' to {typeof(T)}");
    }
}
