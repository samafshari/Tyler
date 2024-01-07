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

        IImmutableBrush? bBoard, bScrollBar, bScrollCursor, bBackground;
        ImmutablePen? pGrid, pGridMajor;
        bool wasLeftPressed = false, wasRightPressed = false;
        bool isVerticalGrabbed = false, isHorizontalGrabbed = false;
        Point grabPoint = new(0, 0);
        double grabValue = 0;

        public int ScrollBarWidth { get; } = 32;
        public int ScrollCursorMinLength { get; } = 32;
        public int ScrollCursorMargin { get; } = 4;
        public Color ScrollCursorColor { get; } = Colors.White;
        public Color ScrollBarColor { get; } = Colors.DarkSlateGray;
        public Color BoardColor { get; } = Colors.DarkSlateBlue;
        public Color GridColor { get; } = Colors.DarkGray;

        double _scrollX = 0, _scrollY = 0;
        public double ScrollX
        {
            get => _scrollX;
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                _scrollX = value;
            }
        }

        public double ScrollY
        {
            get => _scrollY;
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                _scrollY = value;
            }
        }

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

        Rect GetViewportBounds()
        {
            return new Rect(0, 0, Bounds.Width - ScrollBarWidth, Bounds.Height - ScrollBarWidth);
        }

        (Rect Box, Rect Cursor, double WiggleRoom) GetHorizontalScrollBarBounds()
        {
            var viewportBounds = GetViewportBounds();
            var boardWidth = Board != null ? (Board.Width * TileWidth) : Bounds.Width;
            var rctScrollH = new Rect(0, Bounds.Height - ScrollBarWidth, Bounds.Width - ScrollBarWidth, ScrollBarWidth);
            var inflated = rctScrollH.Inflate(-ScrollCursorMargin);
            var lScrollCursorHLength = Math.Min(inflated.Width, Math.Max(ScrollCursorMinLength, inflated.Width * Bounds.Width / boardWidth));
            var rctScrollCursorH = new Rect(
                inflated.Left,
                inflated.Top,
                lScrollCursorHLength,
                inflated.Height);
            var wiggleRoom = rctScrollH.Width - rctScrollCursorH.Width - ScrollCursorMargin * 2;
            if (boardWidth < viewportBounds.Width) ScrollX = 0;
            rctScrollCursorH = rctScrollCursorH.Translate(new Vector(ScrollX * wiggleRoom, 0));
            return (rctScrollH, rctScrollCursorH, wiggleRoom);
        }

        (Rect Box, Rect Cursor, double WiggleRoom) GetVerticalScrollBarBounds()
        {
            var viewportBounds = GetViewportBounds();
            var boardHeight = Board != null ? (Board.Height * TileHeight) : Bounds.Height;
            var rctScrollV = new Rect(Bounds.Width - ScrollBarWidth, 0, ScrollBarWidth, Bounds.Height - ScrollBarWidth);
            var inflated = rctScrollV.Inflate(-ScrollCursorMargin);
            var lScrollCursorVLength = Math.Min(inflated.Height, Math.Max(ScrollCursorMinLength, inflated.Height * Bounds.Height / boardHeight));
            var rctScrollCursorV = new Rect(
                inflated.Left,
                inflated.Top,
                inflated.Width,
                lScrollCursorVLength);
            var wiggleRoom = rctScrollV.Height - rctScrollCursorV.Height - ScrollCursorMargin * 2;
            if (boardHeight < viewportBounds.Height) ScrollY = 0;
            rctScrollCursorV = rctScrollCursorV.Translate(new Vector(0, ScrollY * wiggleRoom));
            return (rctScrollV, rctScrollCursorV, wiggleRoom);
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
            try
            {
                if ((properties.IsLeftButtonPressed || properties.IsRightButtonPressed) &&
                    !(position.X > Bounds.Width - ScrollBarWidth && position.Y > Bounds.Height - ScrollBarWidth))
                {
                    var (rctScrollH, rctScrollCursorH, wiggleRoomH) = GetHorizontalScrollBarBounds();
                    var (rctScrollV, rctScrollCursorV, wiggleRoomV) = GetVerticalScrollBarBounds();

                    if (!wasLeftPressed && properties.IsLeftButtonPressed)
                    {
                        if (rctScrollCursorH.Contains(position))
                        {
                            isHorizontalGrabbed = true;
                            grabPoint = position;
                            grabValue = ScrollX;
                        }
                        else if (rctScrollCursorV.Contains(position))
                        {
                            isVerticalGrabbed = true;
                            grabPoint = position;
                            grabValue = ScrollY;
                        }
                        else if (rctScrollH.Contains(position))
                        {
                            ScrollX = Math.Max(0, Math.Min(1, (position.X - rctScrollCursorH.Width / 2) / wiggleRoomH));
                            InvalidateMeasure();
                            return true;
                        }
                        else if (rctScrollV.Contains(position))
                        {
                            ScrollY = Math.Max(0, Math.Min(1, (position.Y - rctScrollCursorV.Height / 2) / wiggleRoomV));
                            InvalidateMeasure();
                            return true;
                        }
                    }
                    if (isHorizontalGrabbed)
                    {
                        ScrollX = Math.Max(0, Math.Min(1, grabValue + (position.X - grabPoint.X) / wiggleRoomH));
                        InvalidateMeasure();
                        return true;
                    }
                    if (isVerticalGrabbed)
                    {
                        ScrollY = Math.Max(0, Math.Min(1, grabValue + (position.Y - grabPoint.Y) / wiggleRoomV));
                        InvalidateMeasure();
                        return true;
                    }
                }

                if (Board == null) return false;
                var viewportBounds = GetViewportBounds();
                var xOffset = (ScrollX * (double)(Board.Width * TileWidth - viewportBounds.Width));
                var yOffset = (ScrollY * (double)(Board.Height * TileHeight - viewportBounds.Height));
                if (viewportBounds.Width > Board.Width * TileWidth) xOffset = 0;
                if (viewportBounds.Height > Board.Height * TileHeight) yOffset = 0;
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
            catch
            {
                throw;
            }
            finally
            {
                wasLeftPressed = properties.IsLeftButtonPressed;
                wasRightPressed = properties.IsRightButtonPressed;
                if (!wasLeftPressed)
                {
                    isHorizontalGrabbed = false;
                    isVerticalGrabbed = false;
                }
            }
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

        public override void Render(DrawingContext context)
        {
            if (World == null || Board == null) return;
            bBoard = new SolidColorBrush(BoardColor).ToImmutable();
            bScrollBar = new SolidColorBrush(ScrollBarColor).ToImmutable();
            bScrollCursor = new SolidColorBrush(ScrollCursorColor).ToImmutable();
            pGrid = new Pen(new SolidColorBrush(Color.Parse("#111111")).ToImmutable(), 1).ToImmutable();
            pGridMajor = new Pen(new SolidColorBrush(Color.Parse("#222222")).ToImmutable(), 1).ToImmutable();
            if (!Color.TryParse(_settingsService.Data.BackgroundColor, out var cBackground))
                cBackground = Colors.Black;
            bBackground = new SolidColorBrush(cBackground).ToImmutable();
            Dispatcher.UIThread.Invoke(() => context.Custom(new CustomDrawOp(this, Bounds)));
        }

        public void Render(ImmediateDrawingContext context)
        {
            context.FillRectangle(bBoard!, new Rect(0, 0, Bounds.Width, Bounds.Height));

            if (World == null || Board == null) return;

            // Draw board
            var rctBoard = new Rect(0, 0, Board.Width * TileWidth, Board.Height * TileHeight);
            context.FillRectangle(bBackground!, rctBoard);

            var viewportBounds = GetViewportBounds();
            var xOffset = -(ScrollX * (rctBoard.Width - Bounds.Width));
            var yOffset = -(ScrollY * (rctBoard.Height - Bounds.Height));
            if (rctBoard.Width < viewportBounds.Width) xOffset = 0;
            if (rctBoard.Height < viewportBounds.Height) yOffset = 0;

            // Draw grid
            for (int x = 0; x < Board.Width; x++)
                context.DrawLine(x % 10 == 0 ? pGridMajor! : pGrid!, new Point(xOffset + x * TileWidth, 0), new Point(xOffset + x * TileWidth, Board.Height * TileHeight));
            for (int y = 0; y < Board.Height; y++)
                context.DrawLine(y % 10 == 0 ? pGridMajor! : pGrid!, new Point(0, yOffset + y * TileHeight), new Point(Board.Width * TileWidth, yOffset + y * TileHeight));

            // Draw tiles
            void DrawTile(TileViewModel? tile)
            {
                if (tile == null) return;
                if (World!.SpriteCharMap.TryGetValue(tile.Char, out var sprite))
                {
                    if (sprite != null)
                    {
                        var bmpSpriteSheet = _bitmapCache.Get(sprite.Path);
                        var destRect = new Rect(xOffset + tile.X * TileWidth, yOffset + tile.Y * TileHeight, TileWidth, TileHeight);
                        var srcRect = new Rect(sprite.X, sprite.Y, sprite.Width, sprite.Height);
                        context.DrawBitmap(bmpSpriteSheet, srcRect, destRect);
                    }
                }
            }
            //    Parallel.ForEach(Board.Tiles.ToArray(), DrawTile);
            foreach (var tile in Board.Tiles.ToArray()) DrawTile(tile);

            // Draw scroll bars
            var (rctScrollH, rctScrollCursorH, _) = GetHorizontalScrollBarBounds();
            var (rctScrollV, rctScrollCursorV, _) = GetVerticalScrollBarBounds();
            context.FillRectangle(bScrollBar!, rctScrollH);
            context.FillRectangle(bScrollBar!, rctScrollV);
            var rctScrollCorner = new Rect(
                rctScrollV.Left,
                rctScrollH.Top,
                rctScrollV.Width,
                rctScrollH.Height);
            rctScrollCorner.Inflate(-ScrollCursorMargin);
            context.FillRectangle(bBoard!, rctScrollCorner);
            context.FillRectangle(bScrollCursor!, rctScrollCursorH);
            context.FillRectangle(bScrollCursor!, rctScrollCursorV);
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