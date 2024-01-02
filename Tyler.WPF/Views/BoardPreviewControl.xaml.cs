using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Tyler.Models;
using Tyler.ViewModels;
using System.Linq;

namespace Tyler.Views
{
    /// <summary>
    /// Interaction logic for BoardPreviewControl.xaml
    /// </summary>
    public partial class BoardPreviewControl : UserControl
    {
        readonly BitmapCache _bitmapCache;

        readonly Dictionary<(int, int), SpriteControl> spriteControls = new Dictionary<(int, int), SpriteControl>();
        readonly Dictionary<(int, int), Image> scriptIcons = new Dictionary<(int, int), Image>();
        SelectionRectangle selectionRectangle;
        Image previewImg;

        public BoardViewModel Board
        {
            get => (BoardViewModel)GetValue(BoardProperty);
            set => SetValue(BoardProperty, value);
        }

        public static readonly DependencyProperty BoardProperty =
            DependencyProperty.Register("Board", typeof(BoardViewModel), typeof(BoardPreviewControl), new PropertyMetadata(OnBoardPropertyChanged));

        public WorldViewModel World
        {
            get => (WorldViewModel)GetValue(WorldProperty);
            set => SetValue(WorldProperty, value);
        }

        public static readonly DependencyProperty WorldProperty =
            DependencyProperty.Register("World", typeof(WorldViewModel), typeof(BoardPreviewControl), new PropertyMetadata(OnBoardPropertyChanged));

        public TileViewModel SelectedTile
        {
            get => (TileViewModel)GetValue(SelectedTileProperty);
            set => SetValue(SelectedTileProperty, value);
        }

        public static readonly DependencyProperty SelectedTileProperty =
            DependencyProperty.Register("SelectedTile", typeof(TileViewModel), typeof(BoardPreviewControl), new PropertyMetadata(OnSelectionChanged));

        public int TileWidth
        {
            get => (int)GetValue(TileWidthProperty);
            set => SetValue(TileWidthProperty, value);
        }

        public static readonly DependencyProperty TileWidthProperty =
            DependencyProperty.Register("TileWidth", typeof(int), typeof(BoardPreviewControl), new PropertyMetadata(OnBoardPropertyChanged));

        public int TileHeight
        {
            get => (int)GetValue(TileHeightProperty);
            set => SetValue(TileHeightProperty, value);
        }

        public static readonly DependencyProperty TileHeightProperty =
            DependencyProperty.Register("TileHeight", typeof(int), typeof(BoardPreviewControl), new PropertyMetadata(OnBoardPropertyChanged));

        int _oldState;
        public int State
        {
            get => (int)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(int), typeof(BoardPreviewControl), new PropertyMetadata(OnStateChanged));

        public BoardPreviewControl()
        {
            _bitmapCache = ContainerService.Instance.GetOrCreate<BitmapCache>();
            InitializeComponent();
            previewImg = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            RenderOptions.SetBitmapScalingMode(previewImg, BitmapScalingMode.NearestNeighbor);
            grd.Children.Add(previewImg);
            grd.DataContext = this;
        }

        static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BoardPreviewControl control)
            {
                if (control._oldState != control.State)
                {
                    control._oldState = control.State;
                    control.Update();
                }
            }
        }
        static void OnBoardPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BoardPreviewControl control) control.Update();
        }

        static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BoardPreviewControl control) control.Update();
        }

        int w;
        int h;
        public void Update()
        {
            if (Board == null) return;
            if (Board.Width != w || Board.Height != h)
                RebuildGrid();

            foreach (var sc in spriteControls)
            {
                bool found = false;
                if (Board.TileGrid.TryGetValue(sc.Key, out var tile))
                {
                    scriptIcons[sc.Key].Visibility = tile.ScriptIconVisibility;
                    if (tile == null)
                        sc.Value.Sprite = null;
                    else if (World.SpriteMap.TryGetValue(tile.Char, out var sprite))
                        sc.Value.Sprite = sprite;
                    found = true;
                }
                if (!found) sc.Value.Sprite = null;
            }

            if (SelectedTile == null) 
                selectionRectangle.Visibility = Visibility.Hidden;
            else
            {
                selectionRectangle.Visibility = Visibility.Visible;
                selectionRectangle.Margin = new Thickness(
                    SelectedTile.X * World.TileWidth,
                    SelectedTile.Y * World.TileHeight,
                    0,
                    0);
            }
        }

        void RebuildGrid()
        {
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
                    spriteControl.MouseMove += SpriteControl_MouseMove;
                    spriteControl.MouseDown += SpriteControl_MouseDown;

                    var img = new Image
                    {
                        Style = (Style)FindResource("ScriptIcon")
                    };
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
                selectionRectangle = new SelectionRectangle();
                selectionRectangle.Width = World.TileWidth;
                selectionRectangle.Height = World.TileHeight;
                selectionRectangle.HorizontalAlignment = HorizontalAlignment.Left;
                selectionRectangle.VerticalAlignment = VerticalAlignment.Top;
                grd.Children.Add(selectionRectangle);
            }
        }

        private void SpriteControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is SpriteControl spriteControl && spriteControl.Tag is Point p)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    World.SelectTile((int)p.X, (int)p.Y);
                else if (e.RightButton == MouseButtonState.Pressed)
                    Draw(p);
            }
        }

        private void SpriteControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                if (e.Source is SpriteControl spriteControl && spriteControl.Tag is Point p)
                {
                    Draw(p);
                }
        }

        void Draw(Point p)
        {
            if (World.SelectedSprite != null)
            {
                Board.SetTile((int)p.X, (int)p.Y, 0, World.SelectedSprite.RealChar);
            }
        }

        void Render()
        {
            if (Board == null || World == null)
            {
                previewImg.Source = null;
                return;
            }

            PresentationSource source = PresentationSource.FromVisual(this);
            var dpiX = 96.0;// * source.CompositionTarget.TransformToDevice.M11;
            var dpiY = 96.0;// * source.CompositionTarget.TransformToDevice.M22;
            var upscale = 1;
            var rt = new RenderTargetBitmap(
                upscale * Board.Width * World.TileWidth,
                upscale * Board.Height * World.TileHeight,
                dpiX,
                dpiY,
                PixelFormats.Pbgra32);
            previewImg.Width = Board.Width * World.TileWidth;
            previewImg.Height = Board.Height * World.TileHeight;
            var drawingVisual = new DrawingVisual();
            RenderOptions.SetBitmapScalingMode(drawingVisual, BitmapScalingMode.NearestNeighbor);

            using var context = drawingVisual.RenderOpen();
            for (int r = 0; r < Board.Height; r++)
            {
                for (int c = 0; c < Board.Width; c++)
                {
                    if (Board.TileGrid.TryGetValue((c, r), out var tile))
                    {
                        if (World.SpriteMap.TryGetValue(tile.Char, out var sprite))
                        {
                            var img = _bitmapCache.Get(sprite.Path, sprite.X, sprite.Y, sprite.Width, sprite.Height);

                            context.DrawImage(img, new Rect(
                                upscale * c * World.TileWidth,
                                upscale * r * World.TileHeight,
                                upscale * World.TileWidth,
                                upscale * World.TileHeight));
                        }
                    }
                }
            }
            context.Close();
            rt.Render(drawingVisual);
            RenderOptions.SetBitmapScalingMode(rt, BitmapScalingMode.NearestNeighbor);

            previewImg.Source = rt;
            RenderOptions.SetBitmapScalingMode(previewImg, BitmapScalingMode.NearestNeighbor);
            previewImg.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
        }
    }
}
