using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using static System.FormattableString;

namespace Pinja.Examples.ConventionBasedFactories
{
    /// <summary>
    /// Factory for creating <see cref="IMessageHandler"/> instances based on a message discriminator.
    /// </summary>
    public class MessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IExampleDependency _dependency;

        private delegate IMessageHandler Activator(ILogger logger, IMessage message, IExampleDependency dependency);

        private static readonly IReadOnlyDictionary<string, (Activator Activator, Type ImplemetingType)> Activators;

        static MessageHandlerFactory()
        {
            // Create the instance activators once.
            // In this example they are build in the static type initializer but it can be done in other ways as well.
            // The point is that the hit from the reflection is taken only once.
            Activators = HandlerMappingHelper.CreateActivators<Activator, IMessageHandler, string, MessageHandlerAttribute>(a => a.Discriminator);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MessageHandlerFactory"/>.
        /// </summary>
        /// <param name="loggerFactory">A factory for creating loggers.</param>
        /// <param name="dependency">The example dependency service.</param>
        public MessageHandlerFactory(ILoggerFactory loggerFactory, IExampleDependency dependency)
        {
            _loggerFactory = loggerFactory;
            _dependency = dependency;
        }

        /// <summary>
        /// Creates a handler for the specified message.
        /// </summary>
        /// <param name="message">The message that needs to be handled.</param>
        /// <returns>A new instance of <see cref="IMessageHandler"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when a handler is not defined for the <see cref="IMessage.Discriminator"/> value.</exception>
        public IMessageHandler GetHandler(IMessage message)
        {
            if (!Activators.TryGetValue(message.Discriminator, out var activatorItem))
            {
                throw new ArgumentException($"No handler defined for discriminator: {message.Discriminator}.", nameof(message));
            }

            // Create a new message handler instance using the message and the common dependency.
            return activatorItem.Activator(_loggerFactory.CreateLogger(activatorItem.ImplemetingType), message, _dependency);
        }
    }
}
