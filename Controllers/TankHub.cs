using Microsoft.AspNetCore.SignalR;
using Models;

namespace Controllers
{
    public class TankHub : Hub
    {
        public void MoveTank(int id, int x, int y)
        {
            Game.Tanks.Move(id, x, y);
        }

        public void Scream()
        {
            Log.Info("AAAAAHHH!");
        }
    }
}
