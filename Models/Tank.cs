using System.Security.Principal;

namespace TankServer.Models
{
    public enum Color
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
        White,
        Hotpink // Developer only
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

    public class Tanks
    {
        public const int MOVEMENT_RANGE = 4;

        public Dictionary<int, Tank> AllTanks = new();

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
            return Add(new Tank(GetUniqueId()));
        }

        public int Add(Tank tank)
        {
            AllTanks.Add(tank.Id, tank);
            return tank.Id;
        }

        public bool Move(int tankId, int x, int y)
        {
            Tank tank = AllTanks[tankId];

            if (!((MOVEMENT_RANGE * -1) <= x && x <= MOVEMENT_RANGE && (MOVEMENT_RANGE * -1) <= y && y <= MOVEMENT_RANGE))
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

    public static class TankEndpoints
    {
        public static readonly int MAX_TANKS = 255;
        public static readonly int MAX_LEVEL = 3;

        public class TankTotal
        {
            public int Total { get; set; }
            public List<Tank> Tanks { get; set; } = new List<Tank>();

            public TankTotal(Dictionary<int, Tank> tanks)
            {
                foreach (KeyValuePair<int, Tank> pair in tanks)
                {
                    Tanks.Add(pair.Value);
                    Total++;
                }
            }
        }

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

        public static void MapTankEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/tank");

            group.MapGet("/", () =>
            {
                Log.Info("Requested all tanks");
                return new TankTotal(Game.Tanks.AllTanks);
            })
            .WithName("GetAllTanks");

            group.MapGet("/{id}", (int id) =>
            {
                if (!Game.Tanks.Contains(id))
                {
                    Log.Error($"Attempted to get tank with id: {id}. Which doesn't exist");
                    return (IResult)TypedResults.NotFound(new Response("ERR_NO_TANK_FOUND", "Tank does not exist"));
                }
                Log.Info($"Got tank {id}");
                return (IResult)TypedResults.Ok(Game.Tanks.AllTanks[id]);
            })
            .WithName("GetTankById");

            group.MapPost("/move", (HttpContext context, int? x, int? y) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                if (x == null || y == null)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                int id = account.TankId;

                // TODO: Refactor this
                try
                {
                    if (!Game.Tanks.Move(id, (int)x, (int)y))
                    {
                        Log.Error(new Response("ERR_NOT_IMPLEMENTED", "Unable to move tank"));
                        return (IResult)TypedResults.BadRequest(new Response("ERR_NOT_IMPLEMENTED", "Unable to move tank"));
                    }
                    Log.Info($"Moved tank {id} {{x: {x}; y: {y}}}");
                }
                catch (KeyNotFoundException)
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                return Response.Ok(Response.OK);
            })
            .WithName("MoveTank");

            group.MapPost("/color", (HttpContext context, string color) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                int id = account.TankId;

                if (!Game.Tanks.Contains(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Color? parsed = ParseColor(color);

                if (parsed == null)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                Game.Tanks.AllTanks[id].Color = (Color)parsed;

                return Response.Ok(Response.OK);
            });

            group.MapPost("/shoot", (HttpContext context, int target) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                int id = account.TankId;

                if (!Game.Tanks.Contains(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                if (!Game.Tanks.Contains(target))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Tank origin = Game.Tanks.AllTanks[id];
                Tank dest = Game.Tanks.AllTanks[target];

                if (origin.ActionPoints <= 0)
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                if (dest.Health <= 0)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                // TODO: Add funcion for this
                origin.ActionPoints--;
                dest.Health--;

                return TypedResults.Ok(Response.OK);
            })
            .WithName("ShootTank");

            group.MapPost("/upgrade", (HttpContext context) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                int id = account.TankId;

                if (!Game.Tanks.Contains(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Tank tank = Game.Tanks.AllTanks[id];

                if (tank.ActionPoints <= 0)
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                if (tank.Level >= MAX_LEVEL)
                {
                    return Response.BadRequest(Response.ERR_MAX_LEVEL_REACHED);
                }

                if (!Game.Tanks.HasActionPoints(id, 1))
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                Game.Tanks.SpendActionPoints(id, 1);

                tank.Level++;

                return Response.Ok(Response.OK);
            })
            .WithName("UpgradeTank");

            group.MapPost("/give", (HttpContext context, int amount, int target) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                int id = account.TankId;

                if (amount <= 0)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                if (!Game.Tanks.Contains(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                if (!Game.Tanks.Contains(target))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Tank origin = Game.Tanks.AllTanks[id];
                Tank destination = Game.Tanks.AllTanks[target];

                if (origin.Health <= 0)
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                if (destination.Health <= 0)
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                // Actually spend points
                if (!Game.Tanks.HasActionPoints(id, amount))
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                Game.Tanks.GiveActionPoints(id, target, amount);
                
                return Response.Ok(Response.OK);
            })
            .WithName("GiveActionPoint");
        }
    }
}
