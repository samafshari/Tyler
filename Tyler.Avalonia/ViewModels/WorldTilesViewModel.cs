﻿using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;

namespace Tyler.ViewModels
{
    public class WorldTilesViewModel : TinyViewModel
    {
        readonly Dictionary<char, TileDefViewModel> charMap = new Dictionary<char, TileDefViewModel>();

        public WorldViewModel World { get; }

        public List<AnimationMode> AnimationModes { get; } = Enum.GetValues(typeof(AnimationMode)).Cast<AnimationMode>().ToList();

        ObservableCollection<TileDefViewModel> _tileDefs = new ObservableCollection<TileDefViewModel>();
        public ObservableCollection<TileDefViewModel> TileDefs
        {
            get => _tileDefs;
            set => SetProperty(ref _tileDefs, value);
        }

        SpriteViewModel? _selectedSprite;
        public SpriteViewModel? SelectedSprite
        {
            get => _selectedSprite;
            set => SetProperty(ref _selectedSprite, value);
        }

        TileDefViewModel? _selectedTileDef;
        public TileDefViewModel? SelectedTileDef
        {
            get => _selectedTileDef;
            set
            {
                SetProperty(ref _selectedTileDef, value);
                RaisePropertyChanged(nameof(HasSelectedTileDef));
            }
        }
        public bool HasSelectedTileDef => SelectedTileDef != null;

        TileViewModel? _selectedTile;
        public TileViewModel? SelectedTile
        {
            get => _selectedTile;
            set
            {
                SetProperty(ref _selectedTile, value);
                RaisePropertyChanged(nameof(IsSelectedTileVisible));
            }
        }
        public bool IsSelectedTileVisible => SelectedTile != null;

        public WorldTilesViewModel(WorldViewModel world)
        {
            World = world;
            World.SpriteSheetsManager.MappedSpritesUpdated += (s, e) =>
            {
                if (SelectedSprite != null && !e.Contains(SelectedSprite))
                    SelectedSprite = null;
            };
        }

        public void Reload(World worldDef)
        {
            TileDefs = new ObservableCollection<TileDefViewModel>(worldDef.TileDefs.Select(x => new TileDefViewModel(World, x)));
            SelectedTileDef = null;
        }

        public void SerializeInto(World worldDef)
        {
            worldDef.TileDefs = TileDefs.Select(x => x.Serialize()).ToList();
        }

        public void EditTileDef()
        {
            if (SelectedTile == null) return;
            World.SelectedTab = WorldViewModel.Tabs.Tiles;
            SelectedTileDef = GetTileDef(SelectedTile.Char);
        }

        public TileDefViewModel? GetTileDef(char c)
        {
            lock (charMap)
            {
                if (charMap.TryGetValue(c, out var tileDef))
                    return tileDef;
            }
            return null;
        }

        public void SelectTile(TileViewModel? tile)
        {
            SelectedTile = tile;
            if (SelectedTile != null && charMap.TryGetValue(SelectedTile.Char, out var tileDef))
                SelectedTileDef = tileDef;
        }

        string GetNewId(string? proposal = null)
        {
            string id = proposal ?? TileDefs.Count.ToString();
            if (TileDefs.Any(x => x.Id == id))
            {
                int max = TileDefs.Max(x =>
                {
                    if (x.Id != null && int.TryParse(x.Id, out var i))
                        return i;
                    return 0;
                });
                id = (max + 1).ToString();
            }
            return id;
        }

        public void NewTileDef()
        {
            var tileDef = new TileDef
            {
                Id = GetNewId(),
                Char = Vars.UnassignedChar,
            };
            AddTileDef(tileDef);
        }

        public void AddTileDef(string? spriteId)
        {
            if (string.IsNullOrWhiteSpace(spriteId)) return;
            var sprite = World.SpriteSheetsManager.GetSprite(spriteId);
            if (sprite != null) AddTileDef(sprite);
        }

        public void AddTileDef(SpriteViewModel? sprite)
        {
            if (sprite == null) return;
            var tileDef = new TileDef
            {
                Id = GetNewId(sprite.Id),
                Char = Vars.UnassignedChar,
                Animation = new TileAnimation
                {
                    KeyFrames = new []
                    {
                        new SpriteKeyFrame
                        {
                            SpriteId = sprite.Id,
                            Duration = 1
                        }
                    }
                }
            };
            AddTileDef(tileDef);
        }

        public void AddTileDef(TileDef tileDef)
        {
            var tileDefViewModel = new TileDefViewModel(World, tileDef);
            AddTileDef(tileDefViewModel);
        }

        public void AddTileDef(TileDefViewModel? tileDef)
        {
            if (tileDef == null) return;
            TileDefs.Add(tileDef);
            lock (charMap)
            {
                if (charMap.ContainsKey(tileDef.Char)) return;
                charMap[tileDef.Char] = tileDef;
            }
            SelectedTileDef = tileDef;
        }

        public void RemoveTileDef(TileDefViewModel? tileDef)
        {
            if (tileDef == null) return;
            TileDefs.Remove(tileDef);
            lock (charMap)
            {
                if (charMap.ContainsKey(tileDef.Char))
                    charMap.Remove(tileDef.Char);
            }
            if (SelectedTileDef == tileDef)
                SelectedTileDef = null;
        }

        public void RemoveTileDef()
        {
            RemoveTileDef(SelectedTileDef);
        }

        public void DuplicateTileDef(TileDefViewModel? tileDef)
        {
            if (tileDef == null) return;
            var newTileDef = tileDef.Serialize();
            newTileDef.Id = GetNewId($"{tileDef.Id} (Copy)");
            AddTileDef(newTileDef);
        }
        
        public void EditSprite(SpriteViewModel? sprite)
        {
            World.SpriteSheetsManager.EditSprite(sprite);
        }

        public CommandModel<SpriteViewModel?> EditSpriteCommand => new CommandModel<SpriteViewModel?>(EditSprite);
        public CommandModel<SpriteViewModel?> NewTileFromSpriteCommand => new CommandModel<SpriteViewModel?>(AddTileDef);
        public CommandModel NewTileDefCommand => new CommandModel(NewTileDef);
        public CommandModel<TileDefViewModel?> DuplicateTileDefCommand => new CommandModel<TileDefViewModel?>(DuplicateTileDef);
        public CommandModel RemoveTileDefCommand => new CommandModel(RemoveTileDef);
    }
}