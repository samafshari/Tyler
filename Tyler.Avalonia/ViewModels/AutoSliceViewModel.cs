using System;

using Net.Essentials;
using System.Linq;
using Tyler.Services;
using Tyler.Views;
using System.Threading.Tasks;
using Tyler.Models;

namespace Tyler.ViewModels
{
    public class AutoSliceViewModel : ViewModel
    {
        readonly RoutingService _routingService;

        readonly SpriteSheetEditorViewModel _editor;
        readonly SpriteSheetViewModel _spriteSheet;

        int _columns = 1;
        public int Columns
        {
            get => _columns;
            set => SetProperty(ref _columns, value);
        }

        int _rows = 1;
        public int Rows
        {
            get => _rows;
            set => SetProperty(ref _rows, value);
        }

        int _width = 32;
        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        int _height = 32;
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        int _xGap = 0;
        public int XGap
        {
            get => _xGap;
            set => SetProperty(ref _xGap, value);
        }

        int _yGap = 0;
        public int YGap
        {
            get => _yGap;
            set => SetProperty(ref _yGap, value);
        }

        int _offsetLeft = 0;
        public int OffsetLeft
        {
            get => _offsetLeft;
            set => SetProperty(ref _offsetLeft, value);
        }

        int _offsetTop = 0;
        public int OffsetTop
        {
            get => _offsetTop;
            set => SetProperty(ref _offsetTop, value);
        }

        int _offsetRight = 0;
        public int OffsetRight
        {
            get => _offsetRight;
            set => SetProperty(ref _offsetRight, value);
        }

        int _offsetBottom = 0;
        public int OffsetBottom
        {
            get => _offsetBottom;
            set => SetProperty(ref _offsetBottom, value);
        }

        public AutoSliceViewModel(SpriteSheetEditorViewModel editor)
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            _editor = editor;
            _spriteSheet = editor.SpriteSheet;
        }

        public void SliceByRowsColumn()
        {
            var bmp = _spriteSheet.Bitmap;
            if (bmp == null) return;
            var w = bmp.PixelSize.Width - OffsetLeft - OffsetRight - ((Columns - 1) * XGap);
            var h = bmp.PixelSize.Height - OffsetTop - OffsetBottom - ((Rows - 1) * YGap);
            Width = w / Columns;
            Height = h / Rows;
            SliceBySize();
        }

        public void  SliceBySize()
        {
            var bmp = _spriteSheet.Bitmap;
            if (bmp == null) return;
            var w = bmp.PixelSize.Width;
            var h = bmp.PixelSize.Height;
            var x = OffsetLeft;
            var y = OffsetTop;

            for (; y + Height <= h; y += Height + YGap)
            {
                for (; x + Width <= w; x += Width + XGap)
                {
                    var sprite = new Sprite
                    {
                        X = x,
                        Y = y,
                        Width = Width,
                        Height = Height
                    };
                    _spriteSheet.AddSprite(sprite);
                }
                x = OffsetLeft;
            }
            _routingService.ShowDialog(default, "Slice Complete", "Slicing completed.");
        }

        public CommandModel SliceByRowsColumnCommand => new CommandModel(SliceByRowsColumn);
        public CommandModel SliceBySizeCommand => new CommandModel(SliceBySize);
        public CommandModel ClearSpritesCommand => _editor.ClearSpritesCommand;
    }
}
