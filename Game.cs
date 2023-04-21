using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Tanks.Models;

namespace Tanks
{
    public static class Game
    {
        private static Dictionary<int, Models.Tank> PTanks { get; set; } = new Dictionary<int, Models.Tank>();
        private static readonly Mutex _tanksMutex = new();
        public static Dictionary<int, Models.Tank> Tanks
        {
            get
            {
                _tanksMutex.WaitOne();
                Dictionary<int, Models.Tank> content = PTanks;
                _tanksMutex.ReleaseMutex();
                return content;
            }
            set
            {
                _tanksMutex.WaitOne();
                PTanks = value;
                _tanksMutex.ReleaseMutex();
            }
        }
        private static Dictionary<int, Account> PAccounts { get; set; } = new Dictionary<int, Account>();
        private static readonly Mutex _accountsMutex = new();
        public static Dictionary<int, Account> Accounts
        {
            get
            {
                _accountsMutex.WaitOne();
                Dictionary<int, Account> content = PAccounts;
                _accountsMutex.ReleaseMutex();
                return content;
            }
            set
            {
                _accountsMutex.WaitOne();
                PAccounts = value;
                _accountsMutex.ReleaseMutex();
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
