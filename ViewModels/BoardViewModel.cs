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
            set => SetProperty(ref _width, value);
        }

        int _height;
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
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

        string _script;
        public string Script
        {
            get => _script;
            set => SetProperty(ref _script, value);
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
