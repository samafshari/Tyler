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
            throw new NotImplementedException();
        }
    }
}
