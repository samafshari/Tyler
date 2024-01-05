using Avalonia.Media.Imaging;

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
        readonly BitmapCache _bitmapCache;
        readonly RoutingService _routingService;
        readonly SettingsService _settingsService;

        ObservableCollection<SpriteViewModel> _sprites = new ObservableCollection<SpriteViewModel>();
        public ObservableCollection<SpriteViewModel> Sprites
        {
            get => _sprites;
            set => SetProperty(ref _sprites, value);
        }

        string? _path;
        public string? Path
        {
            get => _path;
            set
            {
                SetProperty(ref _path, value);
                RaisePropertyChanged(nameof(FileName));
                RaisePropertyChanged(nameof(Bitmap));
            }
        }

        public string FileName => File.Exists(Path) ? System.IO.Path.GetFileNameWithoutExtension(Path) : $"[FNF] {Path}";

        public Bitmap? Bitmap => _bitmapCache.Get(Path);

        public SpriteSheetViewModel()
        {
            _bitmapCache = ContainerService.Instance.GetOrCreate<BitmapCache>();
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
        }

        public SpriteSheetViewModel(string path) : this()
        {
            Path = path;
            LoadFromFile();
        }

        public async Task SaveToFileAsync()
        {
            var json = JsonConvert.SerializeObject(Sprites.Select(x => x.Serialize()), Formatting.Indented);
            var jsonPath = System.IO.Path.ChangeExtension(Path, Vars.SpriteSheetExtension);
            if (File.Exists(jsonPath) && 
                !await _routingService.ShowConfirmDialogAsync(default, "Confirm Overwrite", $"File {jsonPath} already exists. Are you sure you want to overwrite it?"))
                return;
            File.WriteAllText(jsonPath!, json);
        }

        public void LoadFromFile()
        {
            if (File.Exists(Path))
            {
                _settingsService.Data.LastOpenedPNGPath = Path;
                _settingsService.SaveWithLock();

                var jsonPath = System.IO.Path.ChangeExtension(Path, Vars.SpriteSheetExtension);
                if (File.Exists(jsonPath))
                {
                    var json = File.ReadAllText(jsonPath);
                    Sprites = new ObservableCollection<SpriteViewModel>(
                        JsonConvert.DeserializeObject<List<Sprite>>(json)!
                        .Select(x => new SpriteViewModel(Path, x)));
                }
                else
                {
                    Sprites = new ObservableCollection<SpriteViewModel>();
                }
                UpdateProperties();
            }
        }

        public CommandModel SaveCommand => new CommandModel(SaveToFileAsync);
        public CommandModel LoadCommand => new CommandModel(LoadFromFile);
    }
}
