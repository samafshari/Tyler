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
        public int Width { get; set; } 
        public int Height { get; set; }
        public List<Tile> Tiles { get; set; } = new List<Tile>();
        public string BeforeScript { get; set; }
        public string AfterScript { get; set; }
    }
}
