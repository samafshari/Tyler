using System;

using Net.Essentials;
using System.Linq;
using Tyler.Services;
using Tyler.Views;
using System.Threading.Tasks;

namespace Tyler.ViewModels
{
    public class AutoSliceViewModel : ViewModel
    {
        readonly RoutingService _routingService;
        readonly BitmapCache _cache;

        readonly SpriteSheetEditorViewModel _editor;
        public SpriteSheetEditorViewModel Editor => _editor;

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

        public AutoSliceViewModel()
        {
            _cache = ContainerService.Instance.GetOrCreate<BitmapCache>();
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            _editor = new SpriteSheetEditorViewModel();
        }

        public AutoSliceViewModel(SpriteSheetEditorViewModel editor) : this()
        {
            _editor = editor;
        }

        public async Task SliceByRowsColumnAsync()
        {
            var bmp = _cache.Get(_editor.SpriteSheet?.Path);
            if (bmp == null) return;
            var w = bmp.PixelSize.Width - OffsetLeft - OffsetRight - ((Columns - 1) * XGap);
            var h = bmp.PixelSize.Height - OffsetTop - OffsetBottom - ((Rows - 1) * YGap);
            Width = w / Columns;
            Height = h / Rows;
            await SliceBySizeAsync();
        }

        public async Task SliceBySizeAsync()
        {
            var bmp = _cache.Get(_editor.SpriteSheet?.Path);
            if (bmp == null) return;
            var w = bmp.PixelSize.Width;
            var h = bmp.PixelSize.Height;
            var x = OffsetLeft;
            var y = OffsetTop;

            for (; y + Height <= h; y += Height + YGap)
            {
                for (; x + Width <= w; x += Width + XGap)
                {
                    var sprite = _editor.AddSprite();
                    if (sprite == null) return;
                    sprite.X = x;
                    sprite.Y = y;
                    sprite.Width = Width;
                    sprite.Height = Height;
                }
                x = OffsetLeft;
            }
            await _routingService.ShowDialogAsync(default, "Slice Complete", "Slicing completed.");
        }

        public CommandModel SliceByRowsColumnCommand => new CommandModel(SliceByRowsColumnAsync);
        public CommandModel SliceBySizeCommand => new CommandModel(SliceBySizeAsync);
        public CommandModel ClearSpritesCommand => Editor.ClearSpritesCommand;
    }
}
