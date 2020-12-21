using Pinja.Examples.Messages;

namespace Pinja.Examples.MessageHandlers
{
    public interface IGameEventHandlerFactory
    {
        IGameEventHandler GetHandler(GameEvent gameEvent);
    }
}
