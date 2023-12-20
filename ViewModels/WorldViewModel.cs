using Net.Essentials;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;
using Tyler.Services;

namespace Tyler.ViewModels
{
    public class WorldViewModel : ViewModel
    {
        readonly RoutingService _routingService;
        readonly SettingsService _settingsService;

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

        BoardViewModel _selectedBoard;
        public BoardViewModel SelectedBoard
        {
            get => _selectedBoard;
            set => SetProperty(ref _selectedBoard, value);
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

        public WorldViewModel()
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
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
                Boards.Clear();
                TileDefs.Clear();
                SpriteSheets.Clear();
                Width = 0;
                Height = 0;
            }
        }
        public void Load(World worldDef)
        {
            if (worldDef == null) return;
            Boards = new ObservableCollection<BoardViewModel>(worldDef.Boards.Select(x => new BoardViewModel(this, x)));
            TileDefs = new ObservableCollection<TileDefViewModel>(worldDef.TileDefs.Select(x => new TileDefViewModel(this, x)));
            SpriteSheets = new ObservableCollection<SpriteSheetViewModel>(worldDef.SpriteSheets.Select(x => new SpriteSheetViewModel { Path = x }));
            Width = worldDef.Width;
            Height = worldDef.Height;
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

        public CommandModel NewCommand => new CommandModel(New);
        public CommandModel OpenCommand => new CommandModel(LoadAs);
        public CommandModel RevertCommand => new CommandModel(Load);
        public CommandModel SaveCommand => new CommandModel(Save);
        public CommandModel SaveAsCommand => new CommandModel(SaveAs);
        public CommandModel AddBoardCommand => new CommandModel(AddBoard);
        public CommandModel RemoveBoardCommand => new CommandModel(RemoveBoard);
        public CommandModel AddSpriteSheetCommand => new CommandModel(AddSpriteSheet);
        public CommandModel RemoveSpriteSheetCommand => new CommandModel(RemoveSpriteSheet);
        public CommandModel ShowTileDefsEditorCommand => new CommandModel(() => _routingService.ShowTileDefsEditor(this));
        public CommandModel ShowSpriteSheetManagerCommand => new CommandModel(() => _routingService.ShowWorldSpriteSheetManager(this));
        public CommandModel ReinitializeSpriteMapCommand => new CommandModel(ReinitializeSpriteMap);
    }
}
