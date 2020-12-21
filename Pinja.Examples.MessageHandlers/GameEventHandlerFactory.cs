using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Pinja.Examples.ConventionBasedFactories;
using Pinja.Examples.Messages;

namespace Pinja.Examples.MessageHandlers
{
    /// <summary>
    /// A factory for creating game events.
    /// </summary>
    public class GameEventHandlerFactory : IGameEventHandlerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IChat _chat;

        private delegate IGameEventHandler HandlerActivator(ILogger logger, IPlayerRegistry playerRegistry, IChat chat, GameEvent gameEvent);

        private static IReadOnlyDictionary<string, (HandlerActivator Activator, Type ImplemetingType)> Activators { get; }

        /// <summary>
        /// Initializes static members of <see cref="GameEventHandlerFactory"/>.
        /// </summary>
        static GameEventHandlerFactory()
        {
            Activators = HandlerMappingHelper.CreateActivators<HandlerActivator, IGameEventHandler, string, GameEventHandlerAttribute>(a => a.EventType);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="GameEventHandlerFactory"/>.
        /// </summary>
        /// <param name="loggerFactory">A factory for creating loggers.</param>
        /// <param name="playerRegistry">The player registry.</param>
        /// <param name="chat">The chat.</param>
        public GameEventHandlerFactory(ILoggerFactory loggerFactory, IPlayerRegistry playerRegistry, IChat chat)
        {
            _loggerFactory = loggerFactory;
            _playerRegistry = playerRegistry;
            _chat = chat;
        }

        /// <inheritdoc />
        public IGameEventHandler GetHandler(GameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            if (!Activators.TryGetValue(gameEvent.EventType, out var activatorItem))
            {
                throw new ArgumentException($"The game event '{gameEvent.EventType}' does not have a handler associated with it.");
            }

            return activatorItem.Activator(_loggerFactory.CreateLogger(activatorItem.ImplemetingType), _playerRegistry, _chat, gameEvent);
        }
    }
}
