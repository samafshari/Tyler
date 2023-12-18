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
        SpriteSheetViewModel ViewModel => DataContext as SpriteSheetViewModel;
        readonly ContainerService _containerService;
        readonly BitmapCache _cache;
        public SpriteSheetControl()
        {
            InitializeComponent();
            _containerService = ContainerService.Instance;
            _cache = _containerService.GetOrCreate<BitmapCache>();

            Update();
            DataContextChanged += SpriteSheetControl_DataContextChanged;
        }

        private void SpriteSheetControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Update();
        }

        void Update()
        {
            var vm = ViewModel;
            if (vm == null) return;
            if (vm.Sprites == null) return;
            img.Source = _cache.Get(vm.Path);
        }
    }
}
