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

using Tyler.ViewModels;

namespace Tyler.Views
{
    /// <summary>
    /// Interaction logic for TilePreviewControl.xaml
    /// </summary>
    public partial class TilePreviewControl : UserControl
    {
        public TileViewModel Tile
        {
            get => (TileViewModel)GetValue(TileProperty);
            set => SetValue(TileProperty, value);
        }

        // Dependency Property for Tile
        public static readonly DependencyProperty TileProperty =
            DependencyProperty.Register("Tile", typeof(TileViewModel), typeof(TilePreviewControl), new PropertyMetadata(OnTilePropertyChanged));

        public WorldViewModel World
        {
            get => (WorldViewModel)GetValue(WorldProperty);
            set => SetValue(WorldProperty, value);
        }

        // Dependency Property for World
        public static readonly DependencyProperty WorldProperty =
            DependencyProperty.Register("World", typeof(WorldViewModel), typeof(TilePreviewControl), new PropertyMetadata(OnTilePropertyChanged));

        public TilePreviewControl()
        {
            InitializeComponent();
            Update();
        }

        static void OnTilePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TilePreviewControl control) control.Update();
        }

        void Update()
        {
            if (Tile == null || World == null) sprite.Sprite = null;
            else
            {
                World.SpriteMap.TryGetValue(Tile.Char, out var s);
                sprite.Sprite = s;
            }
        }
    }
}
