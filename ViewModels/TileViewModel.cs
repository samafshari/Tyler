using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;

namespace Tyler.ViewModels
{
    public class TileViewModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public char Char { get; set; }

        public TileViewModel(Tile model)
        {
            X = model.X;
            Y = model.Y;
            Z = model.Z;
            Char = model.Char;
        }

        public Tile Serialize()
        {
            return new Tile
            {
                X = X,
                Y = Y,
                Z = Z,
                Char = Char
            };
        }
    }
}
