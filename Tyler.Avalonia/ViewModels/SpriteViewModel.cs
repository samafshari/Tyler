using Avalonia;
using System.IO;
using System.Linq;
using Tyler.Models;
using Tyler.Services;
using Net.Essentials;
using Avalonia.Media.Imaging;

namespace Tyler.ViewModels
{
    public class SpriteViewModel : ViewModel
    {
        static readonly BitmapCache _bitmapCache;

        static SpriteViewModel()
        {
            _bitmapCache = ContainerService.Instance.GetOrCreate<BitmapCache>();
        }

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
            set
            {
                SetProperty(ref _x, value);
                RaisePropertyChanged(nameof(Bitmap));
            }
        }

        int _y;
        public int Y
        {
            get => _y;
            set
            {
                SetProperty(ref _y, value);
                RaisePropertyChanged(nameof(Bitmap));
            }
        }

        int _width;
        public int Width
        {
            get => _width;
            set
            {
                SetProperty(ref _width, value);
                RaisePropertyChanged(nameof(Bitmap));
            }
        }

        int _height;
        public int Height
        {
            get => _height;
            set
            {
                SetProperty(ref _height, value);
                RaisePropertyChanged(nameof(Bitmap));
            }
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
            set
            {
                SetProperty(ref _path, value);
                RaisePropertyChanged(nameof(Bitmap));
            }
        }

        public CroppedBitmap? Bitmap => _bitmapCache.Get(Path, X, Y, Width, Height);

        public SpriteViewModel() { }

        public SpriteViewModel(string path, Sprite sprite)
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

        public Size? GetSpriteSheetSize()
        {
            var bmp = _bitmapCache.Get(Path);
            return bmp?.Size;
        }

        public Size GetSpriteSize()
        {
            return new Size(Width, Height);
        }
    }
}
