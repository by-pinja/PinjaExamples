using System.Threading.Tasks;

namespace Pinja.Examples.MessageHandlers
{
    /// <summary>
    /// Defines an interface for chat.
    /// </summary>
    public interface IChat
    {
        /// <summary>
        /// Adds adds a message to the chat.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="from">The player name who sent the message. If set to <see langword="null" /> the message is from the game.</param>
        Task AddMessageAsync(string message, string from=null);
    }
}
