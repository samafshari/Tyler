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
        

        public WorldSpriteSheetsViewModel SpriteSheetsManager { get; }
        public WorldBoardsViewModel BoardsManager { get; }
        public WorldTilesViewModel TilesManager { get; }

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

        public WorldViewModel()
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            _settingsService = ContainerService.Instance.GetOrCreate<SettingsService>();
            _scriptingService = ContainerService.Instance.GetOrCreate<ScriptingService>();
            SpriteSheetsManager = new WorldSpriteSheetsViewModel(this);
            BoardsManager = new WorldBoardsViewModel(this);
            TilesManager = new WorldTilesViewModel(this);
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
            BoardsManager.Reload(worldDef);
            TilesManager.Reload(worldDef);
        }

        public World Serialize()
        {
            World worldDef = new World();
            worldDef.Width = Width;
            worldDef.Height = Height;
            SpriteSheetsManager.SerializeInto(worldDef);
            BoardsManager.SerializeInto(worldDef);
            TilesManager.SerializeInto(worldDef);
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

        public override string ToString()
        {
            return Path ?? "Untitled";
        }

        public CommandModel NewCommand => new CommandModel(New);
        public CommandModel OpenCommand => new CommandModel(LoadAsAsync);
        public CommandModel RevertCommand => new CommandModel(RevertAsync);
        public CommandModel SaveCommand => new CommandModel(SaveAsync);
        public CommandModel SaveAsCommand => new CommandModel(SaveAsAsync);
        public CommandModel ShowSettingsCommand => new CommandModel(() => _routingService.ShowWorldSettings(this));
        public CommandModel ShowBenchmarksCommand => new CommandModel(_routingService.ShowBenchmarks);
    }
}
