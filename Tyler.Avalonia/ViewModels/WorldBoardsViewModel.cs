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
    public class WorldBoardsViewModel : TinyViewModel
    {
        readonly RoutingService _routingService;
        readonly ScriptingService _scriptingService;

        readonly Dictionary<string, BoardViewModel> boardMap = new Dictionary<string, BoardViewModel>();

        public WorldViewModel World { get; }

        ObservableCollection<BoardViewModel> _boards = new ObservableCollection<BoardViewModel>();
        public ObservableCollection<BoardViewModel> Boards
        {
            get => _boards;
            set => SetProperty(ref _boards, value);
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
                if (isDirty) World.TilesManager.SelectedTile = null;
                RaisePropertyChanged(nameof(IsSelectedBoardVisible));
            }
        }

        public WorldBoardsViewModel(WorldViewModel world)
        {
            _routingService = ContainerService.Instance.Get<RoutingService>();
            _scriptingService = ContainerService.Instance.GetOrCreate<ScriptingService>();
            World = world;
        }

        public void Reload(World worldDef)
        {
            Boards = new ObservableCollection<BoardViewModel>(worldDef.Boards.Select(x => new BoardViewModel(World, x)));
            UpdateBoardMap();
            SelectedBoard = Boards.FirstOrDefault();
        }

        public void SerializeInto(World worldDef)
        {
            worldDef.Boards = Boards.Select(x => x.Serialize()).ToList();
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
                    UpdateBoardMap();
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
            Boards.Add(new BoardViewModel(World, board));
            UpdateBoardMap();
        }

        public void AddBoard()
        {
            var board = new BoardViewModel(World, new Board
            {
                Id = Boards.Count.ToString(),
                Width = World.Width,
                Height = World.Height
            });
            Boards.Add(board);
            SelectedBoard = board;
            UpdateBoardMap();
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
                Boards = new ObservableCollection<BoardViewModel>(boards.Select(x => new BoardViewModel(World, x)));
                UpdateBoardMap();
                await _routingService.ShowDialogAsync(default, "Success", $"Levels imported from {path}");
            }
        }

        public void SelectTile(int x, int y)
        {
            if (SelectedBoard == null) return;
            if (x < 0 || y < 0 || x >= SelectedBoard.Width || y >= SelectedBoard.Height) return;
            var tile = SelectedBoard.TileGrid[x, y];
            World.TilesManager.SelectTile(tile);
        }

        public CommandModel AddBoardCommand => new CommandModel(AddBoard);
        public CommandModel RemoveBoardCommand => new CommandModel(RemoveBoardAsync);
        public CommandModel BoardSettingsCommand => new CommandModel(() => SelectedBoard?.ShowSettings());
        public CommandModel MoveBoardUpCommand => new CommandModel(() => MoveBoard(SelectedBoard, -1));
        public CommandModel MoveBoardDownCommand => new CommandModel(() => MoveBoard(SelectedBoard, 1));
        public CommandModel ExportLevelsCommand => new CommandModel(ExportLevelsAsync);
        public CommandModel ImportLevelsCommand => new CommandModel(ImportLevelsAsync);
        public CommandModel DuplicateBoardCommand => new CommandModel(DuplicateBoard);
    }
}
