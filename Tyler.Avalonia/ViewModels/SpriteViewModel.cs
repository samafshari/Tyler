using Avalonia;
using System.IO;
using System.Linq;
using Tyler.Models;
using Tyler.Services;
using Net.Essentials;

namespace Tyler.ViewModels
{
    public class SpriteViewModel : ViewModel
    {
        readonly BitmapCache _bitmapCache;

        public string DisplayName => ToString();

        string? _id;
        public string? Id
        {
            get => _id;
            set
            {
                SetProperty(ref _id, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        int _x;
        public int X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        int _y;
        public int Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        int _width;
        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        int _height;
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        char _char = Vars.UnassignedChar;
        public string Char
        {
            get => _char.ToString();
            set
            {
                SetProperty(ref _char, value?.LastOrDefault() ?? Vars.DefaultChar);
                RaisePropertyChanged(nameof(DisplayName));
                RaisePropertyChanged(nameof(RealChar));
            }
        }

        public char RealChar => _char;

        string? _path;
        public string? Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public SpriteViewModel()
        {
            _bitmapCache = ContainerService.Instance.GetOrCreate<BitmapCache>();
        }

        public SpriteViewModel(string path, Sprite sprite) : this()
        {
            Path = path;
            sprite.Inject(this);
            Char = sprite.Char.ToString();
        }

        public Sprite Serialize()
        {
            var sprite = this.ReturnAs<Sprite>();
            sprite.Char = RealChar;
            return sprite;
        }

        public override string ToString()
        {
            return $"[{Char}] {Id}";
        }

        public Size? GetImageSize()
        {
            if (!File.Exists(Path)) return default;
            var bmp = _bitmapCache.Get(Path);
            return bmp?.Size;
        }
    }
}
