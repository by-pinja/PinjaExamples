using Pinja.Examples.Extensibility;

namespace Pinja.Examples.Messages
{
    public class Message : IExtensible
    {
        public ExtensionData ExtensionData { get; set; }

        public string Discriminator { get; set; }
    }
}
