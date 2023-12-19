using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.ViewModels
{
    public class WorldSpriteSheetManagerViewModel : ViewModel
    {
        public WorldViewModel World { get; } = new WorldViewModel();


        SpriteSheetViewModel _spriteSheet;
        public SpriteSheetViewModel SpriteSheet
        {
            get => World.SpriteSheets.Contains(_spriteSheet) ? _spriteSheet : null;
            set => SetProperty(ref _spriteSheet, value);
        }

        public WorldSpriteSheetManagerViewModel() { }

        public WorldSpriteSheetManagerViewModel(WorldViewModel world)
        {
            World = world;
        }

        public CommandModel RemoveSpriteSheetCommand => new CommandModel(() => World.RemoveSpriteSheet(SpriteSheet));
    }
}
