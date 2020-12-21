namespace Pinja.Examples.ConventionBasedFactories
{
    /// <summary>
    /// A common interface for all messages that exposes the message discriminator.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The message discriminator.
        /// </summary>
        string Discriminator { get; }
    }
}
