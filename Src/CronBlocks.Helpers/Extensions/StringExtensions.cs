using System.Text.RegularExpressions;

namespace CronBlocks.Helpers.Extensions;

/// <summary>
/// Provides extension methods for working with string formats.
/// </summary>
public static class StringExtensions
{
    private static Regex hexFormat = new(@"(^0[xX][0-9a-fA-F]+$)|(^[0-9a-fA-F]+$)");
    private static Regex positiveIntFormat = new(@"^\+?[0-9]+$");

    public static bool IsValidHex(this string val)
    {
        return hexFormat.IsMatch(val.Trim());
    }

    public static bool IsValidPositiveInteger(this string val)
    {
        return positiveIntFormat.IsMatch(val.Trim());
    }

    public static bool IsValidDeviceAddress(this string val)
    {
        if (int.TryParse(val.Trim(), out int v) == false || (v < 0 || v > 31))
        {
            return false;
        }

        return true;
    }

    public static bool IsValidMultiplicationFactor(this string val)
    {
        if (double.TryParse(val.Trim(), out double v) == false)
        {
            return false;
        }

        return true;
    }

    public static bool IsValidOffset(this string val)
    {
        if (double.TryParse(val.Trim(), out double v) == false)
        {
            return false;
        }

        return true;
    }

    public static bool IsValidMinValue(this string val)
    {
        if (double.TryParse(val.Trim(), out double v) == false)
        {
            return false;
        }

        return true;
    }

    public static bool IsValidMaxValue(this string val)
    {
        if (double.TryParse(val.Trim(), out double v) == false)
        {
            return false;
        }

        return true;
    }
}
