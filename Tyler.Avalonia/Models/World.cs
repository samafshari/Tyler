﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.Models
{
    public class World
    {
        public List<SpriteSheet> SpriteSheets { get; set; } = new List<SpriteSheet>();
        public List<Board> Boards { get; set; } = new List<Board>();
        public List<TileDef> TileDefs { get; set; } = new List<TileDef>();
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 100;
        public int TileWidth { get; set; } = 32;
        public int TileHeight { get; set; } = 32;
    }
}
