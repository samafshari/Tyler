using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.Models
{
    public class Settings
    {
        public string LastOpenedPNGPath { get; set; }
        public string LastWorldPath { get; set; }
        public string BackgroundColor { get; set; }
        public int TileWidth { get; set; } = 32;
        public int TileHeight { get; set; } = 32;
        public int WorldWidth { get; set; } = 100;
        public int WorldHeight { get; set; } = 100;
        public List<string> Spritesheets { get; set; } = new List<string>();
    }
}
