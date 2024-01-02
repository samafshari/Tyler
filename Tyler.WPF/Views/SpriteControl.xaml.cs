using Net.Essentials;

using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for SpriteControl.xaml
    /// </summary>
    public partial class SpriteControl
    {
        SpriteViewModel _oldSprite;

        public SpriteViewModel Sprite
        {
            get => (SpriteViewModel)GetValue(SpriteProperty);
            set => SetValue(SpriteProperty, value);
        }

        // Dependency Property for Sprite
        public static readonly DependencyProperty SpriteProperty =
            DependencyProperty.Register("Sprite", typeof(SpriteViewModel), typeof(SpriteControl), new PropertyMetadata(OnSpritePropertyChanged));
        
        public SpriteControl()
        {
            InitializeComponent();
            SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
            Update();
        }

        static void OnSpritePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpriteControl control) control.Update();
        }

        void Update()
        {
            if (Sprite == null || !File.Exists(Sprite.Path)) Source = null;
            else if (_oldSprite != Sprite)
            {
                _oldSprite = Sprite;
                Source = ContainerService.Instance.Get<BitmapCache>().Get(Sprite.Path, Sprite.X, Sprite.Y, Sprite.Width, Sprite.Height);
            }
        }
    }
}
