namespace Tanks.Models
{
    public class Candidate
    {
        public int Tank { get; set; }
        public int Votes { get; set; }

        public Candidate(int tank)
        {
            Tank = tank;
            Votes = 0;
        }
    }

    public class Jury
    {
        public Dictionary<int, Candidate> Candidates { get; private set; } = new();
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
                Candidates.Add(candidateId, new Candidate(candidateId));
            }
            else
            {
                Candidates[candidateId].Votes++;
            }
        }
    }

    public static class JuryEndPoints
    {
        //public class JuryVoteResultCollection
        //{
        //    public List<Tank> Tanks { get; set; } = new List<Tank>();

        //    public TankTotal(Dictionary<int, Tank> tanks)
        //    {
        //        foreach (KeyValuePair<int, Tank> pair in tanks)
        //        {
        //            Tanks.Add(pair.Value);
        //            Total++;
        //        }
        //    }

        public static void MapJuryEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/jury");

            group.MapGet("/vote", (HttpContext context) =>
            {
                var headers = context.Request.Headers;

                Account? account = Game.Authenticator.GetUser(headers);

                if(account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                return (IResult)TypedResults.Ok(account);
            })
            .WithName("GetJury");
        }
    }
}
