using Net.Essentials;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using Tyler.Models;
using Tyler.Services;
using Tyler.ViewModels;

namespace Tyler.ViewModels
{
    public class SpriteSheetEditorViewModel : ViewModel
    {
        readonly RoutingService _routingService;

        SpriteSheetViewModel _spriteSheet = new SpriteSheetViewModel();
        public SpriteSheetViewModel SpriteSheet
        {
            get => _spriteSheet;
            set => SetProperty(ref _spriteSheet, value);
        }

        SpriteViewModel _selectedSprite;
        public SpriteViewModel SelectedSprite
        {
            get => _selectedSprite;
            set
            {
                SetProperty(ref _selectedSprite, value);
                RaisePropertyChanged(nameof(SelectedSpriteVisibility));
            }
        }

        public Visibility SelectedSpriteVisibility => SelectedSprite == null ? Visibility.Collapsed : Visibility.Visible;

        public SpriteSheetEditorViewModel()
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
        }

        public CommandModel AddSpriteCommand => new CommandModel(() =>
        {
            AddSprite();
        });

        public CommandModel RemoveSpriteCommand => new CommandModel(() =>
        {
            if (SelectedSprite != null)
                if (_routingService.ShowConfirmDialog("Confirm Deletion", $"Sprite ID={SelectedSprite.Id} will be deleted.. Are you sure?"))
                {
                    SpriteSheet.Sprites.Remove(SelectedSprite);
                    SelectedSprite = null;
                }
        });

        public CommandModel ClearSpritesCommand => new CommandModel(() =>
        {
            if (_routingService.ShowConfirmDialog("Warning", "Your unsaved changes will be lost. Are you sure?"))
            {
                SpriteSheet.Sprites.Clear();
                SelectedSprite = null;
            }
        });
        
        public CommandModel OpenPNGCommand => new CommandModel(() =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PNG Files (*.png)|*.png"
            };
            if (dialog.ShowDialog().GetValueOrDefault() && File.Exists(dialog.FileName))
            {
                if (_routingService.ShowConfirmDialog("Warning", "Your unsaved changes will be lost. Are you sure?"))
                {
                    LoadPNG(dialog.FileName);
                }
            }
        });

        public SpriteViewModel AddSprite()
        {
            var id = SpriteSheet.Sprites.Count.ToString();
            
            if (SpriteSheet.Sprites.Any())
                id = SpriteSheet.Sprites.Select(x => int.TryParse(x.Id, out var _i) ? _i : 0).Max() + 1 + "";

            SpriteSheet.Sprites.Add(new SpriteViewModel { Id = SpriteSheet.Sprites.Count.ToString() });
            SelectedSprite = SpriteSheet.Sprites.Last();
            return SelectedSprite;
        }

        public void LoadPNG(string pngPath)
        {
            SpriteSheet = new SpriteSheetViewModel { Path = pngPath };
            SpriteSheet.LoadFromFile();
        }

        public CommandModel AutoSliceCommand => new CommandModel(() =>
        {
            _routingService.ShowAutoSlice(this);
        });
    }
}