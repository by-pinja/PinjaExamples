using System;
using Pinja.Examples.Extensibility;

namespace Pinja.Examples.Events
{
    public class EvenTypeDescriptorPolicy : TypeDescriptorPolicy
    {
        private const string Separator = ".";
        public override string TypeSeparator => Separator;
        public override TypeHierarchyMode TypeHierarchyMode => TypeHierarchyMode.AbstractFirst;

        public override bool IsTypeDescriptorMetaData => false;

        public override string TypeDescriptorPropertyName => nameof(Event.Topic);

        public override bool FallbackToRootType => true;

        /// <inheritdoc />
        public override string GetTypeDescriptor(Type type)
        {
            return TopicMappings.GetTopic(type);
        }

        /// <inheritdoc />
        public override bool TryGetParentDescriptor(string typeDescriptor, out string parentTypeDescriptor)
        {
            var tokenIndex = typeDescriptor.LastIndexOf(TypeSeparator, StringComparison.Ordinal);
            if (tokenIndex == -1)
            {
                parentTypeDescriptor = null;
                return false;
            }

            parentTypeDescriptor = typeDescriptor.Substring(0, tokenIndex);
            return true;
        }
    }
}
