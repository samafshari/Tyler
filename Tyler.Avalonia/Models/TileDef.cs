using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.Models
{
    public class TileDef
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? BasedOn { get; set; }
        public char Char { get; set; }
        public string? SpriteId { get; set; }
        public string? Script { get; set; }
        public TileAnimation? Animation { get; set; }
    }
}
