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
            if (Sprite == null) return;
            else
            {
                var sz = Sprite.GetImageSize();
                if (sz.Width <= 0 || sz.Height <= 0) return;

                rect.Width = (double)Sprite.Width * ActualWidth / sz.Width;
                rect.Height = (double)Sprite.Height * ActualHeight / sz.Height;
                rect.Margin = new Thickness(
                    (double)Sprite.X * ActualWidth / sz.Width,
                    (double)Sprite.Y * ActualHeight / sz.Height
                    , 0, 0);
                rect.HorizontalAlignment = HorizontalAlignment.Left;
                rect.VerticalAlignment = VerticalAlignment.Top;
            }
        }
    }
}
