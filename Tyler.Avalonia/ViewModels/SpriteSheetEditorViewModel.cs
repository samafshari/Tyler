using Net.Essentials;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class SpriteSheetEditorViewModel : ViewModel
    {
        readonly RoutingService _routingService;
        public SpriteSheetViewModel SpriteSheet { get; }

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

        public SpriteSheetEditorViewModel(SpriteSheetViewModel spriteSheet)
        {
            SpriteSheet = spriteSheet;
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
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
                SelectedSprite = null;
                SpriteSheet?.ClearSprites();
            }
        });
        
        public SpriteViewModel? AddSprite()
        {
            if (SpriteSheet == null) return default;
            SelectedSprite = SpriteSheet.AddSprite();
            return SelectedSprite;
        }

        public CommandModel AutoSliceCommand => new CommandModel(() =>
        {
            _routingService.ShowAutoSlice(default, this);
        });
    }
}