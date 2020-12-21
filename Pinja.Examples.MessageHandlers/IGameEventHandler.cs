using System.Threading.Tasks;

namespace Pinja.Examples.MessageHandlers
{
    /// <summary>
    /// Defines an interface for game event handlers.
    /// </summary>
    public interface IGameEventHandler
    {
        /// <summary>
        /// Handles the game event.
        /// </summary>
        Task HandleAsync();
    }
}
