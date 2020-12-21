namespace Pinja.Examples.Messages
{
    /// <summary>
    /// Chat game event from a player.
    /// </summary>
    public class ChatGameEvent : GameEvent
    {
        /// <summary>
        /// The chat message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ChatGameEvent"/>.
        /// </summary>
        public ChatGameEvent()
        {
            EventType = GameEvents.Chat;
        }
    }
}
