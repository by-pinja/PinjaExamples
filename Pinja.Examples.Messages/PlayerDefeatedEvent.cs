namespace Pinja.Examples.Messages
{
    public class PlayerDefeatedEvent : GameEvent
    {
        /// <summary>
        /// The name of the player who defeated the player.
        /// </summary>
        public string DefeatingPlayerName { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="PlayerDefeatedEvent"/>.
        /// </summary>
        public PlayerDefeatedEvent()
        {
            EventType = GameEvents.PlayerDefeated;
        }
    }
}
