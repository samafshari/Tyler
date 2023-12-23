using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler
{
    public static class Vars
    {
        public const string SpriteSheetExtension = ".tyler.sheet";
        public const string WorldExtension = ".tyler.world";
        public const string FileDialogTypePNG = "PNG Image";
        public const string FileDialogTypeSheet = "Tyler Sprite Sheet";
        public const string FileDialogTypeWorld = "Tyler World";
        public const char DefaultChar = '.';
        public const char UnassignedChar = ' ';
        public const int StateMax = int.MaxValue - 1;
        public const int StateMin = int.MinValue + 1;

        public static void BumpState(ref int state)
        {
            if (state < StateMax) state++;
            else state = StateMin;
        }
    }
}
