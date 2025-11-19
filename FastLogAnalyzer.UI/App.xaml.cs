using System.Windows;
using FastLogAnaluzer.UI;
using FastLogAnalyzer.Core;
using FastLogAnalyzer.Logic;
using Microsoft.Extensions.DependencyInjection;

namespace FastLogAnalyzer.UI;

public partial class App : Application
{
    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; }

    public App()
    {
        try
        {
            Services = ConfigureServices();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error with DI setting: \n{ex}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        //register of ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
        
        //register of parsers
        services.AddTransient<LegacyLogParser>();
        services.AddTransient<FastLogParser>();
        
        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}