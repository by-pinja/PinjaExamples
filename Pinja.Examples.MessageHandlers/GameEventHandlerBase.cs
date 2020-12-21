using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pinja.Examples.Messages;

namespace Pinja.Examples.MessageHandlers
{
    /// <summary>
    /// Base class for all game events.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event message.</typeparam>
    public abstract class GameEventHandlerBase<TEvent> : IGameEventHandler
        where TEvent : GameEvent
    {
        protected readonly ILogger Logger;

        protected readonly IPlayerRegistry PlayerRegistry;

        protected readonly IChat Chat;

        protected TEvent GameEvent;

        /// <summary>
        /// Initializes a new instance of <see cref="GameEventHandlerBase{TEvent}"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="playerRegistry">The player registry.</param>
        /// <param name="chat">The chat.</param>
        /// <param name="gameEvent">The game event to handle.</param>
        /// <exception cref="ArgumentException">Thrown when the handler cannot handle the message.</exception>
        protected GameEventHandlerBase(ILogger logger, IPlayerRegistry playerRegistry, IChat chat, GameEvent gameEvent)
        {
            Logger = logger;
            PlayerRegistry = playerRegistry;
            Chat = chat;

            if (!(gameEvent is TEvent actualEvent))
            {
                throw new ArgumentException($"The handler {GetType()} cannot handle message of type {gameEvent?.GetType().FullName ?? "<null>"}");
            }

            GameEvent = actualEvent;
        }

        /// <inheritdoc />
        public abstract Task HandleAsync();
    }
}
