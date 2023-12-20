using Microsoft.Win32;

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
        readonly SettingsService _settingsService;

        public RoutingService()
        {
            _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
        }

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

        string GetFileFilter(string extension, string typeName)
        {
            return $"{typeName} (*{extension})|*{extension}";
        }

        public string ShowOpenFileDialog(string title, string extension, string typeName)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = title;
            dialog.Filter = GetFileFilter(extension, typeName);
            if (File.Exists(_settingsService.Data.LastOpenedPNGPath))
                dialog.InitialDirectory = Path.GetDirectoryName(_settingsService.Data.LastOpenedPNGPath);
            var result = dialog.ShowDialog();
            if (result == true && File.Exists(dialog.FileName))
                return dialog.FileName;
            return null;
        }

        public string ShowSaveFileDialog(string title, string extension, string typeName)
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = GetFileFilter(extension, typeName)
            };
            if (File.Exists(_settingsService.Data.LastOpenedPNGPath))
                dialog.InitialDirectory = Path.GetDirectoryName(_settingsService.Data.LastOpenedPNGPath);
            var result = dialog.ShowDialog();
            if (result == true)
            {
                if (File.Exists(dialog.FileName) &&
                    ShowConfirmDialog("Confirm Overwrite", $"File {dialog.FileName} already exists. Are you sure you want to overwrite it?"))
                    return dialog.FileName;
            }
            return null;
        }

        public void ShowSpriteSheetEditor()
        {
            var lastPngPath = _settingsService.Data.LastOpenedPNGPath;
            if (!File.Exists(lastPngPath)) lastPngPath = null;
            ShowSpriteSheetEditor(lastPngPath);
        }

        public void ShowSpriteSheetEditor(string path)
        {
            var window = new SpriteSheetEditorWindow();
            var vm = new SpriteSheetEditorViewModel();
            if (File.Exists(path)) vm.LoadPNG(path);
            window.DataContext = vm;

            window.Show();
        }

        public void ShowSpriteSheetEditor(SpriteSheetViewModel spriteSheet, SpriteViewModel selectedSprite)
        {
            var window = new SpriteSheetEditorWindow();
            var vm = new SpriteSheetEditorViewModel();
            vm.SpriteSheet = spriteSheet;
            vm.SelectedSprite = selectedSprite;
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

        public void ShowWorldEditor()
        {
            var window = new WorldEditorWindow();
            var vm = new WorldViewModel();
            if (File.Exists(_settingsService.Data.LastWorldPath))
            {
                vm.Path = _settingsService.Data.LastWorldPath;
                vm.Load();
            }
            window.DataContext = vm;
            window.Show();
        }

        public void ShowTileDefsEditor(WorldViewModel world)
        {
            var window = new TileDefsEditorWindow();
            var vm = new TileDefsEditorViewModel(world);
            window.DataContext = vm;
            window.Show();
        }

        public void ShowWorldSpriteSheetManager(WorldViewModel world)
        {
            var window = new WorldSpriteSheetManagerWindow();
            var vm = new WorldSpriteSheetManagerViewModel(world);
            window.DataContext = vm;
            window.Show();
        }
    }
}
