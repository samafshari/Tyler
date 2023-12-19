using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Tyler.Services;

namespace Tyler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        readonly RoutingService _routingService;
        readonly SettingsService _settingsService;

        public App()
        {
            _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
        }
    }
}
