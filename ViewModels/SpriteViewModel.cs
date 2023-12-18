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
        string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
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

        public SpriteViewModel()
        {
        }

        public SpriteViewModel(Sprite sprite)
        {
            sprite.Inject(this);
        }
    }
}
