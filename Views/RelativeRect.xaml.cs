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
    /// Interaction logic for RelativeRect.xaml
    /// </summary>
    public partial class RelativeRect : UserControl
    {
        public SpriteViewModel Sprite
        {
            get => (SpriteViewModel)GetValue(SpriteProperty);
            set => SetValue(SpriteProperty, value);
        }

        // Dependency Property for Sprite
        public static readonly DependencyProperty SpriteProperty =
            DependencyProperty.Register("Sprite", typeof(SpriteViewModel), typeof(RelativeRect), new PropertyMetadata(OnSpritePropertyChanged));

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(RelativeRect), new PropertyMetadata(OnSpritePropertyChanged));

        public RelativeRect()
        {
            InitializeComponent();
            Update();
        }

        static void OnSpritePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RelativeRect control)
            {
                control.Update();
                control.RegisterCallbacks();
            }
        }

        void RegisterCallbacks()
        {
            if (Sprite != null)
            {
                Sprite.PropertyChanged -= Sprite_PropertyChanged;
                Sprite.PropertyChanged += Sprite_PropertyChanged;
            }
        }

        private void Sprite_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Update();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Update();
        }

        void Update()
        {
            if (Sprite == null || !File.Exists(Path)) return;
            else
            {
                var bmp = ContainerService.Instance.GetOrCreate<BitmapCache>().Get(Path);
                var ar = bmp.PixelWidth / bmp.PixelHeight;
                var wCoef = ActualWidth / bmp.PixelWidth;

                if (double.IsNaN(wCoef)) return;
                var x = Sprite.X * wCoef;
                var w = Sprite.Width * wCoef;
                var y = Sprite.Y * wCoef;
                var h = Sprite.Height * wCoef;
                rect.VerticalAlignment = VerticalAlignment.Top;
                rect.HorizontalAlignment = HorizontalAlignment.Left;
                rect.Width = w;
                rect.Height = h;
                rect.Margin = new Thickness(x, y, 0, 0);
                HorizontalAlignment = HorizontalAlignment.Stretch;
                VerticalAlignment = VerticalAlignment.Top;
                Height = ActualWidth / ar;
            }
        }
    }
}
