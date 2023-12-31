﻿using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Services;

namespace Tyler.ViewModels
{
    public class WorldSpriteSheetManagerViewModel : ViewModel
    {
        public WorldViewModel World { get; } = new WorldViewModel();

        SpriteSheetViewModel? _spriteSheet;
        public SpriteSheetViewModel? SpriteSheet
        {
            get => _spriteSheet != null && World.SpriteSheets.Contains(_spriteSheet) ? _spriteSheet : null;
            set => SetProperty(ref _spriteSheet, value);
        }

        public WorldSpriteSheetManagerViewModel(WorldViewModel world)
        {
            World = world;
        }

        public CommandModel RemoveSpriteSheetCommand => new CommandModel(() => World.RemoveSpriteSheet(SpriteSheet));
        public CommandModel EditSpriteSheetCommand => new CommandModel(() =>
        {
            if (SpriteSheet == null) return;
            World.SelectedSpriteSheet = SpriteSheet;
            World.SelectedTab = WorldViewModel.Tabs.Sprites;
        });
    }
}
