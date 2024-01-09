using Net.Essentials;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Tyler.Models;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class WorldViewModel : ViewModel
    {
        readonly RoutingService _routingService;
        readonly SettingsService _settingsService;
        readonly ScriptingService _scriptingService;
        
        Dictionary<string, BoardViewModel> boardMap = new Dictionary<string, BoardViewModel>();
        Dictionary<char, TileDefViewModel> charMap = new Dictionary<char, TileDefViewModel>();

        public WorldSpriteSheetsViewModel SpriteSheetsManager { get; }

        public enum Tabs
        {
            Sprites,
            Tiles,
            Brushes,
            Boards,
        }

        public Tabs SelectedTab
        {
            get => (Tabs)SelectedTabIndex;
            set => SelectedTabIndex = (int)value;
        }

        int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                SetProperty(ref _selectedTabIndex, value);
                RaisePropertyChanged(nameof(SelectedTab));
            }
        }

        string? _path;
        public string? Path
        {
            get => _path;
            set
            {
                SetProperty(ref _path, value);
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        public string DisplayName => ToString();

        int _width;
        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        int _height;
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        int _tileWidth = 32;
        public int TileWidth
        {
            get => _tileWidth;
            set => SetProperty(ref _tileWidth, value);
        }

        int _tileHeight = 32;
        public int TileHeight
        {
            get => _tileHeight;
            set => SetProperty(ref _tileHeight, value);
        }

        ObservableCollection<BoardViewModel> _boards = new ObservableCollection<BoardViewModel>();
        public ObservableCollection<BoardViewModel> Boards
        {
            get => _boards;
            set => SetProperty(ref _boards, value);
        }

        ObservableCollection<TileDefViewModel> _tileDefs = new ObservableCollection<TileDefViewModel>();
        public ObservableCollection<TileDefViewModel> TileDefs
        {
            get => _tileDefs;
            set => SetProperty(ref _tileDefs, value);
        }

        public bool IsSelectedBoardVisible => SelectedBoard != null;
        BoardViewModel? _selectedBoard;
        public BoardViewModel? SelectedBoard
        {
            get => _selectedBoard;
            set
            {
                var isDirty = value != SelectedBoard;
                SetProperty(ref _selectedBoard, value);
                if (isDirty) SelectedTile = null;
                RaisePropertyChanged(nameof(IsSelectedBoardVisible));
            }
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

        public WorldViewModel()
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
            _scriptingService = ContainerService.Instance.GetOrCreate<ScriptingService>();
            SpriteSheetsManager = new WorldSpriteSheetsViewModel(this);
        }

        public WorldViewModel(string path) : this()
        {
            Path = path;
        }

        public async Task LoadAsAsync()
        {
            var newFile = await _routingService.ShowOpenFileDialogAsync(
                _routingService.GetMainWindow(),
                "Open World",
                Vars.WorldExtension,
                Vars.FileDialogTypeWorld);
            var newPath = await _routingService.ShowOpenFileDialogAsync(default, "Open World", Vars.WorldExtension, Vars.FileDialogTypeWorld);
            if (File.Exists(newPath?.Path.LocalPath))
            {
                Path = newPath.Path.LocalPath;
                Load();
            }
        }

        public async Task RevertAsync()
        {
            var confirm = await _routingService.ShowConfirmDialogAsync(default, "Warning", "Your unsaved changes will be lost. Are you sure?");
            if (confirm)
                Load();
        }

        public void Load()
        {
            if (!File.Exists(Path))
                Path = null;
            else
            {
                var worldDef = JsonConvert.DeserializeObject<World>(File.ReadAllText(Path));
                if (worldDef != null)
                {
                    Load(worldDef);
                    _settingsService.Data.LastWorldPath = Path;
                    _settingsService.SaveWithLock();
                }
            }
            if (SelectedBoard == null)
                SelectedBoard = Boards.FirstOrDefault();
        }

        public void New()
        {
            //var confirm = await _routingService.ShowConfirmDialogAsync(default, "Warning", "Your unsaved changes will be lost. Are you sure?");
            //if (confirm)
            //{
            //    Path = null;
            //    Load(new World());
            //}
            _routingService.ShowWorldEditor(false);
        }

        public void Load(World worldDef)
        {
            if (worldDef == null) return;
            Width = worldDef.Width;
            Height = worldDef.Height;
            TileWidth = worldDef.TileWidth;
            TileHeight = worldDef.TileHeight;
            SpriteSheetsManager.Reload(worldDef);
            Boards = new ObservableCollection<BoardViewModel>(worldDef.Boards.Select(x => new BoardViewModel(this, x)));
            TileDefs = new ObservableCollection<TileDefViewModel>(worldDef.TileDefs.Select(x => new TileDefViewModel(this, x)));
            UpdateBoardMap();
        }

        public World Serialize()
        {
            World worldDef = new World();
            worldDef.Width = Width;
            worldDef.Height = Height;
            worldDef.Boards = Boards.Select(x => x.Serialize()).ToList();
            worldDef.TileDefs = TileDefs.Select(x => x.Serialize()).ToList();
            SpriteSheetsManager.SerializeInto(worldDef);
            return worldDef;
        }

        public async Task SaveAsAsync()
        {
            var newPath = await _routingService.ShowSaveFileDialogAsync(default, "Save World", Vars.WorldExtension, Vars.FileDialogTypeWorld);
            if (newPath != null)
            {
                Path = newPath?.Path?.LocalPath;
                await SaveAsync();
            }
        }

        public async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                await SaveAsAsync();
                return;
            }
            var worldDef = Serialize();
            var json = JsonConvert.SerializeObject(worldDef, Formatting.Indented);
            File.WriteAllText(Path, json);
            _settingsService.Data.LastWorldPath = Path;
            _settingsService.SaveWithLock();
            await _routingService.ShowDialogAsync(default, "Success", $"World saved to {Path}");
        }

        public void AddBoard()
        {
            var board = new BoardViewModel(this, new Board
            {
                Id = Boards.Count.ToString(),
                Width = Width,
                Height = Height
            });
            Boards.Add(board);
            SelectedBoard = board;
        }

        public async Task RemoveBoardAsync()
        {
            await RemoveBoardAsync(SelectedBoard);
        }

        public async Task RemoveBoardAsync(BoardViewModel? board)
        {
            if (board == null) return;
            if (SelectedBoard != null)
                if (await _routingService.ShowConfirmDialogAsync(default, "Confirm Deletion", $"Board {board.Name} ({board.Id}) will be deleted.. Are you sure?"))
                {
                    if (SelectedBoard == board)
                        SelectedBoard = null;
                    Boards.Remove(board);
                }
        }

        public void MoveBoard(BoardViewModel? board, int direction)
        {
            if (board == null) return;
            var index = Boards.IndexOf(board);
            if (index == -1) return;
            var newIndex = index + direction;
            if (newIndex < 0 || newIndex >= Boards.Count) return;
            Boards.Move(index, newIndex);
        }

        public void MoveBoard(int direction)
        {
            MoveBoard(SelectedBoard, direction);
        }



        public void DuplicateBoard()
        {
            if (SelectedBoard == null) return;
            var board = SelectedBoard.Serialize();
            board.Id = Boards.Count.ToString();
            board.Name += " (Copy)";
            Boards.Add(new BoardViewModel(this, board));
        }

        public async Task ExportLevelsAsync()
        {
            var folder = await _routingService.ShowOpenFolderDialogAsync(default, "Select Folder to Export Levels");
            var folderPath = folder?.Path?.LocalPath;
            if (folderPath != null)
            {
                _scriptingService.ExportBoardsToFolder(Boards.Select(x => x.Serialize()).ToList(), folderPath);
                await _routingService.ShowDialogAsync(default, "Success", $"Levels exported to {folderPath}");
            }
        }

        public async Task ImportLevelsAsync()
        {
            var file = await _routingService.ShowOpenFileDialogAsync(default, "Select levels.txt", ".txt", "levels.txt");
            var path = file?.Path?.LocalPath;
            if (path != null)
            {
                var boards = _scriptingService.ImportBoardsFromFolder(path);
                Boards = new ObservableCollection<BoardViewModel>(boards.Select(x => new BoardViewModel(this, x)));
                await _routingService.ShowDialogAsync(default, "Success", $"Levels imported from {path}");
            }
        }

        public void EditTileDef()
        {
            if (SelectedTile == null) return;
            SelectedTab = Tabs.Tiles;
            SelectedTileDef = GetTileDef(SelectedTile.Char);
        }

        public void SelectTile(int x, int y)
        {
            if (SelectedBoard == null) return;
            if (x < 0 || y < 0 || x >= SelectedBoard.Width || y >= SelectedBoard.Height) return;
            var tile = SelectedBoard.TileGrid[x, y];
            SelectedTile = tile;
            if (SelectedTile != null && charMap.TryGetValue(SelectedTile.Char, out var tileDef))
                SelectedTileDef = tileDef;
        }

        public TileDefViewModel? GetTileDef(char c)
        {
            if (charMap.TryGetValue(c, out var tileDef))
                return tileDef;
            return null;
        }

        void UpdateBoardMap()
        {
            lock (boardMap)
            {
                boardMap.Clear();
                foreach (var board in Boards.Where(x => x.Id != null))
                    boardMap[board.Id!] = board;
            }
        }

        public override string ToString()
        {
            return Path ?? "Untitled";
        }

        public CommandModel NewCommand => new CommandModel(New);
        public CommandModel OpenCommand => new CommandModel(LoadAsAsync);
        public CommandModel RevertCommand => new CommandModel(RevertAsync);
        public CommandModel SaveCommand => new CommandModel(SaveAsync);
        public CommandModel SaveAsCommand => new CommandModel(SaveAsAsync);
        public CommandModel AddBoardCommand => new CommandModel(AddBoard);
        public CommandModel RemoveBoardCommand => new CommandModel(RemoveBoardAsync);
        public CommandModel BoardSettingsCommand => new CommandModel(() => SelectedBoard?.ShowSettings());
        public CommandModel ShowSettingsCommand => new CommandModel(() => _routingService.ShowWorldSettings(this));
        public CommandModel MoveBoardUpCommand => new CommandModel(() => MoveBoard(SelectedBoard, -1));
        public CommandModel MoveBoardDownCommand => new CommandModel(() => MoveBoard(SelectedBoard, 1));
        public CommandModel ExportLevelsCommand => new CommandModel(ExportLevelsAsync);
        public CommandModel ImportLevelsCommand => new CommandModel(ImportLevelsAsync);
        public CommandModel DuplicateBoardCommand => new CommandModel(DuplicateBoard);
        public CommandModel EditTileDefCommand => new CommandModel(EditTileDef);
        public CommandModel ShowBenchmarksCommand => new CommandModel(_routingService.ShowBenchmarks);
    }
}
