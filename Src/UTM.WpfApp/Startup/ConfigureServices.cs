using CronBlocks.UTM.InternalServices;
using CronBlocks.UTM.Settings;
using CronBlocks.Helpers;
using CronBlocks.SerialPortInterface.Interfaces;
using CronBlocks.SerialPortInterface.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CronBlocks.UTM.Startup;

internal static class ConfigureServices
{
    public static void ConfigureAppServices(this IServiceCollection services, App app)
    {
        services.AddSingleton(_ => app);
        services.AddSingleton(_ => services);

        //- Serial Port Services
        services.AddSingleton<ISerialPortsDiscoveryService, SerialPortsDiscoveryService>();
        services.AddSingleton<ISerialModbusClientService, SerialModbusClientService>((sp) =>
        {
            return new SerialModbusClientService(
                sp.GetRequiredService<ILogger<SerialModbusClientService>>(),
                new IniConfigIO(
                    FilePaths.ModbusSvcFilename,
                    sp.GetRequiredService<ILogger<IniConfigIO>>()));
        });
        services.AddSingleton<ISerialOptionsService, SerialOptionsService>();
        services.AddSingleton<ISerialModbusDataScalingService>((sp) =>
        {
            return new SerialModbusDataScalingService(
                sp.GetRequiredService<ILogger<SerialModbusDataScalingService>>(),
                sp.GetRequiredService<ISerialModbusClientService>(),
                FilePaths.DataScalingFilename,
                sp.GetRequiredService<ILogger<IniConfigIO>>());
        });
        services.AddSingleton<ISerialModbusDataCalibrationService>((sp) =>
        {
            return new SerialModbusDataCalibrationService(
                sp.GetRequiredService<ISerialModbusDataScalingService>(),
                sp.GetRequiredService<ILogger<SerialModbusDataCalibrationService>>(),
                FilePaths.DataCalibrationFilename,
                sp.GetRequiredService<ILogger<IniConfigIO>>());
        });
        services.AddSingleton((sp) =>
        {
            return new DataExchangeService(
                new IniConfigIO(
                    FilePaths.DataExchangeSvcFilename,
                    sp.GetRequiredService<ILogger<IniConfigIO>>()));
        });

        //- Windows
        services.AddSingleton<Windows.MainWindow>();
        services.AddTransient<Windows.DeviceConnectionWindow>();
        services.AddTransient<Windows.DeviceCalibrationWindow>((sp) =>
        {
            return new Windows.DeviceCalibrationWindow(
                sp.GetRequiredService<ISerialModbusClientService>(),
                sp.GetRequiredService<ISerialModbusDataScalingService>(),
                sp.GetRequiredService<ISerialModbusDataCalibrationService>(),
                sp.GetRequiredService<DataExchangeService>(),
                new IniConfigIO(
                    FilePaths.PasswordFilename,
                    sp.GetRequiredService<ILogger<IniConfigIO>>())
                );
        });
        services.AddTransient<Windows.MeasurementSettingsWindow>();
        services.AddTransient<Windows.AboutWindow>();
    }
}
