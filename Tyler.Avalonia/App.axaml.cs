using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using Net.Essentials;

using System;
using System.Diagnostics;

using Tyler.Services;
using Tyler.ViewModels;
using Tyler.Views;

namespace Tyler;

public partial class App : Application
{
    readonly RoutingService routingService;

    public App()
    {
        routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
        BenchmarkService.Instance.IsEnabled = true;
        BenchmarkService.Instance.IsDebug = true;
        BenchmarkService.Instance.Echo = true;
        BenchmarkService.Instance.ShouldPrintAfterStops = true;
        BenchmarkService.Instance.AutoPrintInterval = 2000;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);
        ViewModel.RunOnUIAction = Dispatcher.UIThread.Invoke;
        ViewModel.RunOnUITask = Dispatcher.UIThread.InvokeAsync;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            routingService.GetWindowFunc = () => desktop.MainWindow;
            routingService.ShowBenchmarks();
            routingService.ShowWorldEditor(true);
        }
        //else throw new NotSupportedException("Unsupported application lifetime.");
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
