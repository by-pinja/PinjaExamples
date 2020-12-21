using System.Threading.Tasks;

namespace Pinja.Examples.ConventionBasedFactories
{
    /// <summary>
    /// A common interface for message handlers.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handles the owned message.
        /// </summary>
        /// <returns>A task that completes when the message has been handled.</returns>
        Task HandleAsync();
    }
}
