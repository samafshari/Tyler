using Net.Essentials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tyler.ViewModels;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Tyler.Views;
using Avalonia.Controls.ApplicationLifetimes;

namespace Tyler.Services
{
    public class RoutingService : Singleton<RoutingService>
    {
        readonly SettingsService _settingsService;

        public RoutingService()
        {
            _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
        }

        public Window? GetMainWindow()
        {
            var windows = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows;
            return windows?.FirstOrDefault();
        }

        public async Task<bool> ShowConfirmDialogAsync(Window? parent, string title, string text)
        {
            parent ??= GetMainWindow();
            if (parent == null)
                throw new InvalidOperationException("Cannot show open file dialog without a parent window.");

            var result = await MessageBoxWindow.ShowConfirmDialogAsync(parent, title, text, ["Yes", "No"]);
            return result == "Yes";
        }

        public async Task<bool> ShowDialogAsync(Window? parent, string title, string text)
        {
            parent ??= GetMainWindow();
            if (parent == null)
                throw new InvalidOperationException("Cannot show open file dialog without a parent window.");

            var result = await MessageBoxWindow.ShowConfirmDialogAsync(parent, title, text, ["OK"]);
            return result == "OK";
        }

        static IReadOnlyList<FilePickerFileType> GetFileFilter(string extension, string typeName)
        {
            return new[]
            {
                new FilePickerFileType(typeName) {
                    Patterns = new[]{ $"*{extension}" } 
                }
            };
        }

        public async Task<IStorageFile?> ShowOpenFileDialogAsync(Window? parent, string title, string extension, string typeName)
        {
            parent ??= GetMainWindow();
            if (parent == null)
                throw new InvalidOperationException("Cannot show open file dialog without a parent window.");

            var options = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = title,
                FileTypeFilter = GetFileFilter(extension, typeName),
            };
            
            var result = await parent.StorageProvider.OpenFilePickerAsync(options);
            var file = result?.FirstOrDefault();
            if (file != null)
                return file;
            return null;
        }

        public async Task<IStorageFile?> ShowSaveFileDialogAsync(Window? parent, string title, string extension, string typeName)
        {
            parent ??= GetMainWindow(); 
            if (parent == null)
                throw new InvalidOperationException("Cannot show open file dialog without a parent window.");

            var options = new FilePickerSaveOptions
            {
                Title = title,
                DefaultExtension = extension,
                SuggestedFileName = $"New{extension}",
                FileTypeChoices = GetFileFilter(extension, typeName),
                ShowOverwritePrompt = true
            };

            var result = await parent.StorageProvider.SaveFilePickerAsync(options);
            if (result != null)
                return result;
            return null;
        }

        public async Task<IStorageFolder?> ShowOpenFolderDialogAsync(Window? parent, string title)
        {
            parent ??= GetMainWindow(); 
            if (parent == null)
                throw new InvalidOperationException("Cannot show open file dialog without a parent window.");

            if (!parent.StorageProvider.CanPickFolder)
                throw new NotSupportedException("Storage provider does not support picking folders.");

            var result = await parent.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

            var directory = result?.FirstOrDefault();
            if (directory != null)
                return directory;
            return null;
        }

        public void ShowSpriteSheetEditor()
        {
            var lastPngPath = _settingsService.Data.LastOpenedPNGPath;
            if (!File.Exists(lastPngPath)) lastPngPath = null;
            ShowSpriteSheetEditor(lastPngPath);
        }

        public void ShowSpriteSheetEditor(string? path)
        {
            //var window = new SpriteSheetEditorWindow();
            //var vm = new SpriteSheetEditorViewModel();
            //if (File.Exists(path)) vm.LoadPNG(path);
            //window.DataContext = vm;

            //window.Show();
        }

        public void ShowSpriteSheetEditor(SpriteSheetViewModel spriteSheet, SpriteViewModel? selectedSprite)
        {
            //var window = new SpriteSheetEditorWindow();
            //var vm = new SpriteSheetEditorViewModel();
            //vm.SpriteSheet = spriteSheet;
            //vm.SelectedSprite = selectedSprite;
            //window.DataContext = vm;
            //window.Show();
        }

        public void ShowAutoSlice(SpriteSheetEditorViewModel editor)
        {
            //var window = new AutoSliceWindow();
            //var vm = new AutoSliceViewModel(editor);
            //window.DataContext = vm;
            //window.Show();
        }

        public void ShowWorldEditor()
        {
            //var window = new WorldEditorWindow();
            //var vm = new WorldViewModel();
            //if (File.Exists(_settingsService.Data.LastWorldPath))
            //{
            //    vm.Path = _settingsService.Data.LastWorldPath;
            //    vm.Load();
            //}
            //window.DataContext = vm;
            //window.Show();
        }

        public void ShowTileDefsEditor(WorldViewModel world)
        {
            //var window = new TileDefsEditorWindow();
            //var vm = new TileDefsEditorViewModel(world);
            //window.DataContext = vm;
            //window.Show();
        }

        public void ShowWorldSpriteSheetManager(WorldViewModel world)
        {
            //var window = new WorldSpriteSheetManagerWindow();
            //var vm = new WorldSpriteSheetManagerViewModel(world);
            //window.DataContext = vm;
            //window.Show();
        }

        public void ShowBoardSettings(BoardViewModel board)
        {
            //var window = new BoardSettingsWindow();
            //window.DataContext = board;
            //window.ShowDialog();
        }

        public void ShowWorldSettings(WorldViewModel world)
        {
            //var window = new WorldSettingsWindow();
            //window.DataContext = world;
            //window.ShowDialog();
        }
    }
}
