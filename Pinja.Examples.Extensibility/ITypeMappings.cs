using System;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Defines an interface for type mappings
    /// </summary>
    public interface ITypeMappings
    {
        /// <summary>
        /// The root type of the extensible type hierarchy
        /// </summary>
        Type RootType { get; }

        /// <summary>
        /// The type descriptor policy
        /// </summary>
        public TypeDescriptorPolicy TypeDescriptorPolicy { get;  }

        /// <summary>
        /// Tries to get the closest type associated with the type descriptor.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor.</param>
        /// <param name="type">The type.</param>
        /// <returns><see langword="true"/> if closest type can be resolved; otherwise <see langword="false"/>.</returns>
        bool TryGetClosestType(string typeDescriptor, out Type type);

        /// <summary>
        /// Determines if hierarchy is mapped for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="typeDescriptor">The type descriptor.</param>
        /// <returns><see langword="true"/> if type hierarchy is mapped; otherwise <see langword="false"/>.</returns>
        bool IsHierarchyMapped(Type type, out string typeDescriptor);
    }
}
