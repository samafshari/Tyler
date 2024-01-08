using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.Models
{
    public class TileAnimation
    {
        public AnimationMode Mode { get; set; }
        public SpriteKeyFrame[]? KeyFrames { get; set; }
        public double Rate { get; set; } = 1;
        public double InitialTimeVariance { get; set; } = 0;
    }
}
