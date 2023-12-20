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
    /// Interaction logic for SpriteSheetExplorerControl.xaml
    /// </summary>
    public partial class SpriteSheetExplorerControl : UserControl
    {
        public IEnumerable<SpriteViewModel> Sprites
        {
            get => (IEnumerable<SpriteViewModel>)GetValue(SpritesProperty);
            set => SetValue(SpritesProperty, value);
        }

        // Two way dependency property for SpriteSheet
        public static readonly DependencyProperty SpritesProperty =
            DependencyProperty.Register("Sprites", typeof(IEnumerable<SpriteViewModel>), typeof(SpriteSheetExplorerControl), new PropertyMetadata(OnSpriteSheetPropertyChanged));

        public SpriteViewModel SelectedSprite
        {
            get => (SpriteViewModel)GetValue(SelectedSpriteProperty);
            set => SetValue(SelectedSpriteProperty, value);
        }

        // Two way dependency property for SelectedSprite
        public static readonly DependencyProperty SelectedSpriteProperty =
            DependencyProperty.Register("SelectedSprite", typeof(SpriteViewModel), typeof(SpriteSheetExplorerControl), new PropertyMetadata(OnSpriteSheetPropertyChanged));

        public SpriteSheetExplorerControl()
        {
            InitializeComponent();
            grd.DataContext = this;
            Update();
        }

        static void OnSpriteSheetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpriteSheetExplorerControl control) control.Update();
        }

        void Update()
        {

        }
    }
}
