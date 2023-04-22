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
        private static Dictionary<string, Account> PAccounts { get; set; } = new Dictionary<string, Account>();
        private static readonly Mutex _accountsMutex = new();
        public static Dictionary<string, Account> Accounts
        {
            get
            {
                _accountsMutex.WaitOne();
                Dictionary<string, Account> content = PAccounts;
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

        // Password must be hashed beforehand
        public static void CreateAccount(string username, string email, string password)
        {
            int id = Tanks.Count + 1;

            Tanks.Add(id, new Tank(id));
            Accounts.Add(email, new Account(username, email, password, id));
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
