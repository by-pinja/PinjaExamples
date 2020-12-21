using System;

namespace Pinja.Examples.ConventionBasedFactories
{
    /// <summary>
    /// An attribute to apply to the message handler implementing types that defines the discriminator value of the message that the type can handle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MessageHandlerAttribute : Attribute
    {
        /// <summary>
        /// The message discriminator value.
        /// </summary>
        public string Discriminator { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="MessageHandlerAttribute"/>.
        /// </summary>
        /// <param name="discriminator">The message discriminator value.</param>
        public MessageHandlerAttribute(string discriminator)
        {
            Discriminator = discriminator;
        }
    }
}
