using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Reactive;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Threading;

using Net.Essentials;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Tyler.Services;
using Tyler.ViewModels;

namespace Tyler.Views
{
    public partial class BoardPreviewControl : Control
    {
        static readonly SettingsService _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
        static readonly BitmapCache _bitmapCache = ContainerService.Instance.GetOrCreate<BitmapCache>();

        int w;
        int h;

        public int ScrollBarWidth { get; } = 32;
        public int ScrollCursorMinLength { get; } = 32;
        public int ScrollCursorMargin { get; } = 4;
        public Color ScrollCursorColor { get; } = Colors.White;
        public Color ScrollBarColor { get; } = Colors.DarkSlateGray;
        public Color BoardColor { get; } = Colors.DarkSlateBlue;
        public Color GridColor { get; } = Colors.DarkGray;
        public double ScrollX { get; set; }
        public double ScrollY { get; set; }

        public BoardViewModel? Board
        {
            get => (BoardViewModel?)GetValue(BoardProperty);
            set => SetValue(BoardProperty, value);
        }

        public static readonly AvaloniaProperty<BoardViewModel?> BoardProperty =
            AvaloniaProperty.Register<BoardPreviewControl, BoardViewModel?>(nameof(Board));

        public WorldViewModel? World
        {
            get => (WorldViewModel?)GetValue(WorldProperty);
            set => SetValue(WorldProperty, value);
        }

        public static readonly AvaloniaProperty<WorldViewModel?> WorldProperty =
            AvaloniaProperty.Register<BoardPreviewControl, WorldViewModel?>(nameof(World));

        public TileViewModel? SelectedTile
        {
            get => (TileViewModel?)GetValue(SelectedTileProperty);
            set => SetValue(SelectedTileProperty, value);
        }

        public static readonly AvaloniaProperty<TileViewModel?> SelectedTileProperty =
            AvaloniaProperty.Register<BoardPreviewControl, TileViewModel?>(nameof(SelectedTile));

        public int TileWidth
        {
            get => (int)GetValue(TileWidthProperty)!;
            set => SetValue(TileWidthProperty, value);
        }

        public static readonly AvaloniaProperty<int> TileWidthProperty =
            AvaloniaProperty.Register<BoardPreviewControl, int>(nameof(TileWidth));

        public int TileHeight
        {
            get => (int)GetValue(TileHeightProperty)!;
            set => SetValue(TileHeightProperty, value);
        }

        public static readonly AvaloniaProperty<int> TileHeightProperty =
            AvaloniaProperty.Register<BoardPreviewControl, int>(nameof(TileHeight));

        int _oldState;
        public int State
        {
            get => (int)GetValue(StateProperty)!;
            set => SetValue(StateProperty, value);
        }

        public static readonly AvaloniaProperty<int> StateProperty =
            AvaloniaProperty.Register<BoardPreviewControl, int>(nameof(State));

        public BoardPreviewControl()
        {
            InitializeComponent();
            ClipToBounds = true;
            RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
            this.GetObservable(BoardProperty).Subscribe(new AnonymousObserver<BoardViewModel?>(s => OnBoardChanged()));
            this.GetObservable(WorldProperty).Subscribe(new AnonymousObserver<WorldViewModel?>(s => OnBoardChanged()));
            this.GetObservable(SelectedTileProperty).Subscribe(new AnonymousObserver<TileViewModel?>(s => OnSelectionChanged()));
            this.GetObservable(TileWidthProperty).Subscribe(new AnonymousObserver<int>(s => OnBoardChanged()));
            this.GetObservable(TileHeightProperty).Subscribe(new AnonymousObserver<int>(s => OnBoardChanged()));
            this.GetObservable(StateProperty).Subscribe(new AnonymousObserver<int>(s => OnStateChanged()));
        }

        void OnStateChanged()
        {
            if (_oldState != State)
            {
                _oldState = State;
                Update();
            }
        }

        void OnBoardChanged()
        {
            Update();
        }

        void OnSelectionChanged()
        {
            Update();
        }

        public void Update()
        {
            if (Board == null || World == null) return;
            InvalidateVisual();
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (HandlePointerInput(e.GetCurrentPoint(this).Position, e.GetCurrentPoint(this).Properties))
                e.Handled = true;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (HandlePointerInput(e.GetCurrentPoint(this).Position, e.GetCurrentPoint(this).Properties))
                e.Handled = true;
        }

        bool HandlePointerInput(Point position, PointerPointProperties properties)
        {
            var tilePoint = position;

            if ((properties.IsLeftButtonPressed || properties.IsRightButtonPressed) &&
                !(position.X > Bounds.Width - ScrollBarWidth && position.Y > Bounds.Height - ScrollBarWidth))
            {
                if (position.X > Bounds.Width - ScrollBarWidth)
                {
                    ScrollY = position.Y / Bounds.Height;
                    InvalidateMeasure();
                    return true;
                }
                if (position.Y > Bounds.Height - ScrollBarWidth)
                {
                    ScrollX = position.X / Bounds.Width;
                    InvalidateMeasure();
                    return true;
                }
            }

            if (Board == null) return false;
            var xOffset = (ScrollX * (double)(Board.Width * TileWidth - Bounds.Width));
            var yOffset = (ScrollY * (double)(Board.Height * TileHeight - Bounds.Height));
            tilePoint = new Point((xOffset + tilePoint.X) / TileWidth, (yOffset + tilePoint.Y) / TileHeight);
            if (properties.IsLeftButtonPressed)
            {
                Select(tilePoint);
                return true;
            }
            else if (properties.IsRightButtonPressed)
            {
                Draw(tilePoint);
                return true;
            }
            return false;
        }

