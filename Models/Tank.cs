using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System;
using System.Numerics;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Tanks.Models
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

    public class Tank
    {
        public const int MOVEMENT_RANGE = 4;

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
        public Position Position { get; set; } = new Position(0, 0);

        public Tank(int id)
        {
            Id = id;
        }
        
        // Returns false if the tank has no action points left
        public bool SpendActionPoint()
        {
            if (ActionPoints <= 0)
            {
                return false;
            }

            ActionPoints--;
            return true;
        }

        // Returns false if the tank has not enough action points
        public bool SpendActionPoint(int amount)
        {
            if (ActionPoints < amount)
            {
                return false;
            }

            ActionPoints -= amount;
            return true;
        }

        // Returns false if move was invalid
        public bool Move(int x, int y)
        {
            // Dont have enough action points
            if (!SpendActionPoint())
            {
                return false;
            }

            // Check if specified move is within range
            if (!((MOVEMENT_RANGE * -1) <= x && x <= MOVEMENT_RANGE && (MOVEMENT_RANGE * -1) <= y && y <= MOVEMENT_RANGE))
            {
                return false;
            }

            Position.X += x;
            Position.Y += y;

            return true;
        }
    }

    public static class TankEndpoints
    {
        public static readonly int MAX_TANKS = 255;
        public static readonly int MAX_LEVEL = 3; 

        public class TankTotal {
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
                Log.Info("Requested all PTanks");
                return new TankTotal(Game.Tanks);
            })
            .WithName("GetAllTanks");

            group.MapGet("/{id}", (int id) =>
            {
                if (!Game.Tanks.ContainsKey(id))
                {
                    Log.Error($"Attempted to get tank with id: {id}. Which doesn't exist");
                    return (IResult)TypedResults.NotFound(new Response("ERR_NO_TANK_FOUND", "Tank does not exist"));
                }
                Log.Info($"Got tank {id}");
                return (IResult)TypedResults.Ok(Game.Tanks[id]);
            })
            .WithName("GetTankById");

            group.MapPost("/create", () =>
            {
                if (Game.Tanks.Count >= MAX_TANKS)
                {
                    return Response.BadRequest(Response.ERR_MAX_TANKS_REACHED);
                }

                // Make sure that the key is unique. Should in theory never be needed
                int id = Game.Tanks.Count + 1;
                while (Game.Tanks.ContainsKey(id))
                {
                    id++;
                }
                Game.Tanks.Add(id, new Tank(id));
                Log.Info($"Created new tank with id: {id}");
                return (IResult)TypedResults.Created($"/api/v1/PTanks/{id}", Game.Tanks[id]);
            })
            .WithName("CreateTank");

            group.MapPost("/{id}/move", (int id, int? x, int? y) =>
            {
                if (x == null || y == null)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                // TODO: Refactor this
                try
                {
                    if (!Game.Tanks[id].Move((int)x, (int)y))
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

            group.MapPost("/{id}/color", (int id, string color) =>
            {
                if (!Game.Tanks.ContainsKey(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Color? parsed = ParseColor(color);

                if (parsed == null)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                Game.Tanks[id].Color = (Color)parsed;

                return Response.Ok(Response.OK);
            });

            group.MapPost("/{id}/shoot", (int id, int target) =>
            {
                if (!Game.Tanks.ContainsKey(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                if (!Game.Tanks.ContainsKey(target))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Tank origin = Game.Tanks[id];
                Tank dest = Game.Tanks[target];

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

            group.MapPost("/{id}/upgrade", (int id) =>
            {
                if (!Game.Tanks.ContainsKey(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Tank tank = Game.Tanks[id];

                if (tank.ActionPoints <= 0)
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                if (tank.Level >= MAX_LEVEL)
                {
                    return Response.BadRequest(Response.ERR_MAX_LEVEL_REACHED);
                }

                if (!tank.SpendActionPoint())
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                tank.Level++;

                return Response.Ok(Response.OK);
            })
            .WithName("UpgradeTank");

            group.MapPost("/{id}/give", (int id, int amount, int target) =>
            {
                if(amount <= 0)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                if (!Game.Tanks.ContainsKey(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                if (!Game.Tanks.ContainsKey(target))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Tank origin = Game.Tanks[id];
                Tank destination = Game.Tanks[target];

                if(origin.Health <= 0)
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                if (destination.Health <= 0)
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                // Actually spend points
                if(!origin.SpendActionPoint(amount))
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                destination.ActionPoints += amount;

                return Response.Ok(Response.OK);
            })
            .WithName("GiveActionPoint");
        }
    }
}
