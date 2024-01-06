using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Reactive;
using Avalonia.Rendering.SceneGraph;

using Net.Essentials;

using System.Diagnostics;

using Tyler.Services;
using Tyler.ViewModels;

namespace Tyler.Views
{
    public partial class BoardPreviewControl : Control
    {
        static readonly BitmapCache _bitmapCache;

        int w;
        int h;

        static BoardPreviewControl()
        {
            _bitmapCache = ContainerService.Instance.GetOrCreate<BitmapCache>();
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

        //private void SpriteControl_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        //{
        //    if (World != null && e.Source is SpriteControl spriteControl && spriteControl.Tag is Point p)
        //    {
        //        if (e.GetCurrentPoint(spriteControl).Properties.IsLeftButtonPressed)
        //            World.SelectTile((int)p.X, (int)p.Y);
        //        else if (e.GetCurrentPoint(spriteControl).Properties.IsRightButtonPressed)
        //            Draw(p);
        //    }
        //}

        //private void SpriteControl_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        //{
        //    if (e.Source is SpriteControl spriteControl && spriteControl.Tag is Point p)
        //    {
        //        if (e.GetCurrentPoint(spriteControl).Properties.IsLeftButtonPressed)
        //            World?.SelectTile((int)p.X, (int)p.Y);
        //        else if (e.GetCurrentPoint(spriteControl).Properties.IsRightButtonPressed)
        //            Draw(p);
        //    }
        //}

        //void Draw(Point p)
        //{
        //    if (World != null && Board != null && World.SelectedSprite != null)
        //    {
        //        Board.SetTile((int)p.X, (int)p.Y, 0, World.SelectedSprite.RealChar);
        //    }
        //}

        public override void Render(DrawingContext context)
        {
            if (World == null || Board == null) return;

            context.Custom(new CustomDrawOp(this, Bounds));
        }

        public void Render(ImmediateDrawingContext context)
        {
            context.FillRectangle(Brushes.CornflowerBlue, new Rect(0, 0, Bounds.Width, Bounds.Height));
            if (World == null || Board == null) return;
            foreach (var tile in Board.Tiles)
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
            public bool HitTest(Point p) => false;

            public void Render(ImmediateDrawingContext context)
            {
                _control.Render(context);
            }
        }
    }
}