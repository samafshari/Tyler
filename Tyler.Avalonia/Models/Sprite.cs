using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.Models
{
    public class Sprite
    {
        public string? Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public char Char { get; set; } = Vars.UnassignedChar;
        public string? SpriteSheetId { get; set; }
    }
}
