using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class TileDefsEditorViewModel : TinyViewModel
    {
        public RoutingService _routingService;
        public WorldViewModel World { get; } = new WorldViewModel();

        public TileDefsEditorViewModel() : this(new WorldViewModel()) { }

        public TileDefsEditorViewModel(WorldViewModel world)
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            World = world;
        }

        SpriteSheetViewModel? _spriteSheet;
        public SpriteSheetViewModel? SpriteSheet
        {
            get => _spriteSheet != null && World.SpriteSheets.Contains(_spriteSheet) ? _spriteSheet : null;
            set => SetProperty(ref _spriteSheet, value);
        }

        SpriteViewModel? _selectedSprite;
        public SpriteViewModel? SelectedSprite
        {
            get => SpriteSheet != null && _selectedSprite != null && SpriteSheet.Sprites.Contains(_selectedSprite) ? _selectedSprite : null;
            set => SetProperty(ref _selectedSprite, value);
        }

        TileDefViewModel? _selectedTileDef;
        public TileDefViewModel? SelectedTileDef
        {
            get => _selectedTileDef != null && World.TileDefs.Contains(_selectedTileDef) ? _selectedTileDef : null;
            set => SetProperty(ref _selectedTileDef, value);
        }

        public void AddTileDef()
        {
            var tileDef = new TileDefViewModel(World, new TileDef());
            if (SelectedSprite != null)
            {
                var spriteSheet = World.SpriteSheets?.FirstOrDefault(x => x.Sprites.Contains(SelectedSprite));
                tileDef.SpriteSheet = spriteSheet?.Path;
                tileDef.SpriteId = SelectedSprite.Id;
            }

            World.TileDefs.Add(tileDef);
            SelectedTileDef = tileDef;
        }
    }
}
