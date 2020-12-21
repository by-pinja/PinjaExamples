namespace Pinja.Examples.Messages
{
    public class DetailedVoteEntry : VoteEntry
    {
        public string Reason { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} ({Reason})";
        }
    }
}
