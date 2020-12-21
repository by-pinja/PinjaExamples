using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static System.FormattableString;

namespace Pinja.Examples.ConventionBasedFactories
{
    /// <summary>
    /// An abstract base class for all message handlers that define the constructor signature convention to help creating new handler types.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessageHandlerBase<TMessage> : IMessageHandler
    {
        protected readonly ILogger Logger;
        protected readonly IExampleDependency Dependency;
        protected readonly TMessage Message;

        /// <summary>
        /// Initializes a new instance of <see cref="MessageHandlerBase{TMessage}"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message to handle.</param>
        /// <param name="dependency">The dependency service.</param>
        /// <exception cref="ArgumentException">Thrown when the type of the <paramref name="message"/> is not such that the handler can process it.</exception>
        protected MessageHandlerBase(ILogger logger, IMessage message, IExampleDependency dependency)
        {
            if (!(message is TMessage convertedMessage))
            {
                throw new ArgumentException(Invariant($"{GetType().FullName} cannot handle message of type {message?.GetType().FullName ?? "<null>"}."));
            }

            Logger = logger;
            Dependency = dependency;
            Message = convertedMessage;
        }

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <returns>A task that completes when the message has been handled.</returns>
        public abstract Task HandleAsync();
    }
}
