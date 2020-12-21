namespace Pinja.Examples.Messages
{
    /// <summary>
    /// Contains discriminators for game events.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>
        /// Discriminator for new player events.
        /// </summary>
        public const string NewPlayer = nameof(NewPlayer);

        /// <summary>
        /// Discriminator for chat events.
        /// </summary>
        public const string Chat = nameof(Chat);

        /// <summary>
        /// Discriminator for player defeated events.
        /// </summary>
        public const string PlayerDefeated = nameof(PlayerDefeated);
    }
}
