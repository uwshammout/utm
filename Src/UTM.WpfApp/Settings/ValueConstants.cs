using CronBlocks.Helpers.Extensions;

namespace CronBlocks.UTM.Settings;

internal static class ValueConstants
{
    public readonly static double ExperimentTimeMinimumSec = 5;
    public readonly static double ExperimentTimeMaximumSec = 500;

    public readonly static double FuelCellCurrentMeasurementResistanceOhm = 1;
    public readonly static double ElectrolyzerCurrentMeasurementResistanceOhm = 1;

    public readonly static string DefaultPasswordHash = "admin".Hash();
}
