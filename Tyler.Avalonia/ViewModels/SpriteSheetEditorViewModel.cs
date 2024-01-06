using Net.Essentials;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using Tyler.Models;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class SpriteSheetEditorViewModel : ViewModel
    {
        readonly RoutingService _routingService;
        readonly SettingsService _settingsService;

        SpriteSheetViewModel? _spriteSheet = new SpriteSheetViewModel();
        public SpriteSheetViewModel? SpriteSheet
        {
            get => _spriteSheet;
            set => SetProperty(ref _spriteSheet, value);
        }

        SpriteViewModel? _selectedSprite;
        public SpriteViewModel? SelectedSprite
        {
            get => _selectedSprite;
            set
            {
                SetProperty(ref _selectedSprite, value);
                RaisePropertyChanged(nameof(IsSelectedSpriteVisible));
            }
        }

        public bool IsSelectedSpriteVisible => SelectedSprite != null;

        public SpriteSheetEditorViewModel()
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
        }

        public CommandModel AddSpriteCommand => new CommandModel(() =>
        {
            AddSprite();
        });

        public CommandModel RemoveSpriteCommand => new CommandModel(async () =>
        {
            if (SelectedSprite != null)
                if (await _routingService.ShowConfirmDialogAsync(default, "Confirm Deletion", $"Sprite ID={SelectedSprite.Id} will be deleted.. Are you sure?"))
                {
                    SpriteSheet?.Sprites.Remove(SelectedSprite);
                    SelectedSprite = null;
                }
        });

        public CommandModel ClearSpritesCommand => new CommandModel(async () =>
        {
            if (await _routingService.ShowConfirmDialogAsync(default, "Warning", "Your unsaved changes will be lost. Are you sure?"))
            {
                SpriteSheet?.Sprites.Clear();
                SelectedSprite = null;
            }
        });
        
        public CommandModel OpenPNGCommand => new CommandModel(async () =>
        {
            var file = await _routingService.ShowOpenFileDialogAsync(default, "Open Sprite Sheet PNG", ".png", Vars.FileDialogTypePNG);
            var fileName = file?.Path?.LocalPath;
            if (File.Exists(fileName))
                if (await _routingService.ShowConfirmDialogAsync(default, "Warning", "Your unsaved changes will be lost. Are you sure?"))
                    LoadPNG(fileName);
        });

        public SpriteViewModel? AddSprite()
        {
            if (SpriteSheet == null) return default;

            var id = SpriteSheet.Sprites.Count.ToString();
            
            if (SpriteSheet.Sprites.Any())
                id = SpriteSheet.Sprites.Select(x => int.TryParse(x.Id, out var _i) ? _i : 0).Max() + 1 + "";

            SpriteSheet.Sprites.Add(new SpriteViewModel { Path = SpriteSheet.Path, Id = id });
            SelectedSprite = SpriteSheet.Sprites.Last();
            return SelectedSprite;
        }

        public void LoadPNG(string pngPath)
        {
            var model = new SpriteSheet
            {
                Path = pngPath,
                Id = Path.GetFileNameWithoutExtension(pngPath),
                Sprites = new List<Sprite>()
            };
            SpriteSheet = new SpriteSheetViewModel(model);
        }

        public CommandModel AutoSliceCommand => new CommandModel(() =>
        {
            _routingService.ShowAutoSlice(this);
        });
    }
}