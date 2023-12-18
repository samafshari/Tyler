using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Services;

namespace Tyler.ViewModels
{
    public class MainViewModel : ViewModel
    {
        readonly RoutingService _routingService;
        public MainViewModel() { 
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
        }

        public CommandModel ShowSpriteSheetEditorCommand => new CommandModel(() => _routingService.ShowSpriteSheetEditor());
    }
}
