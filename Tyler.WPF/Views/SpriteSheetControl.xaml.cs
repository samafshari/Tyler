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
    /// Interaction logic for SpriteSheetControl.xaml
    /// </summary>
    public partial class SpriteSheetControl : UserControl
    {
        readonly ContainerService _containerService;
        readonly BitmapCache _cache;
        public SpriteViewModel Sprite
        {
            get => (SpriteViewModel)GetValue(SpriteProperty);
            set => SetValue(SpriteProperty, value);
        }

        // Dependency Property for Sprite
        public static readonly DependencyProperty SpriteProperty =
            DependencyProperty.Register("Sprite", typeof(SpriteViewModel), typeof(SpriteSheetControl), new PropertyMetadata(OnSpritePropertyChanged));

        public SpriteSheetViewModel SpriteSheet
        {
            get => (SpriteSheetViewModel)GetValue(SpriteSheetProperty);
            set => SetValue(SpriteSheetProperty, value);
        }

        public static readonly DependencyProperty SpriteSheetProperty =
            DependencyProperty.Register("SpriteSheet", typeof(SpriteSheetViewModel), typeof(SpriteSheetControl), new PropertyMetadata(OnSpritePropertyChanged));

        public SpriteSheetControl()
        {
            InitializeComponent();
            _containerService = ContainerService.Instance;
            _cache = _containerService.GetOrCreate<BitmapCache>();
            rect.DataContext = this;
            Update();
            DataContextChanged += SpriteSheetControl_DataContextChanged;
        }

        static void OnSpritePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpriteSheetControl control)
            {
                control.Update();
            }
        }

        private void SpriteSheetControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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
            var vm = SpriteSheet;
            if (vm == null) return;
            if (vm.Sprites == null) return;
            var bmp = _cache.Get(vm.Path);
            img.Source = bmp;
        }
    }
}
