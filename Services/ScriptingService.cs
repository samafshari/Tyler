using Net.Essentials;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tyler.Models;

namespace Tyler.Services
{
    public class ScriptingService : Singleton<ScriptingService>
    {
        public char[] BoardToCharGrid(Board board)
        {
            var sz = board.Width * board.Height;
            var grid = new char[sz];
            var sortedTiles = board.Tiles
                .OrderBy(t => t.Z)
                .GroupBy(t => t.Y * board.Height + t.X)
                .ToDictionary(t => t.Key, t => t.FirstOrDefault());

            for (var i = 0; i < sz; i++)
            {
                char c = Vars.DefaultChar;
                if (sortedTiles.ContainsKey(i)) c = sortedTiles[i].Char;
                grid[i] = c;
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
                    sb.Append(charGrid[i].ToString());
                sb.AppendLine();
            }
            sb.AppendLine("ENDGRID");
            sb.AppendLine($"board_id {board.Id}");
            sb.AppendLine($"board_name {board.Name}");
            sb.AppendLine();
            foreach (var tile in board.Tiles)
            {
                if (!string.IsNullOrWhiteSpace(tile.Script))
                {
                    sb.AppendLine($"pos {tile.X},{tile.Y}");
                    sb.AppendLine(tile.Script);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        public Board ScriptToBoard(string script)
        {
            var board = new Board();
            if (string.IsNullOrWhiteSpace(script)) return board;

            var lines = script.Split('\n').Select(x => x.Trim()).ToArray();
            if (!lines.Any()) return board;

            board.Height = 0;
            board.Width = lines[0].Length;
            bool isReadingGrid = true;
            var tileMap = new Dictionary<(int, int), Tile>();
            Tile selE = null;

            for (int i = 0; i < lines.Length; i++)
            {
                if (isReadingGrid)
                {
                    if (lines[i] == "ENDGRID")
                    {
                        isReadingGrid = false;
                        continue;
                    }

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
                var command = split < 0 ? "" : lower.Substring(0, split);
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
    }
}
