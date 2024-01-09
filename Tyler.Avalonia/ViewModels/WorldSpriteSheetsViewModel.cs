﻿using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;
using Tyler.Services;

using static Tyler.ViewModels.WorldViewModel;

namespace Tyler.ViewModels
{
    public class WorldSpriteSheetsViewModel : TinyViewModel
    {
        readonly RoutingService _routingService;

        Dictionary<string, SpriteSheetViewModel> spriteSheetsMap = new Dictionary<string, SpriteSheetViewModel>();
        Dictionary<string, SpriteViewModel> spritesMap = new Dictionary<string, SpriteViewModel>();

        public WorldViewModel World { get; }

        ObservableCollection<SpriteSheetViewModel> _spriteSheets = new ObservableCollection<SpriteSheetViewModel>();
        public ObservableCollection<SpriteSheetViewModel> SpriteSheets
        {
            get => _spriteSheets;
            set => SetProperty(ref _spriteSheets, value);
        }

        List<SpriteViewModel> _mappedSprites = new List<SpriteViewModel>();
        public List<SpriteViewModel> MappedSprites
        {
            get => _mappedSprites;
            set => SetProperty(ref _mappedSprites, value);
        }

        SpriteSheetViewModel? _selectedSpriteSheet;
        public SpriteSheetViewModel? SelectedSpriteSheet
        {
            get => _selectedSpriteSheet;
            set => SetProperty(ref _selectedSpriteSheet, value);
        }

        SpriteViewModel? _selectedSprite;
        public SpriteViewModel? SelectedSprite
        {
            get => _selectedSprite;
            set => SetProperty(ref _selectedSprite, value);
        }

        public WorldSpriteSheetsViewModel(WorldViewModel world)
        {
            _routingService = ContainerService.Instance.Get<RoutingService>();
            World = world;
        }

        public void Reload(World worldDef)
        {
            SpriteSheets.Clear();
            foreach (var spriteSheet in worldDef.SpriteSheets.Select(x => new SpriteSheetViewModel(x)))
                AddSpriteSheet(spriteSheet, true);
            ReinitializeSpriteMap();
            UpdateSpriteSheetsMap();
            UpdateSpritesMap();
        }

        public void SerializeInto(World worldDef)
        {
            worldDef.SpriteSheets = SpriteSheets.Select(x => x.Serialize()).ToList();
        }
        public async Task AddSpriteSheetAsync()
        {
            var path = await _routingService.ShowOpenFileDialogAsync(default, "Open Sprite Sheet PNG", ".png", Vars.FileDialogTypePNG);
            AddSpriteSheet(path?.Path?.LocalPath);
        }

        public void AddSpriteSheet(string? path)
        {
            if (path != null)
            {
                var spriteSheet = new SpriteSheetViewModel(path);
                AddSpriteSheet(spriteSheet);
            }
        }

        public void AddSpriteSheet(SpriteSheetViewModel spriteSheet, bool silent = false)
        {
            SpriteSheets.Add(spriteSheet);
            SelectedSpriteSheet = spriteSheet;
            spriteSheet.IdChanged += SpriteSheet_IdChanged;
            spriteSheet.SpriteIdChanged += SpriteSheet_SpriteIdChanged;
            spriteSheet.SpriteListChanged += SpriteSheet_SpriteListChanged;

            if (!silent)
            {
                ReinitializeSpriteMap();
                World.SelectedTab = Tabs.Sprites;
            }
        }

        public void RemoveSpriteSheet()
        {
            RemoveSpriteSheet(SelectedSpriteSheet);
        }


        public void RemoveSpriteSheet(SpriteSheetViewModel? spriteSheet)
        {
            if (spriteSheet == null) return;
            if (SpriteSheets.Contains(spriteSheet))
            {
                if (SelectedSpriteSheet == spriteSheet)
                    SelectedSpriteSheet = null;
                SpriteSheets.Remove(spriteSheet);
                spriteSheet.IdChanged -= SpriteSheet_IdChanged;
                spriteSheet.SpriteIdChanged -= SpriteSheet_SpriteIdChanged;
                spriteSheet.SpriteListChanged -= SpriteSheet_SpriteListChanged;
                ReinitializeSpriteMap();
            }
        }

        private void SpriteSheet_SpriteListChanged(object? sender, ObservableCollection<SpriteViewModel> e)
        {
            ReinitializeSpriteMap();
        }

        private void SpriteSheet_IdChanged(object? sender, NameChangeEventArgs e)
        {
            if (e.Entity is not SpriteSheetViewModel spriteSheet) return;
            if (spriteSheetsMap.ContainsValue(spriteSheet))
            {
                var key = spriteSheetsMap.First(x => x.Value == spriteSheet).Key;
                spriteSheetsMap.Remove(key);
            }
            if (e.NewName != null)
                spriteSheetsMap[e.NewName] = spriteSheet;
        }

        private void SpriteSheet_SpriteIdChanged(object? sender, NameChangeEventArgs e)
        {
            if (e.Entity is not SpriteViewModel sprite) return;
            bool isDirty = false;
            if (spritesMap.ContainsValue(sprite))
            {
                lock (spritesMap)
                {
                    var key = spritesMap.First(x => x.Value == sprite).Key;
                    spritesMap.Remove(key);
                    isDirty = true;
                }
            }
            if (e.NewName != null)
            {
                lock (spritesMap)
                {
                    spritesMap[e.NewName] = sprite;
                    isDirty = true;
                }
            }
            if (isDirty)
                lock (MappedSprites)
                    MappedSprites = spritesMap.Values.ToList();
        }

        public void ReinitializeSpriteMap()
        {
            lock (spritesMap)
                lock (MappedSprites)
                {
                    spritesMap.Clear();
                    foreach (var spriteSheet in SpriteSheets)
                        foreach (var sprite in spriteSheet.Sprites)
                        {
                            if (sprite.Id != null)
                                spritesMap[sprite.Id] = sprite;
                        }
                    MappedSprites = spritesMap.Values.ToList();
                }
        }

        public void EditSpriteSheet()
        {
            if (SelectedSpriteSheet == null) return;
            World.SelectedTab = Tabs.Sprites;
        }

        public SpriteViewModel? GetSprite(string? id)
        {
            if (id == null) return null;
            spritesMap.TryGetValue(id, out var sprite);
            return sprite;
        }

        void UpdateSpriteSheetsMap()
        {
            lock (spriteSheetsMap)
            {
                spriteSheetsMap.Clear();
                foreach (var spriteSheet in SpriteSheets.Where(x => x.Id != null))
                    spriteSheetsMap[spriteSheet.Id!] = spriteSheet;
            }
        }

        void UpdateSpritesMap()
        {
            lock (spritesMap)
                lock (MappedSprites)
                {
                    spritesMap.Clear();
                    foreach (var spriteSheet in SpriteSheets)
                        foreach (var sprite in spriteSheet.Sprites.Where(x => x.Id != null))
                            spritesMap[sprite.Id!] = sprite;
                    MappedSprites = spritesMap.Values.ToList();
                }
        }

        public CommandModel AddSpriteSheetCommand => new CommandModel(AddSpriteSheetAsync);
        public CommandModel RemoveSpriteSheetCommand => new CommandModel(RemoveSpriteSheet);
        public CommandModel EditSpriteSheetCommand => new CommandModel(EditSpriteSheet);
        public CommandModel ReinitializeSpriteMapCommand => new CommandModel(() =>
        {
            ReinitializeSpriteMap();
            UpdateSpriteSheetsMap();
            UpdateSpritesMap();
        });
    }
}
