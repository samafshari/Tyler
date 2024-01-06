using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Reactive;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Threading;

using Net.Essentials;

using System;
using System.Diagnostics;

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
        public int ScrollCursorMargin { get; } = 2;
        public Color ScrollCursorColor { get; } = Colors.White;
        public Color ScrollBarColor { get; } = Colors.Black;
        public Color BoardColor { get; } = Colors.DarkSlateBlue;

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
            var tilePoint = e.GetCurrentPoint(this).Position;
            tilePoint = new Point(tilePoint.X / TileWidth, tilePoint.Y / TileHeight);
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                Select(tilePoint);
                e.Handled = true;
            }
            else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                Draw(tilePoint);
                e.Handled = true;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            var tilePoint = e.GetCurrentPoint(this).Position;
            tilePoint = new Point(tilePoint.X / TileWidth, tilePoint.Y / TileHeight);
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                Select(tilePoint);
                e.Handled = true;
            }
            else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                Draw(tilePoint);
                e.Handled = true;
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

        IImmutableBrush bBoard, bScrollBar, bScrollCursor;

        public override void Render(DrawingContext context)
        {
            if (World == null || Board == null) return;
            bBoard = new SolidColorBrush(BoardColor).ToImmutable();
            bScrollBar = new SolidColorBrush(ScrollBarColor).ToImmutable();
            bScrollCursor = new SolidColorBrush(ScrollCursorColor).ToImmutable();
            Dispatcher.UIThread.Invoke(() => context.Custom(new CustomDrawOp(this, Bounds)));
        }

        public void Render(ImmediateDrawingContext context)
        {
            context.FillRectangle(Brushes.CornflowerBlue, new Rect(0, 0, Bounds.Width, Bounds.Height));

            if (World == null || Board == null) return;

            // Draw board
            var rctBoard = new Rect(0, 0, Board.Width * TileWidth, Board.Height * TileHeight);
            
            context.FillRectangle(bBoard, rctBoard);

            foreach (var tile in Board.Tiles.ToArray())
            {
                if (World.SpriteCharMap.TryGetValue(tile.Char, out var sprite))
                {
                    if (sprite != null)
                    {
                        var bmpSpriteSheet = _bitmapCache.Get(sprite.Path);
                        var destRect = new Rect(tile.X * TileWidth, tile.Y * TileHeight, TileWidth, TileHeight);
                        var srcRect = new Rect(sprite.X, sprite.Y, sprite.Width, sprite.Height);
                        context.DrawBitmap(bmpSpriteSheet, srcRect, destRect);
                    }
                }
            }
            
            // Draw scroll bars
            var rctScrollH = new Rect(0, Bounds.Height - ScrollBarWidth, Bounds.Width - ScrollBarWidth, ScrollBarWidth);
            var rctScrollV = new Rect(Bounds.Width - ScrollBarWidth, 0, ScrollBarWidth, Bounds.Height - ScrollBarWidth);
            context.FillRectangle(bScrollBar, rctScrollH);
            context.FillRectangle(bScrollBar, rctScrollV);

            var lScrollCursorHLength = Math.Min(Bounds.Width, Math.Max(ScrollCursorMinLength, Bounds.Width / rctBoard.Width));
            var lScrollCursorVLength = Math.Min(Bounds.Height, Math.Max(ScrollCursorMinLength, Bounds.Height / rctBoard.Height));

            var rctScrollCursorH = new Rect(
                ScrollCursorMargin, 
                Bounds.Height - ScrollBarWidth + ScrollCursorMargin, 
                lScrollCursorHLength - ScrollCursorMargin * 2, 
                ScrollBarWidth - ScrollCursorMargin * 2);
            var rctScrollCursorV = new Rect(
                Bounds.Width - ScrollBarWidth + ScrollCursorMargin, 
                ScrollCursorMargin, 
                ScrollBarWidth - ScrollCursorMargin * 2, 
                lScrollCursorVLength - ScrollCursorMargin * 2);
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