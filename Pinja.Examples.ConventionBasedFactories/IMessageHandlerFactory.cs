using System;

namespace Pinja.Examples.ConventionBasedFactories
{
    /// <summary>
    /// An interface for message handler factories.
    /// </summary>
    public interface IMessageHandlerFactory
    {
        /// <summary>
        /// Creates a handler for the specified message.
        /// </summary>
        /// <param name="message">The message that needs to be handled.</param>
        /// <returns>A new instance of <see cref="IMessageHandler"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when a handler is not defined for the <see cref="IMessage.Discriminator"/> value.</exception>
        IMessageHandler GetHandler(IMessage message);
    }
}
