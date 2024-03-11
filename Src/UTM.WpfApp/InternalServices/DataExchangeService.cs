using CronBlocks.Helpers;

namespace CronBlocks.UTM.InternalServices;

public class DataExchangeService : IDisposable
{
    private readonly IniConfigIO _iniConfig;

    public DataExchangeService(IniConfigIO iniConfig)
    {
        _iniConfig = iniConfig;

        LoadConfig();
    }

    private void LoadConfig()
    {
        //- TODO Read from INI file
    }

    private void SaveConfig()
    {
        //- TODO Store values into INI file

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
