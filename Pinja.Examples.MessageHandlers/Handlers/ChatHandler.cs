using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pinja.Examples.Messages;

namespace Pinja.Examples.MessageHandlers.Handlers
{
    [GameEventHandler(GameEvents.Chat)]
    public class ChatHandler : GameEventHandlerBase<ChatGameEvent>
    {
        public ChatHandler(ILogger logger, IPlayerRegistry playerRegistry, IChat chat, GameEvent gameEvent) : base(logger, playerRegistry, chat, gameEvent)
        {
        }

        public override async Task HandleAsync()
        {
            await Chat.AddMessageAsync(GameEvent.Message, GameEvent.PlayerName);

            Logger.LogTrace("Player {playerName} added sent message to chat: {message}.", GameEvent.PlayerName, GameEvent.Message);
        }
    }
}
