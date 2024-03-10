using System.IO;

namespace CronBlocks.UTM.Settings;

internal static class FilePaths
{
    public static readonly string FilesDirectory =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}" +
        $"{Path.DirectorySeparatorChar}CronBlocks" +
        $"{Path.DirectorySeparatorChar}UTM" +
        $"{Path.DirectorySeparatorChar}";

    public static readonly string PasswordFilename = $"{FilesDirectory}Pwd.ini";
    public static readonly string ModbusSvcFilename = $"{FilesDirectory}Modbus.ini";
    public static readonly string DataScalingFilename = $"{FilesDirectory}ScalingFactors.ini";
    public static readonly string DataExchangeSvcFilename = $"{FilesDirectory}CommonData.ini";
    public static readonly string CsvDumpFilename = $"{FilesDirectory}temp.csv";
}
