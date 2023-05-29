using System.Threading.Tasks;

namespace Models
{
    public class TankTotal
    {
        public int Total { get; set; }
        public List<Tank> Tanks { get; set; } = new List<Tank>();

        public TankTotal(Dictionary<int, Tank> tanks)
        {
            foreach (KeyValuePair<int, Tank> pair in tanks)
            {
                if (pair.Value.Health >= 1)
                {
                    Tanks.Add(pair.Value);
                    Total++;
                }
            }
        }
    }

    public enum Color
    {
        Red = 0,
        Orange = 1,
        Yellow = 2,
        Green = 3,
        Blue = 4,
        Purple = 5,
        White = 6,
        Hotpink = 7 // Really cool and secret!
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class TankCollection
    {
        public static readonly int MAX_TANKS = 255;
        public static readonly int MAX_LEVEL = 3;

        public static readonly int FIELD_WIDTH = 32;
        public static readonly int FIELD_HEIGHT = 24;

        public const int MOVEMENT_RANGE = 4;

        public Dictionary<int, Tank> AllTanks = new();

        public static Color? ParseColor(string color)
        {
            return color.ToLower() switch
            {
                "red" => Color.Red,
                "orange" => Color.Orange,
                "yellow" => Color.Yellow,
                "green" => Color.Green,
                "blue" => Color.Blue,
                "purple" => Color.Purple,
                "white" => Color.White,
                "hotpink" => Color.Hotpink,
                _ => null
            };
        }

        public int GetUniqueId()
        {
            int id = 1;
            while (AllTanks.ContainsKey(id))
            {
                id++;
            }

            return id;
        }

        public int New()
        {
            Tank tank = new(GetUniqueId());
            Random rand = new();

            // Lets hope I'm lucky
            for(int i = 0; i <= 1000; i++)
            {
                int x = rand.Next(FIELD_WIDTH + 1);
                int y = rand.Next(FIELD_HEIGHT + 1);

                if (!IsPosOccupied(x, y))
                {
                    tank.Position = new(x, y);
                    break;
                }
            }

            return Add(tank);
        }

        public int Add(Tank tank)
        {
            AllTanks.Add(tank.Id, tank);
            return tank.Id;
        }

        public bool Move(int tankId, int x, int y)
        {
            Tank tank = AllTanks[tankId];

            // Check if movement is within range
            if (!((MOVEMENT_RANGE * -1) <= x && x <= MOVEMENT_RANGE && (MOVEMENT_RANGE * -1) <= y && y <= MOVEMENT_RANGE))
            {
                return false;
            }

            // Check if movement is outside of the field
            if(tank.Position.X + x > FIELD_WIDTH
                || tank.Position.Y + y > FIELD_HEIGHT
                || tank.Position.X + x < 0
                || tank.Position.Y + y < 0)
            {
                return false;
            }


            tank.Position.X += x;
            tank.Position.Y += y;

            return true;
        }

        public bool Contains(int id)
        {
            return AllTanks.ContainsKey(id);
        }

        public bool IsPosOccupied(int x, int y)
        {
            foreach (KeyValuePair<int, Tank> pair in AllTanks)
            {
                if(pair.Value.Position.X == x
                    && pair.Value.Position.Y == y)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasActionPoints(int id, int amount)
        {
            return AllTanks[id].ActionPoints >= amount;
        }

        public void SpendActionPoints(int id, int amount)
        {
            AllTanks[id].ActionPoints -= amount;
        }

        internal void GiveActionPoints(int from, int to, int amount)
        {
            AllTanks[from].ActionPoints -= amount;
            AllTanks[to].ActionPoints += amount;
        }
    }

    public class Tank
    {
        public int Id { get; set; }
        public int Health { get; set; } = 3;
        public int Level { get; set; } = 1;
#if DEBUG
        public int ActionPoints { get; set; } = 10;
#else
        public int ActionPoints { get; set; } = 0;
#endif
        public Color Color { get; set; } = Color.Green;
        // Only use if MOVEMENT_RANGE should be ignored. Otherwise use Move()
        public Position Position { get; set; } = new(0, 0);

        public Tank(int id)
        {
            Id = id;
        }
    }
}
