using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Layout;
using Avalonia.Reactive;
using Net.Essentials;
using System.Collections.Generic;
using Tyler.Services;
using Tyler.ViewModels;
using System;

namespace Tyler.Views
{
    public partial class BoardPreviewControl : UserControl
    {
        static readonly BitmapCache _bitmapCache;

        static BoardPreviewControl()
        {
            _bitmapCache = ContainerService.Instance.GetOrCreate<BitmapCache>();
        }

        readonly Dictionary<(int, int), SpriteControl> spriteControls = new Dictionary<(int, int), SpriteControl>();
        readonly Dictionary<(int, int), Image> scriptIcons = new Dictionary<(int, int), Image>();
        SelectionRect? selectionRectangle;
        Image previewImg;

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
            previewImg = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            grd.Children.Add(previewImg);
            grd.DataContext = this;
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

        int w;
        int h;
        public void Update()
        {
            if (Board == null || World == null) return;
            if (Board.Width != w || Board.Height != h || selectionRectangle == null)
                RebuildGrid();

            foreach (var sc in spriteControls)
            {
                bool found = false;
                if (Board.TileGrid.TryGetValue(sc.Key, out var tile))
                {
                    scriptIcons[sc.Key].IsVisible = tile.IsScriptIconVisible;
                    if (tile == null)
                        sc.Value.Sprite = null;
                    else if (World.SpriteCharMap.TryGetValue(tile.Char, out var sprite))
                        sc.Value.Sprite = sprite;
                    found = true;
                }
                if (!found) sc.Value.Sprite = null;
            }

            if (selectionRectangle == null)
                throw new Exception("selectionRectangle is null");

            if (SelectedTile == null)
                selectionRectangle.IsVisible = false;
            else
            {
                selectionRectangle.IsVisible = true;
                selectionRectangle.Margin = new Thickness(
                    SelectedTile.X * World.TileWidth,
                    SelectedTile.Y * World.TileHeight,
                    0,
                    0);
            }
        }

        void RebuildGrid()
        {
            if (Board == null || World == null) return;

            w = Board.Width;
            h = Board.Height;
            grd.Children.Clear();
            grd.ColumnDefinitions.Clear();
            grd.RowDefinitions.Clear();

            spriteControls.Clear();
            scriptIcons.Clear();
            for (int r = 0; r < Board.Height; r++)
            {
                for (int c = 0; c < Board.Width; c++)
                {
                    var spriteControl = new SpriteControl();
                    grd.Children.Add(spriteControl);
                    spriteControls[(c, r)] = spriteControl;
                    spriteControl.Margin = new Thickness(
                        c * World.TileWidth,
                        r * World.TileHeight,
                        0,
                        0);
                    spriteControl.Width = World.TileWidth;
                    spriteControl.Height = World.TileHeight;
                    spriteControl.HorizontalAlignment = HorizontalAlignment.Left;
                    spriteControl.VerticalAlignment = VerticalAlignment.Top;

                    spriteControl.Tag = new Point(c, r);
                    spriteControl.PointerMoved += SpriteControl_PointerMoved;
                    spriteControl.PointerPressed += SpriteControl_PointerPressed;

                    var img = new Image();
                    img.Classes.Add("ScriptIcon");
                    grd.Children.Add(img);
                    img.Margin = new Thickness(
                        (c + 0.6) * World.TileWidth,
                        (r + 0.6) * World.TileHeight,
                        0,
                        0);
                    img.Width = World.TileWidth / 2.5;
                    img.Height = World.TileHeight / 2.5;
                    img.HorizontalAlignment = HorizontalAlignment.Left;
                    img.VerticalAlignment = VerticalAlignment.Top;
                    img.Tag = spriteControl.Tag;
                    scriptIcons[(c, r)] = img;
                }
            }

            if (selectionRectangle == null)
            {
                selectionRectangle = new SelectionRect();
                selectionRectangle.Width = World.TileWidth;
                selectionRectangle.Height = World.TileHeight;
                selectionRectangle.HorizontalAlignment = HorizontalAlignment.Left;
                selectionRectangle.VerticalAlignment = VerticalAlignment.Top;
                grd.Children.Add(selectionRectangle);
            }
        }

        private void SpriteControl_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (World != null && e.Source is SpriteControl spriteControl && spriteControl.Tag is Point p)
            {
                if (e.GetCurrentPoint(spriteControl).Properties.IsLeftButtonPressed)
                    World.SelectTile((int)p.X, (int)p.Y);
                else if (e.GetCurrentPoint(spriteControl).Properties.IsRightButtonPressed)
                    Draw(p);
            }
        }

        private void SpriteControl_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (e.Source is SpriteControl spriteControl && spriteControl.Tag is Point p)
            {
                if (e.GetCurrentPoint(spriteControl).Properties.IsLeftButtonPressed)
                    World?.SelectTile((int)p.X, (int)p.Y);
                else if (e.GetCurrentPoint(spriteControl).Properties.IsRightButtonPressed)
                    Draw(p);
            }
        }

        void Draw(Point p)
        {
            if (World != null && Board != null && World.SelectedSprite != null)
            {
                Board.SetTile((int)p.X, (int)p.Y, 0, World.SelectedSprite.RealChar);
            }
        }
    }
}