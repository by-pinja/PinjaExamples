using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pinja.Examples.Messages;

namespace Pinja.Examples.MessageHandlers.Handlers
{
    [GameEventHandler(GameEvents.NewPlayer)]
    public class NewPlayerHandler : GameEventHandlerBase<NewPlayerGameEvent>
    {
        public NewPlayerHandler(ILogger logger, IPlayerRegistry playerRegistry, IChat chat, GameEvent gameEvent)
            : base(logger, playerRegistry, chat, gameEvent)
        {
        }

        public override async Task HandleAsync()
        {
            await PlayerRegistry.AddPlayerAsync(GameEvent.PlayerName, GameEvent.Peer);

            await Chat.AddMessageAsync($"Player '{GameEvent.PlayerName}' has entered the game.");

            Logger.LogInformation("Player {playerName} added from {peer}.", GameEvent.PlayerName, GameEvent.Peer);
        }
    }
}
