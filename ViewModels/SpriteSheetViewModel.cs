using Net.Essentials;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class SpriteSheetViewModel : ViewModel
    {
        readonly RoutingService _routingService;

        ObservableCollection<SpriteViewModel> _sprites = new ObservableCollection<SpriteViewModel>();
        public ObservableCollection<SpriteViewModel> Sprites
        {
            get => _sprites;
            set => SetProperty(ref _sprites, value);
        }

        string _path;
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public SpriteSheetViewModel()
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
        }

        public void SaveToFile()
        {
            var json = JsonConvert.SerializeObject(Sprites.ReturnAsList<Sprite>(), Formatting.Indented);
            var jsonPath = System.IO.Path.ChangeExtension(Path, ".tyler.sheet");
            if (File.Exists(jsonPath) && 
                !_routingService.ShowConfirmDialog("Confirm Overwrite", $"File {jsonPath} already exists. Are you sure you want to overwrite it?"))
                return;
            File.WriteAllText(jsonPath, json);
        }

        public void LoadFromFile()
        {
            if (File.Exists(Path))
            {
                var jsonPath = System.IO.Path.ChangeExtension(Path, ".tyler.sheet");
                if (File.Exists(jsonPath))
                {
                    var json = File.ReadAllText(jsonPath);
                    Sprites = new ObservableCollection<SpriteViewModel>(
                        JsonConvert.DeserializeObject<List<Sprite>>(json)
                        .Select(x => new SpriteViewModel(x)));
                }
                else
                {
                    Sprites = new ObservableCollection<SpriteViewModel>();
                }
                UpdateProperties();
            }
        }

        public CommandModel SaveCommand => new CommandModel(SaveToFile);
        public CommandModel LoadCommand => new CommandModel(LoadFromFile);
    }
}
