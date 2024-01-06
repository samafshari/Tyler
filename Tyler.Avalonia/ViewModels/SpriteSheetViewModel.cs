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
        static readonly BitmapCache _bitmapCache;

        static SpriteSheetViewModel()
        {
            _bitmapCache = ContainerService.Instance.GetOrCreate<BitmapCache>();
        }

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
                RaisePropertyChanged(nameof(DisplayName));
                RaisePropertyChanged(nameof(Bitmap));
            }
        }

        string _id;
        public string Id
        {
            get => _id;
            set
            {
                SetProperty(ref _id, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        public string DisplayName => ToString();

        public Bitmap? Bitmap => _bitmapCache.Get(Path);

        public SpriteSheetViewModel()
        {
            _id = Guid.NewGuid().ToString();
        }

        public SpriteSheetViewModel(SpriteSheet model) : this()
        {
            Path = model.Path;
            Id = model.Id ?? Guid.NewGuid().ToString();
            Sprites = new ObservableCollection<SpriteViewModel>(model.Sprites.Select(x => new SpriteViewModel(Path, x)));
        }

        public SpriteSheet Serialize()
        {
            var model = new SpriteSheet
            {
                Id = Id,
                Path = Path,
                Sprites = Sprites.Select(x => x.Serialize()).ToList()
            };
            return model;
        }

        public override string ToString()
        {
            return File.Exists(Path) ? Id : $"[FNF] {Id}";
        }
    }
}
