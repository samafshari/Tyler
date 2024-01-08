using Avalonia;
using System.IO;
using System.Linq;
using Tyler.Models;
using Tyler.Services;
using Net.Essentials;
using Avalonia.Media.Imaging;
using System;

namespace Tyler.ViewModels
{
    public class SpriteViewModel : TinyViewModel
    {
        static readonly BitmapCache _bitmapCache;
        public event EventHandler<NameChangeEventArgs>? IdChanged;

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
                var oldId = _id;
                SetProperty(ref _id, value);
                RaisePropertyChanged(nameof(DisplayName));
                if (oldId != value)
                    IdChanged?.Invoke(this, new NameChangeEventArgs(this, oldId, value));
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
                RaisePropertyChanged(nameof(DisplayName));
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
                RaisePropertyChanged(nameof(DisplayName));
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
                RaisePropertyChanged(nameof(SizeString));
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
                RaisePropertyChanged(nameof(SizeString));
            }
        }

        public string SizeString => Width == Height ? $"{Width}" : $"{Width}x{Height}";

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

        public SpriteViewModel(string path, Sprite sprite)
        {
            _id = sprite.Id;
            X = sprite.X;
            Y = sprite.Y;
            Width = sprite.Width;
            Height = sprite.Height;
            Char = sprite.Char.ToString();
            Path = path;
        }

        public Sprite Serialize()
        {
            var sprite = new Sprite
            {
                Id = Id,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                Char = RealChar,
            };
            return sprite;
        }

        public override string ToString()
        {
            if (Bitmap == null)
                return $"⚠ {Id}";
            if (string.IsNullOrWhiteSpace(Id))
                return $"[{X},{Y}]";
            return $"{Id}";
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
