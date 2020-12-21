using System.Net;

namespace Pinja.Examples.Messages
{
    /// <summary>
    /// Game event about a new player.
    /// </summary>
    public class NewPlayerGameEvent : GameEvent
    {
        /// <summary>
        /// The peer the player connected from.
        /// </summary>
        public IPAddress Peer { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NewPlayerGameEvent"/>.
        /// </summary>
        public NewPlayerGameEvent()
        {
            EventType = GameEvents.NewPlayer;
        }
    }
}
