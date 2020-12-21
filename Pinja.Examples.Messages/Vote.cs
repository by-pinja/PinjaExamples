using System;
using System.Collections.Generic;

namespace Pinja.Examples.Messages
{
    public class Vote : Message
    {
        public int VoteId { get; set; }

        public string Subject { get; set; }

        public int RequiredVotes { get; set; }

        public List<VoteEntry> VoteEntries { get; set; }

        public Vote()
        {
            Discriminator = "Vote";
            VoteEntries = new List<VoteEntry>();
        }

        public bool IsCompleted()
        {
            return VoteEntries.Count >= RequiredVotes;
        }
    }
}
