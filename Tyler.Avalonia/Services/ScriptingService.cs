using Net.Essentials;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;

namespace Tyler.Services
{
    public class ScriptingService : Singleton<ScriptingService>
    {
        public static char[,] BoardToCharGrid(Board board)
        {
            var grid = new char[board.Width, board.Height];
            var sortedTiles = board.Tiles
                .OrderBy(t => t.Z)
                .GroupBy(t => (t.X, t.Y))
                .ToDictionary(t => t.Key, t => t.FirstOrDefault());

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (sortedTiles.TryGetValue((x, y), out var tile) && tile != null)
                        grid[x, y] = tile.Char;
                    else
                        grid[x, y] = Vars.EmptyTileChar;
                }
            }

            return grid;
        }

        public string BoardToScript(Board board)
        {
            var sb = new StringBuilder();
            var charGrid = BoardToCharGrid(board);
            var i = 0;
            for (int y = 0; y < board.Height; y++)
            { 
                for (int x = 0; x < board.Width; x++, i++)
                    sb.Append(charGrid[x, y].ToString());
                sb.AppendLine();
            }
            sb.AppendLine("ENDGRID");
            sb.AppendLine($"board_id {board.Id}");
            sb.AppendLine($"board_name {board.Name}");
            if (!string.IsNullOrWhiteSpace(board.BeforeScript))
            {
                sb.AppendLine("begin_section");
                sb.AppendLine(board.BeforeScript.Trim());
                sb.AppendLine("end_before_script");
            }
            sb.AppendLine();
            foreach (var tile in board.Tiles)
            {
                if (!string.IsNullOrWhiteSpace(tile.Script))
                {
                    sb.AppendLine($"pos {tile.X},{tile.Y}");
                    sb.AppendLine(tile.Script.Trim());
                    sb.AppendLine();
                }
            }
            if (!string.IsNullOrWhiteSpace(board.AfterScript))
            {
                sb.AppendLine("begin_section");
                sb.AppendLine(board.AfterScript.Trim());
                sb.AppendLine("end_after_script");
            }
            return sb.ToString();
        }

        public Board ScriptToBoard(string script)
        {
            var board = new Board();
            if (string.IsNullOrWhiteSpace(script)) return board;

            var lines = script.Replace("\r", "").Split('\n').Select(x => x.Trim()).ToArray();
            if (!lines.Any()) return board;

            board.Height = 0;
            board.Width = lines[0].Length;
            bool isReadingGrid = true;
            var sb = new StringBuilder();
            bool isReadingSection = false;

            var tileMap = new Dictionary<(int, int), Tile>();
            Tile? selE = null;

            for (int i = 0; i < lines.Length; i++)
            {
                if (isReadingGrid)
                {
                    if (lines[i] == "ENDGRID")
                    {
                        isReadingGrid = false;
                        continue;
                    }
                    if (lines[i].Length != board.Width)
                        throw new Exception($"Line sizes don't match");
                    for (int x = 0; x < lines[i].Length; x++)
                    {
                        var tile = new Tile
                        {
                            Char = lines[i][x],
                            X = x,
                            Y = i,
                            Z = 0,
                            Script = ""
                        };
                        tileMap[(x, i)] = tile;
                        board.Tiles.Add(tile);
                    }
                    board.Height++;
                    continue;
                }


                var lower = lines[i].ToLower();
                var split = lower.IndexOf(' ');
                var command = split < 0 ? lower : lower.Substring(0, split);
                if (command == null) command = "";
                if (isReadingSection)
                {
                    if (command == "end_before_script")
                    {
                        isReadingSection = false;
                        board.BeforeScript = sb.ToString().Trim();
                        continue;
                    }
                    if (command == "end_after_script")
                    {
                        isReadingSection = false;
                        board.AfterScript = sb.ToString().Trim();
                        continue;
                    }

                    sb.AppendLine(lines[i]);
                    continue;
                }
                if (command == "begin_section")
                {
                    isReadingSection = true;
                    sb.Clear();
                    continue;
                }
                if (command == "board_name")
                    board.Name = lines[i].Substring(split + 1);
                else if (command == "board_id")
                    board.Id = lines[i].Substring(split + 1);
                else if (command == "pos")
                {
                    var args = lines[i].Substring(split + 1).Split(',');
                    var pX = int.Parse(args[0]);
                    var pY = int.Parse(args[1]);
                    tileMap.TryGetValue((pX, pY), out selE);
                }
                else if (selE != null)
                {
                    if (selE.Script == null) selE.Script = "";
                    selE.Script += $"{lines[i]}\n";
                }
            }

            return board;
        }

        public void ExportBoardsToFolder(List<Board> boards, string folderPath)
        {
            Directory.CreateDirectory(folderPath);
            var mapsDirectory = Path.Combine(folderPath, "maps");
            Directory.CreateDirectory(mapsDirectory);
            int i = 0;
            var fileNames = new List<string>();
            foreach (var board in boards)
            {
                var script = BoardToScript(board);
                var fileName = $"{i++}.txt";
                fileNames.Add(fileName);
                var path = Path.Combine(mapsDirectory, fileName);
                File.WriteAllText(path, script);
            }
            File.WriteAllLines(Path.Combine(folderPath, "levels.txt"), fileNames);
        }

        public List<Board> ImportBoardsFromFolder(string levelsListPath)
        {
            var boards = new List<Board>();
            var lines = File.ReadAllLines(levelsListPath);
            var folderPath = Path.GetDirectoryName(levelsListPath) ?? "";
            foreach (var line in lines)
            {
                var path = Path.Combine(folderPath, line);
                var script = File.ReadAllText(path);
                var board = ScriptToBoard(script);
                boards.Add(board);
            }
            return boards;
        }
    }
}
