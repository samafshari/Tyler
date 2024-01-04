using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using Net.Essentials;

using System;

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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            routingService.GetWindowFunc = () => desktop.MainWindow;
            routingService.ShowWorldEditor();
        }
        else throw new NotSupportedException("Unsupported application lifetime.");
        //else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        //{
        //    singleViewPlatform.MainView = new MainView
        //    {
        //        DataContext = new MainViewModel()
        //    };
        //}

        base.OnFrameworkInitializationCompleted();
    }
}
