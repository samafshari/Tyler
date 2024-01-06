using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.Models
{
    public class SpriteSheet
    {
        public string? Id { get; set; }
        public string? Path { get; set; }
        public List<Sprite> Sprites { get; set; } = new List<Sprite>();
    }
}
