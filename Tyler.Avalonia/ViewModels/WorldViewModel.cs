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

        Dictionary<string, SpriteSheetViewModel> spriteSheetsMap = new Dictionary<string, SpriteSheetViewModel>();
        Dictionary<string, SpriteViewModel> spritesMap = new Dictionary<string, SpriteViewModel>();
        Dictionary<string, BoardViewModel> boardMap = new Dictionary<string, BoardViewModel>();

        public enum Tabs
        {
            Boards,
            Sprites
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

        ObservableCollection<SpriteSheetViewModel> _spriteSheets = new ObservableCollection<SpriteSheetViewModel>();
        public ObservableCollection<SpriteSheetViewModel> SpriteSheets
        {
            get => _spriteSheets;
            set => SetProperty(ref _spriteSheets, value);
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

        public Dictionary<char, SpriteViewModel> SpriteCharMap = new Dictionary<char, SpriteViewModel>();
        List<SpriteViewModel> _spriteMapList = new List<SpriteViewModel>();
        public List<SpriteViewModel> SpriteMapList
        {
            get => _spriteMapList;
            set => SetProperty(ref _spriteMapList, value);
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
            Boards = new ObservableCollection<BoardViewModel>(worldDef.Boards.Select(x => new BoardViewModel(this, x)));
            TileDefs = new ObservableCollection<TileDefViewModel>(worldDef.TileDefs.Select(x => new TileDefViewModel(this, x)));
            SpriteSheets = new ObservableCollection<SpriteSheetViewModel>(worldDef.SpriteSheets.Select(x => new SpriteSheetViewModel(x)));
            Width = worldDef.Width;
            Height = worldDef.Height;
            TileWidth = worldDef.TileWidth;
            TileHeight = worldDef.TileHeight;
            ReinitializeSpriteMap();
            UpdateBoardMap();
            UpdateSpriteSheetsMap();
            UpdateSpritesMap();
        }

        public World Serialize()
        {
            World worldDef = new World();
            worldDef.Width = Width;
            worldDef.Height = Height;
            worldDef.Boards = Boards.Select(x => x.Serialize()).ToList();
            worldDef.TileDefs = TileDefs.Select(x => x.Serialize()).ToList();
            worldDef.SpriteSheets = SpriteSheets.Select(x => x.Serialize()).ToList();
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
                SpriteSheets.Add(spriteSheet);
                SelectedSpriteSheet = spriteSheet;
                ReinitializeSpriteMap();
                spriteSheet.IdChanged += SpriteSheet_IdChanged;
                spriteSheet.SpriteIdChanged += SpriteSheet_SpriteIdChanged;
                spriteSheet.SpriteListChanged += SpriteSheet_SpriteListChanged;
                SelectedTab = Tabs.Sprites;
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
            if (spriteSheetsMap.ContainsKey(e.OldName) && spriteSheetsMap[e.OldName] == sender!)
                spriteSheetsMap.Remove(e.OldName);
            spriteSheetsMap[e.NewName] = (SpriteSheetViewModel)sender!;
        }

        private void SpriteSheet_SpriteIdChanged(object? sender, NameChangeEventArgs e)
        {
            if (spritesMap.ContainsKey(e.OldName) && spritesMap[e.OldName] == sender!)
                spritesMap.Remove(e.OldName);
            spritesMap[e.NewName] = (SpriteViewModel)sender!;
        }

        public void ReinitializeSpriteMap()
        {
            SpriteCharMap.Clear();
            spritesMap.Clear();
            foreach (var spriteSheet in SpriteSheets)
                foreach (var sprite in spriteSheet.Sprites)
                {
                    SpriteCharMap[sprite.RealChar] = sprite;
                    spritesMap[sprite.Id] = sprite;
                }
            SpriteMapList = SpriteCharMap.Values.ToList();
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

        public void EditSpriteSheet()
        {
            if (SelectedSpriteSheet == null) return;
            SelectedTab = Tabs.Sprites;
        }

        public void EditTileDef()
        {
            if (SelectedSprite == null) return;
            var sprite = SpriteCharMap[SelectedSprite.RealChar];
            var spritesheet = SpriteSheets.First(x => x.Sprites.Contains(sprite));
            SelectedSpriteSheet = spritesheet;
            EditSpriteSheet();
        }

        public void SelectTile(int x, int y)
        {
            if (SelectedBoard == null) return;
            if (x < 0 || y < 0 || x >= SelectedBoard.Width || y >= SelectedBoard.Height) return;
            var tile = SelectedBoard.TileGrid[x, y];
            SelectedTile = tile;
            if (SelectedTile != null && SpriteCharMap.TryGetValue(SelectedTile.Char, out var sprite))
                SelectedSprite = sprite;
        }

        void UpdateBoardMap()
        {
            boardMap.Clear();
            foreach (var board in Boards.Where(x => x.Id != null))
                boardMap[board.Id!] = board;
        }

        void UpdateSpriteSheetsMap()
        {
            spriteSheetsMap.Clear();
            foreach (var spriteSheet in SpriteSheets.Where(x => x.Id != null))
                spriteSheetsMap[spriteSheet.Id!] = spriteSheet;
        }

        void UpdateSpritesMap()
        {
            spritesMap.Clear();
            foreach (var spriteSheet in SpriteSheets)
                foreach (var sprite in spriteSheet.Sprites.Where(x => x.Id != null))
                    spritesMap[sprite.Id!] = sprite;
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
        public CommandModel AddSpriteSheetCommand => new CommandModel(AddSpriteSheetAsync);
        public CommandModel RemoveSpriteSheetCommand => new CommandModel(RemoveSpriteSheet);
        public CommandModel ShowTileDefsEditorCommand => new CommandModel(() => _routingService.ShowTileDefsEditor(this));
        public CommandModel ShowSpriteSheetManagerCommand => new CommandModel(() => _routingService.ShowWorldSpriteSheetManager(this));
        public CommandModel ShowSettingsCommand => new CommandModel(() => _routingService.ShowWorldSettings(this));
        public CommandModel ReinitializeSpriteMapCommand => new CommandModel(() =>
        {
            ReinitializeSpriteMap();
            UpdateBoardMap();
            UpdateSpriteSheetsMap();
            UpdateSpritesMap();
        });
        public CommandModel MoveBoardUpCommand => new CommandModel(() => MoveBoard(SelectedBoard, -1));
        public CommandModel MoveBoardDownCommand => new CommandModel(() => MoveBoard(SelectedBoard, 1));
        public CommandModel ExportLevelsCommand => new CommandModel(ExportLevelsAsync);
        public CommandModel ImportLevelsCommand => new CommandModel(ImportLevelsAsync);
        public CommandModel DuplicateBoardCommand => new CommandModel(DuplicateBoard);
        public CommandModel EditSpriteSheetCommand => new CommandModel(EditSpriteSheet);
        public CommandModel EditTileDefCommand => new CommandModel(EditTileDef);
        public CommandModel ShowBenchmarksCommand => new CommandModel(_routingService.ShowBenchmarks);
    }
}