        void Select(Point p)
        {
            World?.SelectTile((int)p.X, (int)p.Y);
        }

        void Draw(Point p)
        {
            if (World != null && Board != null && World.SelectedSprite != null)
            {
                if (p.X < 0 || p.Y < 0 || p.X >= Board.Width || p.Y >= Board.Height) return;
                Board.SetTile((int)p.X, (int)p.Y, 0, World.SelectedSprite.RealChar);
                InvalidateVisual();
            }
        }

        IImmutableBrush bBoard, bScrollBar, bScrollCursor, bBackground;
        ImmutablePen pGrid;

        public override void Render(DrawingContext context)
        {
            if (World == null || Board == null) return;
            bBoard = new SolidColorBrush(BoardColor).ToImmutable();
            bScrollBar = new SolidColorBrush(ScrollBarColor).ToImmutable();
            bScrollCursor = new SolidColorBrush(ScrollCursorColor).ToImmutable();
            var bGrid = new SolidColorBrush(GridColor).ToImmutable();
            pGrid = new Pen(bGrid, 1).ToImmutable();
            if (!Color.TryParse(_settingsService.Data.BackgroundColor, out var cBackground))
                cBackground = Colors.Black;
            bBackground = new SolidColorBrush(cBackground).ToImmutable();
            Dispatcher.UIThread.Invoke(() => context.Custom(new CustomDrawOp(this, Bounds)));
        }

        public void Render(ImmediateDrawingContext context)
        {
            context.FillRectangle(bBoard, new Rect(0, 0, Bounds.Width, Bounds.Height));

            if (World == null || Board == null) return;

            // Draw board
            var rctBoard = new Rect(0, 0, Board.Width * TileWidth, Board.Height * TileHeight);
            context.FillRectangle(bBackground, rctBoard);

            // Draw grid
            for (int x = 0; x < Board.Width; x++)
                context.DrawLine(pGrid, new Point(x * TileWidth, 0), new Point(x * TileWidth, Board.Height * TileHeight));

            // Draw tiles
            void DrawTile(TileViewModel? tile)
            {
                if (tile == null) return;
                if (World!.SpriteCharMap.TryGetValue(tile.Char, out var sprite))
                {
                    if (sprite != null)
                    {
                        var bmpSpriteSheet = _bitmapCache.Get(sprite.Path);
                        var xOffset = -(ScrollX * (rctBoard.Width - Bounds.Width));
                        var yOffset = -(ScrollY * (rctBoard.Height - Bounds.Height));
                        var destRect = new Rect(xOffset + tile.X * TileWidth, yOffset + tile.Y * TileHeight, TileWidth, TileHeight);
                        var srcRect = new Rect(sprite.X, sprite.Y, sprite.Width, sprite.Height);
                        context.DrawBitmap(bmpSpriteSheet, srcRect, destRect);
                    }
                }
            }
            //    Parallel.ForEach(Board.Tiles.ToArray(), DrawTile);
            foreach (var tile in Board.Tiles.ToArray()) DrawTile(tile);

            // Draw scroll bars
            var rctScrollH = new Rect(0, Bounds.Height - ScrollBarWidth, Bounds.Width - ScrollBarWidth, ScrollBarWidth);
            var rctScrollV = new Rect(Bounds.Width - ScrollBarWidth, 0, ScrollBarWidth, Bounds.Height - ScrollBarWidth);
            context.FillRectangle(bScrollBar, rctScrollH);
            context.FillRectangle(bScrollBar, rctScrollV);
            rctScrollH = rctScrollH.Inflate(-ScrollCursorMargin);
            rctScrollV = rctScrollV.Inflate(-ScrollCursorMargin);

            var lScrollCursorHLength = Math.Min(rctScrollH.Width, Math.Max(ScrollCursorMinLength, rctScrollH.Width * Bounds.Width / rctBoard.Width));
            var lScrollCursorVLength = Math.Min(rctScrollV.Height, Math.Max(ScrollCursorMinLength, rctScrollV.Height * Bounds.Height / rctBoard.Height));

            var rctScrollCursorH = new Rect(
                rctScrollH.Left,
                rctScrollH.Top,
                lScrollCursorHLength,
                rctScrollH.Height);
            var rctScrollCursorV = new Rect(
                rctScrollV.Left,
                rctScrollV.Top,
                rctScrollV.Width,
                lScrollCursorVLength);
            rctScrollCursorH = rctScrollCursorH.Translate(new(rctScrollH.Width * ScrollX, 0));
            rctScrollCursorV = rctScrollCursorV.Translate(new(0, rctScrollV.Height * ScrollY));
            var rctScrollCorner = new Rect(
                rctScrollV.Left,
                rctScrollH.Top,
                rctScrollV.Width,
                rctScrollH.Height);
            rctScrollCorner.Inflate(-ScrollCursorMargin);
            context.FillRectangle(bBoard, rctScrollCorner);
            context.FillRectangle(bScrollCursor, rctScrollCursorH);
            context.FillRectangle(bScrollCursor, rctScrollCursorV);
        }

        class CustomDrawOp : ICustomDrawOperation
        {
            readonly BoardPreviewControl _control;

            public Rect Bounds { get; }
            public CustomDrawOp(BoardPreviewControl control, Rect rect)
            {
                _control = control;
                Bounds = rect;
            }

            public void Dispose() { }
            public bool Equals(ICustomDrawOperation? other) => false;
            public bool HitTest(Point p) => true;

            public void Render(ImmediateDrawingContext context)
            {
                _control.Render(context);
            }
        }
    }
}