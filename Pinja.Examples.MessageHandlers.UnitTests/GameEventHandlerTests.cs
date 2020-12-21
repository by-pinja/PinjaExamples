using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Pinja.Examples.Messages;

namespace Pinja.Examples.MessageHandlers.UnitTests
{
    [TestFixture]
    public class GameEventHandlerTests
    {
        private ILoggerFactory _loggerFactory;
        private Mock<IChat> _mockChat;
        private Mock<IPlayerRegistry> _mockPlayerRegistry;
        private GameEventHandlerFactory _factory;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        }

        [SetUp]
        public void Setup()
        {
            _mockChat = new Mock<IChat>();
            _mockPlayerRegistry = new Mock<IPlayerRegistry>();
            _factory = new GameEventHandlerFactory(_loggerFactory, _mockPlayerRegistry.Object, _mockChat.Object);
        }

        [Test]
        public async Task HandleAsync_NewPlayerEvent_AddsNewPlayer_SendsChatMessage()
        {
            // Arrange
            var ipAddress = IPAddress.Parse("192.168.1.112");
            const string playerName = "TheNewPlayer";
            var message = new NewPlayerGameEvent
            {
                Peer = ipAddress,
                PlayerName = playerName
            };

            var handler = _factory.GetHandler(message);

            // Act
            await handler.HandleAsync();

            // Assert
            _mockPlayerRegistry.Verify(m => m.AddPlayerAsync(playerName, ipAddress), Times.Once);
            _mockChat.Verify(m => m.AddMessageAsync($"Player '{playerName}' has entered the game.", null), Times.Once);
        }

        [Test]
        public async Task HandleAsync_ChatMessage_SendsChatMessage()
        {
            // Arrange
            const string playerName = "SomePlayerName";
            const string chatMessage = "I am a very sophisticated message!";
            var message = new ChatGameEvent
            {
                PlayerName = playerName,
                Message = chatMessage
            };

            var handler = _factory.GetHandler(message);

            // Act
            await handler.HandleAsync();

            // Assert
            _mockChat.Verify(m => m.AddMessageAsync(chatMessage, playerName), Times.Once);
        }

        [Test]
        public async Task HandleAsync_PlayerDefeated_SetsPlayerDefeated_SendsChatMessage()
        {
            // Arrange
            const string playerName = "DefeatedPlayer";
            const string defeatingPlayerName = "Pwner";
            var message = new PlayerDefeatedEvent
            {
                PlayerName = playerName,
                DefeatingPlayerName = defeatingPlayerName
            };

            var handler = _factory.GetHandler(message);

            // Act
            await handler.HandleAsync();

            // Assert
            _mockPlayerRegistry.Verify(m => m.SetPlayerDefeated(playerName), Times.Once);
            _mockChat.Verify(m => m.AddMessageAsync($"Player '{defeatingPlayerName}' has defeated player '{playerName}'.", null), Times.Once);
        }
    }
}

