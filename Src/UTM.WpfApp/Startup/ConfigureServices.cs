using CronBlocks.UTM.InternalServices;
using CronBlocks.UTM.Settings;
using CronBlocks.Helpers;
using CronBlocks.SerialPortInterface.Interfaces;
using CronBlocks.SerialPortInterface.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CronBlocks.UTM.Windows;

namespace CronBlocks.UTM.Startup;

internal static class ConfigureServices
{
    public static void ConfigureAppServices(this IServiceCollection services, App app)
    {
        services
            .AddSingleton(_ => app)
            .AddSingleton(_ => services)
            //- Serial Port Services
            .AddSingleton<ISerialPortsDiscoveryService, SerialPortsDiscoveryService>()
            .AddSingleton<ISerialModbusClientService>(sp =>
            {
                return new SerialModbusClientService(
                    sp.GetRequiredService<ILogger<SerialModbusClientService>>(),
                    new IniConfigIO(
                        FilePaths.ModbusSvcFilename,
                        sp.GetRequiredService<ILogger<IniConfigIO>>()));
            })
            .AddSingleton<ISerialOptionsService, SerialOptionsService>()
            .AddSingleton<ISerialModbusDataScalingService>(sp =>
            {
                return new SerialModbusDataScalingService(
                    sp.GetRequiredService<ILogger<SerialModbusDataScalingService>>(),
                    sp.GetRequiredService<ISerialModbusClientService>(),
                    FilePaths.DataScalingFilename,
                    sp.GetRequiredService<ILogger<IniConfigIO>>());
            })
            .AddSingleton<ISerialModbusDataCalibrationService>(sp =>
            {
                return new SerialModbusDataCalibrationService(
                    sp.GetRequiredService<ISerialModbusDataScalingService>(),
                    sp.GetRequiredService<ILogger<SerialModbusDataCalibrationService>>(),
                    FilePaths.DataCalibrationFilename,
                    sp.GetRequiredService<ILogger<IniConfigIO>>());
            })
            .AddSingleton(sp =>
            {
                return new DataExchangeService(
                    new IniConfigIO(
                        FilePaths.DataExchangeSvcFilename,
                        sp.GetRequiredService<ILogger<IniConfigIO>>()));
            })
            //- Windows
            .AddSingleton<MainWindow>()
            .AddTransient<DeviceConnectionWindow>()
            .AddTransient(sp =>
            {
                return new DeviceCalibrationWindow(
                    sp.GetRequiredService<ISerialModbusClientService>(),
                    sp.GetRequiredService<ISerialModbusDataScalingService>(),
                    sp.GetRequiredService<ISerialModbusDataCalibrationService>(),
                    sp.GetRequiredService<DataExchangeService>(),
                    new IniConfigIO(
                        FilePaths.PasswordFilename,
                        sp.GetRequiredService<ILogger<IniConfigIO>>())
                    );
            })
            .AddTransient<MeasurementSettingsWindow>()
            .AddTransient<AboutWindow>();
    }
}
