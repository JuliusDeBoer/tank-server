using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Tanks.Models;

namespace Tanks
{
    public static class Board
    {
        private static Dictionary<int, Models.Tank> Dictionary { get; set; } = new Dictionary<int, Models.Tank>();
        private static readonly Mutex _mutex = new();
        public static Dictionary<int, Models.Tank> Tanks
        {
            get
            {
                _mutex.WaitOne();
                Dictionary<int, Models.Tank> content = Dictionary;
                _mutex.ReleaseMutex();
                return content;
            }
            set
            {
                _mutex.WaitOne();
                Dictionary = value;
                _mutex.ReleaseMutex();
            }
        }
        public static int GetTankByPosition(Models.Position position)
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
