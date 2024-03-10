using CronBlocks.UTM.Settings;
using CronBlocks.Helpers;

namespace CronBlocks.UTM.InternalServices;

public class DataExchangeService : IDisposable
{
    private readonly IniConfigIO _iniConfig;

    /// <summary>
    /// Experiment duration in seconds
    /// </summary>
    public double ExperimentTimeDuration { get; set; }
    /// <summary>
    /// Resistance value in Ohm (Ω)
    /// </summary>
    public double FuelCellCurrentMeasurementResistance { get; set; }
    /// <summary>
    /// Resistance value in Ohm (Ω)
    /// </summary>
    public double ElectrolyzerCurrentMeasurementResistance { get; set; }

    public DataExchangeService(IniConfigIO iniConfig)
    {
        _iniConfig = iniConfig;

        LoadConfig();
    }

    private void LoadConfig()
    {
        ExperimentTimeDuration = _iniConfig.GetDouble(KeyOf(nameof(ExperimentTimeDuration)), ValueConstants.ExperimentTimeMinimumSec + 20);

        FuelCellCurrentMeasurementResistance = _iniConfig.GetDouble(KeyOf(nameof(FuelCellCurrentMeasurementResistance)), ValueConstants.FuelCellCurrentMeasurementResistanceOhm);
        ElectrolyzerCurrentMeasurementResistance = _iniConfig.GetDouble(KeyOf(nameof(ElectrolyzerCurrentMeasurementResistance)), ValueConstants.ElectrolyzerCurrentMeasurementResistanceOhm);
    }

    private void SaveConfig()
    {
        _iniConfig.SetDouble(KeyOf(nameof(ExperimentTimeDuration)), ExperimentTimeDuration);

        _iniConfig.SetDouble(KeyOf(nameof(FuelCellCurrentMeasurementResistance)), FuelCellCurrentMeasurementResistance);
        _iniConfig.SetDouble(KeyOf(nameof(ElectrolyzerCurrentMeasurementResistance)), ElectrolyzerCurrentMeasurementResistance);

        _iniConfig.SaveFile();
    }

    private string KeyOf(string paramName)
    {
        const string sectionName = nameof(DataExchangeService);
        return $"{sectionName}/{paramName}";
    }

    public void Dispose()
    {
        SaveConfig();
    }
}
