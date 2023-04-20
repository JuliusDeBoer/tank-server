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
        public int Range { get; set; } = 1;
        public int ActionPoints { get; set; } = 10; // DEBUG. REMOVE FOR PRODUCTION
        public Color Color { get; set; } = Color.Green;
        // Only use if MOVEMENT_RANGE should be ignored. Otherwise use Move()
        public Position Position { get; set; } = new Position(0, 0);

        public Tank()
        {
            Id = -1;
        }

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
        public const int MAX_TANKS = 255;

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
                Log.Info("Requested all tanks");
                return new TankTotal(Board.Tanks);
            })
            .WithName("GetAllTanks");

            group.MapGet("/{id}", (int id) =>
            {
                if (!Board.Tanks.ContainsKey(id))
                {
                    Log.Error($"Attempted to get tank with id: {id}. Which doesn't exist");
                    return (IResult)TypedResults.NotFound(new Response("ERR_NO_TANK_FOUND", "Tank does not exist"));
                }
                Log.Info($"Got tank {id}");
                return (IResult)TypedResults.Ok(Board.Tanks[id]);
            })
            .WithName("GetTankById");

            group.MapPost("/create", () =>
            {
                if (Board.Tanks.Count >= MAX_TANKS)
                {
                    Log.Error("Maximum number of tanks reached");
                    return (IResult)TypedResults.Problem("Maximum number of tanks reached");
                }

                // Mke sure that the key is unique. Should in theory never be needed
                int id = Board.Tanks.Count + 1;
                while (Board.Tanks.ContainsKey(id))
                {
                    id++;
                }
                Board.Tanks.Add(id, new Tank(id));
                Log.Info($"Created new tank with id: {id}");
                return (IResult)TypedResults.Created($"/api/v1/tanks/{id}", Board.Tanks[id]);
            })
            .WithName("CreateTank");

            group.MapPost("/{id}/move", (int id, int? x, int? y) =>
            {
                if (x == null || y == null)
                {
                    Log.Error("Move was sent. But the position was not correctly specified.");
                    return (IResult)TypedResults.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                // TODO: Refactor this
                try
                {
                    if (!Board.Tanks[id].Move((int)x, (int)y))
                    {
                        Log.Error(new Response("ERR_NOT_IMPLEMENTED", "Unable to move tank"));
                        return (IResult)TypedResults.BadRequest(new Response("ERR_NOT_IMPLEMENTED", "Unable to move tank"));
                    }
                    Log.Info($"Moved tank {id} {{x: {x}; y: {y}}}");
                }
                catch (KeyNotFoundException)
                {
                    Log.Error(Response.ERR_NO_SUCH_TANK);
                    return (IResult)TypedResults.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                return (IResult)TypedResults.Ok(Response.OK);
            })
            .WithName("MoveTank");

            group.MapPost("/{id}/color", (int id, string color) =>
            {
                if (!Board.Tanks.ContainsKey(id))
                {
                    Log.Error(Response.ERR_NO_SUCH_TANK);
                    return (IResult)TypedResults.NotFound(Response.ERR_NO_SUCH_TANK);
                }

                Color? parsed = ParseColor(color);

                if (parsed == null)
                {
                    Log.Error($"\"{color}\" is not a recognized color");
                    return (IResult)TypedResults.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                Board.Tanks[id].Color = (Color)parsed;

                Log.Info("Successfully changed color");
                return (IResult)TypedResults.Ok(Response.OK);
            });

            group.MapPost("/{id}/shoot", (int id, int target) =>
            {
                if (!Board.Tanks.ContainsKey(id))
                {
                    Log.Error(Response.ERR_NO_SUCH_TANK);
                    return (IResult)TypedResults.NotFound(Response.ERR_NO_SUCH_TANK);
                }

                if (!Board.Tanks.ContainsKey(target))
                {
                    Log.Error(Response.ERR_NO_SUCH_TANK);
                    return (IResult)TypedResults.NotFound(Response.ERR_NO_SUCH_TANK);
                }

                Tank origin = Board.Tanks[id];
                Tank dest = Board.Tanks[target];

                if (origin.ActionPoints <= 0)
                {
                    Log.Error("Not enough action points");
                    return (IResult)TypedResults.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                if (dest.Health <= 0)
                {
                    Log.Error("Target already dead");
                    return (IResult)TypedResults.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                // TODO: Add funcion for this
                origin.ActionPoints--;
                dest.Health--;

                return TypedResults.Ok(Response.OK);
            })
            .WithName("ShootTank");

            group.MapPost("/{id}/give", (int id, int amount, int target) =>
            {
                if(amount <= 0)
                {
                    Log.Error("Amount must be higher that 0");
                    return (IResult)TypedResults.BadRequest(Response.OK);
                }

                if (!Board.Tanks.ContainsKey(id))
                {
                    Log.Error(Response.ERR_NO_SUCH_TANK);
                    return (IResult)TypedResults.NotFound(Response.ERR_NO_SUCH_TANK);
                }

                if (!Board.Tanks.ContainsKey(target))
                {
                    Log.Error(Response.ERR_NO_SUCH_TANK);
                    return (IResult)TypedResults.NotFound(Response.ERR_NO_SUCH_TANK);
                }

                Tank origin = Board.Tanks[id];
                Tank destination = Board.Tanks[target];

                if(origin.Health <= 0)
                {
                    Log.Error($"Tank {id} is dead");
                    return (IResult)TypedResults.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                if (destination.Health <= 0)
                {
                    Log.Error($"Tank {target} is dead");
                    return (IResult)TypedResults.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                // Actually spend points
                if(!origin.SpendActionPoint(amount))
                {
                    Log.Error("Tank did not have enough action points");
                    return (IResult)TypedResults.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                destination.ActionPoints += amount;

                Log.Info($"Tank {id} gave {amount} action points to tank {target}");
                return (IResult)TypedResults.Ok(Response.OK);
            })
            .WithName("GiveActionPoint");
        }
    }
}
