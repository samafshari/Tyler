using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class BoardViewModel : ViewModel
    {
        readonly RoutingService _routingService;
        readonly ScriptingService _scriptingService;

        public WorldViewModel World { get; } = new WorldViewModel();

        public string DisplayName => ToString();

        string _id;
        public string Id
        {
            get => _id;
            set
            {
                SetProperty(ref _id, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        int _width;
        public int Width
        {
            get => _width;
            set
            {
                var dirty = Width != value;
                SetProperty(ref _width, value);
                if (dirty) BuildTileGrid();
            }
        }

        int _height;
        public int Height
        {
            get => _height;
            set
            {
                var dirty = Height != value;
                SetProperty(ref _height, value);
                if (dirty) BuildTileGrid();
            }
        }

        string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        ObservableCollection<TileViewModel> _tiles = new ObservableCollection<TileViewModel>();
        public ObservableCollection<TileViewModel> Tiles
        {
            get => _tiles;
            set => SetProperty(ref _tiles, value);
        }

        public readonly Dictionary<(int, int), TileViewModel> TileGrid = new Dictionary<(int, int), TileViewModel>();

        string _script;
        public string Script
        {
            get => _script;
            set => SetProperty(ref _script, value);
        }

        int _state = Vars.StateMin;
        public int State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public BoardViewModel() : this(null, new Board())
        {
        }

        public BoardViewModel(WorldViewModel world, Board board)
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            _scriptingService = ContainerService.Instance.GetOrCreate<ScriptingService>();

            World = world;
            Load(board);
        }

        public void Load(Board board)
        {
            Id = board.Id;
            Name = board.Name;
            Width = board.Width;
            Height = board.Height;
            Tiles = new ObservableCollection<TileViewModel>(board.Tiles.Select(x => new TileViewModel(x)));
            Script = _scriptingService.BoardToScript(board);
            BuildTileGrid();
        }

        public void SetTile(Tile tile)
        {
            var vm = new TileViewModel(tile);
            if (TileGrid.TryGetValue((tile.X, tile.Y), out var oldTile))
            {
                Tiles.Remove(oldTile);
                TileGrid.Remove((tile.X, tile.Y));
            }
            Tiles.Add(vm);
            TileGrid[(tile.X, tile.Y)] = vm;
            BumpState();
        }

        public void SetTile(int x, int y, int z, char c)
        {
            var tile = new Tile
            {
                X = x,
                Y = y,
                Z = z,
                Char = c,
                Script = ""
            };
            SetTile(tile);
        }

        void BuildTileGrid()
        {
            TileGrid.Clear();
            foreach (var tile in Tiles)
                TileGrid[(tile.X, tile.Y)] = tile;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (!TileGrid.ContainsKey((x, y)))
                        TileGrid[(x, y)] = null;
                }
            BumpState();
        }

        void BumpState()
        {
            Vars.BumpState(ref _state);
            RaisePropertyChanged(nameof(State));
        }

        public Board Serialize()
        {
            var model = new Board
            {
                Id = Id,
                Name = Name,
                Width = Width,
                Height = Height,
                Tiles = Tiles.Select(x => x.Serialize()).ToList()
            };
            return model;
        }

        public override string ToString()
        {
            return $"[{Id}] {Name}";
        }

        public void ReadScript()
        {
            if (!_routingService.ShowConfirmDialog("Warning", "You might lose your pending changes. Do you want to continue?"))
                return;

            var board = _scriptingService.ScriptToBoard(Script);
            Load(board);
        }

        public void WriteScript()
        {
            if (!_routingService.ShowConfirmDialog("Warning", "This will rewrite the script. Do you want to continue?"))
                return;

            var board = Serialize();
            Script = _scriptingService.BoardToScript(board);
        }

        public void ShowSettings()
        {
            _routingService.ShowBoardSettings(this);
        }

        public CommandModel ReadScriptCommand => new CommandModel(ReadScript);
        public CommandModel WriteScriptCommand => new CommandModel(WriteScript);
        public CommandModel ShowSettingsCommand => new CommandModel(ShowSettings);
    }
}
