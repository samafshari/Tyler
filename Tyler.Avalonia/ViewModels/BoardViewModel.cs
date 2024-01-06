using Net.Essentials;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class BoardViewModel : TinyViewModel
    {
        readonly RoutingService _routingService;
        readonly ScriptingService _scriptingService;

        public WorldViewModel World { get; } = new WorldViewModel();

        public string DisplayName => ToString();

        string? _id;
        public string? Id
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

        string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        public List<TileViewModel?> Tiles = new List<TileViewModel?>();
        public TileViewModel?[,] TileGrid = new TileViewModel?[10, 10];

        string? _beforeScript;
        public string? BeforeScript
        {
            get => _beforeScript;
            set => SetProperty(ref _beforeScript, value);
        }

        string? _afterScript;
        public string? AfterScript
        {
            get => _afterScript;
            set => SetProperty(ref _afterScript, value);
        }

        string? _script;
        public string? Script
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

        public BoardViewModel(WorldViewModel? world, Board board)
        {
            using var _ = BenchmarkService.Instance.StartBenchmark();
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            _scriptingService = ContainerService.Instance.GetOrCreate<ScriptingService>();

            World = world ?? new WorldViewModel();
            Load(board);
        }

        public void Load(Board board)
        {
            using var _ = BenchmarkService.Instance.StartBenchmark();
            Id = board.Id;
            Name = board.Name;
            Width = board.Width;
            Height = board.Height;
            var viewModels = new TileViewModel?[board.Tiles.Count];
            using (BenchmarkService.Instance.StartBenchmark("Create ViewModels"))
            {
                Parallel.For(0, board.Tiles.Count, i =>
                {
                    var tile = board.Tiles[i];
                    var vm = new TileViewModel(tile);
                    viewModels[i] = vm;
                });
            }
            using (BenchmarkService.Instance.StartBenchmark("Tiles To List"))
                Tiles = viewModels.ToList();

            BeforeScript = board.BeforeScript;
            AfterScript = board.AfterScript;
            Script = _scriptingService.BoardToScript(board);
        }

        public void SetTile(Tile tile)
        {
            var vm = new TileViewModel(tile);
            if (TileGrid.GetLength(0) <= tile.X || TileGrid.GetLength(1) <= tile.Y)
                BuildTileGrid();

            var oldTile = TileGrid[tile.X, tile.Y];
            if (oldTile != null)
            {
                Tiles.Remove(oldTile);
                TileGrid[tile.X, tile.Y] = null;
            }
            Tiles.Add(vm);
            TileGrid[tile.X, tile.Y] = vm;
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

        public void BuildTileGrid()
        {
            using (BenchmarkService.Instance.StartBenchmark())
            {
                if (TileGrid.GetLength(0) != Width || TileGrid.GetLength(1) != Height)
                    TileGrid = new TileViewModel[Width, Height];
                bool hasGarbage = false;
                Parallel.For(0, Tiles.Count, i =>
                {
                    var tile = Tiles[i];
                    if (tile.X >= Width || tile.Y >= Height)
                    {
                        hasGarbage = true;
                        Tiles[i] = null;
                        return;
                    }

                    TileGrid[tile.X, tile.Y] = tile;
                });
                if (hasGarbage)
                    Tiles = Tiles.Where(x => x != null).ToList();
            }
            BumpState();
        }

        void BumpState()
        {
            using var _ = BenchmarkService.Instance.StartBenchmark();
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
                Tiles = Tiles.Select(x => x.Serialize()).ToList(),
                BeforeScript = BeforeScript,
                AfterScript = AfterScript
            };
            return model;
        }

        public override string ToString()
        {
            return $"[{Id}] {Name}";
        }

        public async Task ReadScriptAsync()
        {
            if (!await _routingService.ShowConfirmDialogAsync(default, "Warning", "You might lose your pending changes. Do you want to continue?"))
                return;

            if (Script == null) return;
            var board = _scriptingService.ScriptToBoard(Script);
            Load(board);
        }

        public async Task WriteScriptAsync()
        {
            if (!await _routingService.ShowConfirmDialogAsync(default, "Warning", "This will rewrite the script. Do you want to continue?"))
                return;

            var board = Serialize();
            Script = _scriptingService.BoardToScript(board);
        }

        public void ShowSettings()
        {
            _routingService.ShowBoardSettings(this);
        }

        public CommandModel ReadScriptCommand => new CommandModel(ReadScriptAsync);
        public CommandModel WriteScriptCommand => new CommandModel(WriteScriptAsync);
        public CommandModel ShowSettingsCommand => new CommandModel(ShowSettings);
        public CommandModel BuildTileGridCommand => new CommandModel(BuildTileGrid);
    }
}
