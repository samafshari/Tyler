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

        public WorldViewModel World { get; } = new WorldViewModel();

        string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
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
            set => SetProperty(ref _name, value);
        }

        ObservableCollection<TileViewModel> _tiles = new ObservableCollection<TileViewModel>();
        public ObservableCollection<TileViewModel> Tiles
        {
            get => _tiles;
            set => SetProperty(ref _tiles, value);
        }

        public BoardViewModel() : this(null, new Board())
        {
        }

        public BoardViewModel(WorldViewModel world, Board board)
        {
            _routingService = ContainerService.Instance.GetOrCreate<RoutingService>();
            World = world;
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
    }
}
