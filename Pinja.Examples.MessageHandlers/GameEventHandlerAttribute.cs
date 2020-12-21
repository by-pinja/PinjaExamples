using System;

namespace Pinja.Examples.MessageHandlers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class GameEventHandlerAttribute : Attribute
    {
        /// <summary>
        /// The event type.
        /// </summary>
        public string EventType { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GameEventHandlerAttribute"/>.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        public GameEventHandlerAttribute(string eventType)
        {
            EventType = eventType;
        }
    }
}
