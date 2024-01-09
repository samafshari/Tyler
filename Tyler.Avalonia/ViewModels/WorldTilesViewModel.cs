using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;

using static Tyler.ViewModels.WorldViewModel;

namespace Tyler.ViewModels
{
    public class WorldTilesViewModel : TinyViewModel
    {
        Dictionary<char, TileDefViewModel> charMap = new Dictionary<char, TileDefViewModel>();

        public WorldViewModel World { get; }

        ObservableCollection<TileDefViewModel> _tileDefs = new ObservableCollection<TileDefViewModel>();
        public ObservableCollection<TileDefViewModel> TileDefs
        {
            get => _tileDefs;
            set => SetProperty(ref _tileDefs, value);
        }

        TileDefViewModel? _selectedTileDef;
        public TileDefViewModel? SelectedTileDef
        {
            get => _selectedTileDef;
            set => SetProperty(ref _selectedTileDef, value);
        }

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
            if (charMap.TryGetValue(c, out var tileDef))
                return tileDef;
            return null;
        }

        public void SelectTile(TileViewModel? tile)
        {
            SelectedTile = tile;
            if (SelectedTile != null && charMap.TryGetValue(SelectedTile.Char, out var tileDef))
                SelectedTileDef = tileDef;
        }
    }
}
