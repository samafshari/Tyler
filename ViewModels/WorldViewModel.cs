using Net.Essentials;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Tyler.Models;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class WorldViewModel : ViewModel
    {
        readonly RoutingService _routingService;
        readonly SettingsService _settingsService;
        readonly ScriptingService _scriptingService;

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

        ObservableCollection<SpriteSheetViewModel> _spriteSheets = new ObservableCollection<SpriteSheetViewModel>();
        public ObservableCollection<SpriteSheetViewModel> SpriteSheets
        {
            get => _spriteSheets;
            set => SetProperty(ref _spriteSheets, value);
        }

        string _path;
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

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

        BoardViewModel _selectedBoard;
        public BoardViewModel SelectedBoard
        {
            get => _selectedBoard;
            set
            {
                var isDirty = value != SelectedBoard;
                SetProperty(ref _selectedBoard, value);
                if (isDirty) SelectedTile = null;
            }
        }

        TileDefViewModel _selectedTileDef;
        public TileDefViewModel SelectedTileDef
        {
            get => _selectedTileDef;
            set => SetProperty(ref _selectedTileDef, value);
        }

        SpriteSheetViewModel _selectedSpriteSheet;
        public SpriteSheetViewModel SelectedSpriteSheet
        {
            get => _selectedSpriteSheet;
            set => SetProperty(ref _selectedSpriteSheet, value);
        }

        SpriteViewModel _selectedSprite;
        public SpriteViewModel SelectedSprite
        {
            get => _selectedSprite;
            set => SetProperty(ref _selectedSprite, value);
        }

        public Dictionary<char, SpriteViewModel> SpriteMap = new Dictionary<char, SpriteViewModel>();

        List<SpriteViewModel> _spriteMapList = new List<SpriteViewModel>();
        public List<SpriteViewModel> SpriteMapList
        {
            get => _spriteMapList;
            set => SetProperty(ref _spriteMapList, value);
        }

        public Visibility SelectedTileVisibility => SelectedTile == null ? Visibility.Collapsed : Visibility.Visible;

        TileViewModel _selectedTile;
        public TileViewModel SelectedTile
        {
            get => _selectedTile;
            set
            {
                SetProperty(ref _selectedTile, value);
                RaisePropertyChanged(nameof(SelectedTileVisibility));
            }
        }

        public WorldViewModel()
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
            _scriptingService = ContainerService.Instance.GetOrCreate<ScriptingService>();
        }

        public WorldViewModel(string path) : this()
        {
            Path = path;
        }

        public void LoadAs()
        {
            var newPath = _routingService.ShowOpenFileDialog("Open World", Vars.WorldExtension, Vars.FileDialogTypeWorld);
            if (newPath != null)
            {
                Path = newPath;
                Load();
            }
        }

        public void Load()
        {
            if (!File.Exists(Path))
                Path = null;
            else
            {
                var worldDef = JsonConvert.DeserializeObject<World>(File.ReadAllText(Path));
                Load(worldDef);
                _settingsService.Data.LastWorldPath = Path;
                _settingsService.SaveWithLock();
            }
        }

        public void New()
        {
            var confirm = _routingService.ShowConfirmDialog("Warning", "Your unsaved changes will be lost. Are you sure?");
            if (confirm)
            {
                Path = null;
                Load(new World());
            }
        }
        public void Load(World worldDef)
        {
            if (worldDef == null) return;
            Boards = new ObservableCollection<BoardViewModel>(worldDef.Boards.Select(x => new BoardViewModel(this, x)));
            TileDefs = new ObservableCollection<TileDefViewModel>(worldDef.TileDefs.Select(x => new TileDefViewModel(this, x)));
            SpriteSheets = new ObservableCollection<SpriteSheetViewModel>(worldDef.SpriteSheets.Select(x => new SpriteSheetViewModel(x)));
            Width = worldDef.Width;
            Height = worldDef.Height;
            TileWidth = worldDef.TileWidth;
            TileHeight = worldDef.TileHeight;
            ReinitializeSpriteMap();
        }

        public World Serialize()
        {
            World worldDef = new World();
            worldDef.Width = Width;
            worldDef.Height = Height;
            worldDef.Boards = Boards.Select(x => x.Serialize()).ToList();
            worldDef.TileDefs = TileDefs.Select(x => x.Serialize()).ToList();
            worldDef.SpriteSheets = SpriteSheets.Select(x => x.Path).ToList();
            return worldDef;
        }

        public void SaveAs()
        {
            var newPath = _routingService.ShowSaveFileDialog("Save World", Vars.WorldExtension, Vars.FileDialogTypeWorld);
            if (newPath != null)
            {
                Path = newPath;
                Save();
            }
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                SaveAs();
                return;
            }
            var worldDef = Serialize();
            var json = JsonConvert.SerializeObject(worldDef, Formatting.Indented);
            File.WriteAllText(Path, json);
            _settingsService.Data.LastWorldPath = Path;
            _settingsService.SaveWithLock();
            _routingService.ShowDialog("Success", $"World saved to {Path}");
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

        public void RemoveBoard()
        {
            RemoveBoard(SelectedBoard);
        }

        public void RemoveBoard(BoardViewModel board)
        {
            if (board == null) return;
            if (SelectedBoard != null)
                if (_routingService.ShowConfirmDialog("Confirm Deletion", $"Board {board.Name} ({board.Id}) will be deleted.. Are you sure?"))
                {
                    if (SelectedBoard == board)
                        SelectedBoard = null;
                    Boards.Remove(board);
                }
        }

        public void MoveBoard(BoardViewModel board, int direction)
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

        public void AddSpriteSheet()
        {
            var path = _routingService.ShowOpenFileDialog("Open Sprite Sheet PNG", ".png", Vars.FileDialogTypePNG);
            AddSpriteSheet(path);
        }

        public void AddSpriteSheet(string path)
        {
            if (path != null)
            {
                var spriteSheet = new SpriteSheetViewModel { Path = path };
                spriteSheet.LoadFromFile();
                SpriteSheets.Add(spriteSheet);
                SelectedSpriteSheet = spriteSheet;
                ReinitializeSpriteMap();
            }
        }

        public void RemoveSpriteSheet(SpriteSheetViewModel spriteSheet)
        {
            if (SpriteSheets.Contains(spriteSheet))
            {
                if (SelectedSpriteSheet == spriteSheet)
                    SelectedSpriteSheet = null;
                SpriteSheets.Remove(spriteSheet);
                ReinitializeSpriteMap();
            }
        }

        public void RemoveSpriteSheet()
        {
            RemoveSpriteSheet(SelectedSpriteSheet);
        }

        public void ReinitializeSpriteMap()
        {
            SpriteMap.Clear();
            foreach (var spriteSheet in SpriteSheets)
                foreach (var sprite in spriteSheet.Sprites)
                    SpriteMap[sprite.RealChar] = sprite;
            SpriteMapList = SpriteMap.Values.ToList();
        }

        public void DuplicateBoard()
        {
            if (SelectedBoard == null) return;
            var board = SelectedBoard.Serialize();
            board.Id = Boards.Count.ToString();
            board.Name += " (Copy)";
            Boards.Add(new BoardViewModel(this, board));
        }

        public void ExportLevels()
        {
            var folderPath = _routingService.ShowOpenFolderDialog("Select Folder to Export Levels");
            if (folderPath != null)
            {
                _scriptingService.ExportBoardsToFolder(Boards.Select(x => x.Serialize()).ToList(), folderPath);
                _routingService.ShowDialog("Success", $"Levels exported to {folderPath}");
            }
        }

        public void ImportLevels()
        {
            var path = _routingService.ShowOpenFileDialog("Select levels.txt", ".txt", "levels.txt");
            if (path != null)
            {
                var boards = _scriptingService.ImportBoardsFromFolder(path);
                Boards = new ObservableCollection<BoardViewModel>(boards.Select(x => new BoardViewModel(this, x)));
                _routingService.ShowDialog("Success", $"Levels imported from {path}");
            }
        }

        public void EditSpriteSheet()
        {
            if (SelectedSpriteSheet == null) return;
            _routingService.ShowSpriteSheetEditor(SelectedSpriteSheet, SelectedSprite);
        }

        public void SelectTile(int x, int y)
        {
            if (SelectedBoard == null) return;
            SelectedBoard.TileGrid.TryGetValue((x, y), out var tile);
            SelectedTile = tile;
            if (SelectedTile != null && SpriteMap.TryGetValue(SelectedTile.Char, out var sprite))
                SelectedSprite = sprite;
        }

        public CommandModel NewCommand => new CommandModel(New);
        public CommandModel OpenCommand => new CommandModel(LoadAs);
        public CommandModel RevertCommand => new CommandModel(Load);
        public CommandModel SaveCommand => new CommandModel(Save);
        public CommandModel SaveAsCommand => new CommandModel(SaveAs);
        public CommandModel AddBoardCommand => new CommandModel(AddBoard);
        public CommandModel RemoveBoardCommand => new CommandModel(RemoveBoard);
        public CommandModel BoardSettingsCommand => new CommandModel(() => SelectedBoard?.ShowSettings());
        public CommandModel AddSpriteSheetCommand => new CommandModel(AddSpriteSheet);
        public CommandModel RemoveSpriteSheetCommand => new CommandModel(RemoveSpriteSheet);
        public CommandModel ShowTileDefsEditorCommand => new CommandModel(() => _routingService.ShowTileDefsEditor(this));
        public CommandModel ShowSpriteSheetManagerCommand => new CommandModel(() => _routingService.ShowWorldSpriteSheetManager(this));
        public CommandModel ShowSettingsCommand => new CommandModel(() => _routingService.ShowWorldSettings(this));
        public CommandModel ReinitializeSpriteMapCommand => new CommandModel(ReinitializeSpriteMap);
        public CommandModel MoveBoardUpCommand => new CommandModel(() => MoveBoard(SelectedBoard, -1));
        public CommandModel MoveBoardDownCommand => new CommandModel(() => MoveBoard(SelectedBoard, 1));
        public CommandModel ExportLevelsCommand => new CommandModel(ExportLevels);
        public CommandModel ImportLevelsCommand => new CommandModel(ImportLevels);
        public CommandModel DuplicateBoardCommand => new CommandModel(DuplicateBoard);
        public CommandModel EditSpriteSheetCommand => new CommandModel(EditSpriteSheet);
    }
}
