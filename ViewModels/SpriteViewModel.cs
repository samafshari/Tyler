using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;

namespace Tyler.ViewModels
{
    public class SpriteViewModel : ViewModel
    {
        public string DisplayName => ToString();

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

        char _char = Vars.DefaultChar;
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

        string _path;
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public SpriteViewModel()
        {
        }

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
    }
}
