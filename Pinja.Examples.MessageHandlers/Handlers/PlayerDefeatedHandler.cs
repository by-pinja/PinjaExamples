using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pinja.Examples.Messages;

namespace Pinja.Examples.MessageHandlers.Handlers
{
    /// <summary>
    /// Message handler for player defeated events.
    /// </summary>
    [GameEventHandler(GameEvents.PlayerDefeated)]
    public class PlayerDefeatedHandler : GameEventHandlerBase<PlayerDefeatedEvent>
    {
        public PlayerDefeatedHandler(ILogger logger, IPlayerRegistry playerRegistry, IChat chat, GameEvent gameEvent)
            : base(logger, playerRegistry, chat, gameEvent)
        {
        }

        /// <inheritdoc />
        public override async Task HandleAsync()
        {
            await PlayerRegistry.SetPlayerDefeated(GameEvent.PlayerName);

            await Chat.AddMessageAsync($"Player '{GameEvent.DefeatingPlayerName}' has defeated player '{GameEvent.PlayerName}'.");

            Logger.LogInformation("Player {playerName} has been set as defeated.", GameEvent.PlayerName);
        }
    }
}
