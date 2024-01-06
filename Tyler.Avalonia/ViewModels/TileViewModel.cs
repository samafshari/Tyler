using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Tyler.Models;

namespace Tyler.ViewModels
{
    public class TileViewModel : TinyViewModel
    {
        int _x, _y, _z;
        char _char;
        string? _script;

        public int X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        public int Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        public int Z
        {
            get => _z;
            set => SetProperty(ref _z, value);
        }

        public char Char
        {
            get => _char;
            set => SetProperty(ref _char, value);
        }

        public bool IsScriptIconVisible => !string.IsNullOrWhiteSpace(Script);

        public string? Script
        {
            get => _script;
            set
            {
                SetProperty(ref _script, value);
                RaisePropertyChanged(nameof(IsScriptIconVisible));
            }
        }

        public TileViewModel(Tile model)
        {
            _x = model.X;
            _y = model.Y;
            _z = model.Z;
            _char = model.Char;
            _script = model.Script;
        }

        public Tile Serialize()
        {
            return new Tile
            {
                X = X,
                Y = Y,
                Z = Z,
                Char = Char,
                Script = Script
            };
        }
    }
}
