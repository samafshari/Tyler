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
        readonly Dictionary<(int, int), SpriteControl> spriteControls = new Dictionary<(int, int), SpriteControl>();
        readonly Dictionary<(int, int), Image> scriptIcons = new Dictionary<(int, int), Image>();
        SelectionRectangle selectionRectangle;

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
            DependencyProperty.Register("SelectedTile", typeof(TileViewModel), typeof(BoardPreviewControl), new PropertyMetadata(null));

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
            InitializeComponent();
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

        public void Update()
        {
            if (Board == null) return;
            if (Board.Width != grd.ColumnDefinitions.Count || Board.Height != grd.ColumnDefinitions.Count)
                RebuildGrid();
            
            foreach (var sc in spriteControls)
            {
                bool found = false;
                if (Board.TileGrid.TryGetValue(sc.Key, out var tile))
                {
                    scriptIcons[sc.Key].DataContext = tile;
                    if (tile == null)
                        sc.Value.Sprite = null;
                    else if (World.SpriteMap.TryGetValue(tile.Char, out var sprite))
                        sc.Value.Sprite = sprite;
                    found = true;
                }
                if (!found) sc.Value.Sprite = null;
            }
        }

        void RebuildGrid()
        {
            grd.Children.Clear();
            grd.ColumnDefinitions.Clear();
            grd.RowDefinitions.Clear();
            selectionRectangle = new SelectionRectangle();
            selectionRectangle.DataContext = this;
            selectionRectangle.SetBinding(Grid.RowProperty, "SelectedTile.Y");
            selectionRectangle.SetBinding(Grid.ColumnProperty, "SelectedTile.X");
            for (int r = 0; r < Board.Height; r++)
            {
                var rd = new RowDefinition();
                rd.SetBinding(RowDefinition.HeightProperty, "TileHeight");
                rd.DataContext = this;
                grd.RowDefinitions.Add(rd);
            }
            for (int c = 0; c < Board.Width; c++)
            {
                var cd = new ColumnDefinition();
                cd.SetBinding(ColumnDefinition.WidthProperty, "TileWidth");
                cd.DataContext = this;
                grd.ColumnDefinitions.Add(cd);
            }

            spriteControls.Clear();
            scriptIcons.Clear();
            for (int r = 0; r < Board.Height; r++)
            {
                for (int c = 0; c < Board.Width; c++)
                {
                    var spriteControl = new SpriteControl();
                    grd.Children.Add(spriteControl);
                    spriteControls[(c, r)] = spriteControl;
                    spriteControl.SetValue(Grid.RowProperty, r);
                    spriteControl.SetValue(Grid.ColumnProperty, c);
                    spriteControl.Tag = new Point(c, r);
                    spriteControl.MouseMove += SpriteControl_MouseMove;
                    spriteControl.MouseDown += SpriteControl_MouseDown;

                    var img = new Image
                    {
                        Style = (Style)FindResource("ScriptIcon")
                    };
                    grd.Children.Add(img);
                    img.SetValue(Grid.RowProperty, r);
                    img.SetValue(Grid.ColumnProperty, c);
                    img.SetBinding(Image.VisibilityProperty, "ScriptIconVisibility");
                    img.Tag = spriteControl.Tag;
                    scriptIcons[(c, r)] = img;
                }
            }

            grd.Children.Add(selectionRectangle);
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
    }
}
