using Pinja.Examples.Extensibility;

namespace Pinja.Examples.Messages
{
    /// <summary>
    /// Base class for game events.
    /// </summary>
    public class GameEvent : IExtensible
    {
        public string EventType { get; set; }

        /// <summary>
        /// The name of the player who sent caused the event.
        /// </summary>
        public string PlayerName { get; set; }

        /// <inheritdoc />
        public ExtensionData ExtensionData { get; set; }
    }
}
