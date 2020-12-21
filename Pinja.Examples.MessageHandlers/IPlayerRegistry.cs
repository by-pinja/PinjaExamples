using System.Net;
using System.Threading.Tasks;

namespace Pinja.Examples.MessageHandlers
{
    /// <summary>
    /// Defines an interface for player registry.
    /// </summary>
    public interface IPlayerRegistry
    {
        /// <summary>
        /// Adds a new player.
        /// </summary>
        /// <param name="playerName">The player name.</param>
        /// <param name="peer">The peer the player connected from.</param>
        Task AddPlayerAsync(string playerName, IPAddress peer);

        /// <summary>
        /// Sets the player defeated.
        /// </summary>
        /// <param name="playerName">The player name.</param>
        Task SetPlayerDefeated(string playerName);
    }
}
