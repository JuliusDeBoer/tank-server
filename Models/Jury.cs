namespace TankServer.Models
{
    public class Jury
    {
        public Dictionary<int, int> Candidates { get; private set; } = new();
        private List<int> Voters { get; set; } = new();

        public bool HasVoted(int voterId)
        {
            return Voters.Contains(voterId);
        }

        public void Vote(int voterId, int candidateId)
        {
            Voters.Add(voterId);
            if (!Candidates.ContainsKey(candidateId))
            {
                Candidates.Add(candidateId, 0);
            }
            else
            {
                Candidates[candidateId]++;
            }
        }

        public int? GetWinner()
        {
            if (Candidates.Count <= 0) { return null; }

            int highest = 0;
            foreach (KeyValuePair<int, int> candidate in Candidates)
            {
                if (candidate.Value >= highest)
                {
                    highest = candidate.Value;
                }
            }

            // If there are multiple winners return null
            foreach (KeyValuePair<int, int> candidate in Candidates)
            {
                if (candidate.Value >= highest)
                {
                    return null;
                }
            }

            return highest;
        }

        public void Empty()
        {
            Candidates.Clear();
            Voters.Clear();
        }
    }

    public static class JuryEndPoints
    {
        public static void MapJuryEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/jury");

            group.MapPost("/vote", (HttpContext context) =>
            {
                IHeaderDictionary headers = context.Request.Headers;
                Account? account = Game.Authenticator.GetUser(headers);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                return (IResult)TypedResults.Ok(account);
            })
            .WithName("GetJury");
        }
    }
}
