using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.Models
{
    public class Board
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Tile> Tiles { get; set; } = new List<Tile>();
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 100;
    }
}
