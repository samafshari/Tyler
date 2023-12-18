using Net.Essentials;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Tyler.ViewModels;
using Tyler.Views;

namespace Tyler.Services
{
    public class RoutingService : Singleton<RoutingService>
    {
        public bool ShowConfirmDialog(string title, string text)
        {
            var result = MessageBox.Show(text, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public bool ShowDialog(string title, string text)
        {
            var result = MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public void ShowSpriteSheetEditor()
        {
            ShowSpriteSheetEditor(null);
        }

        public void ShowSpriteSheetEditor(string path)
        {
            var window = new SpriteSheetEditorWindow();
            var vm = new SpriteSheetEditorViewModel();
            if (File.Exists(path)) vm.LoadPNG(path);
            window.DataContext = vm;

            window.Show();
        }

        public void ShowAutoSlice(SpriteSheetEditorViewModel editor)
        {
            var window = new AutoSliceWindow();
            var vm = new AutoSliceViewModel(editor);
            window.DataContext = vm;
            window.Show();
        }
    }
}
