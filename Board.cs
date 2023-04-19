using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Tanks.Models;

namespace Tanks
{
    public static class BoardMutex
    {
        private static readonly Mutex _mutex = new();
        private static Board _board = new();
        public static Board Board
        {
            get
            {
                _mutex.WaitOne();
                Board content = _board;
                _mutex.ReleaseMutex();
                return content;
            }
            set
            {
                _mutex.WaitOne();
                _board = value;
                _mutex.ReleaseMutex();
            }
        }
    }

    public class Board
    {
        public Dictionary<int, Models.Tank> Tanks { get; set; } = new Dictionary<int, Models.Tank>();

        int GetTankByPosition(Models.Position position)
        {
            foreach (KeyValuePair<int, Tank> pair in Tanks)
            {
                if(pair.Value.Position == position)
                {
                    return pair.Key;
                }
            }

            return 0;
        }
    }
}
