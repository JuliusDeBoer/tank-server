namespace Models
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
}
