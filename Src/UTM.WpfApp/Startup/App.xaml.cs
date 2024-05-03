using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace CronBlocks.UTM.Startup;

public partial class App : Application
{
    public IHost AppHost { get; private set; }

    public App()
    {
        InitializeComponent();

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.ConfigureAppServices(this))
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost.StartAsync();

        var startupWindow = GetInstance<Windows.MainWindow>();
        startupWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        base.OnExit(e);
    }

    public T GetInstance<T>() where T : class
    {
        return AppHost.Services.GetRequiredService<T>();
    }
}
