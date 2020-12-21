using Pinja.Examples.Extensibility;

namespace Pinja.Examples.Messages
{
    public class VoteEntry : IExtensible
    {
        public ExtensionData ExtensionData { get; set; }

        public string Voter { get; set; }

        public string Result { get; set; }

        public override string ToString()
        {
            return $"{Voter}: {Result}";
        }
    }
}
