using System;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Specifies the type descriptor policy.
    /// </summary>
    public abstract class TypeDescriptorPolicy
    {
        /// <summary>
        /// The default type descriptor policy.
        /// </summary>
        /// <remarks>Uses full type names, with : separator, and derived first.</remarks>
        public static readonly TypeDescriptorPolicy Default = new ClassNameBasedTypeDescriptorPolicy(SerializationHelpers.DefaultTypeSeparator, TypeHierarchyMode.AbstractFirst, true);

        /// <summary>
        /// The separator to use when serializing the whole type hierarchy to the type descriptor.
        /// </summary>
        public abstract string TypeSeparator { get; }

        /// <summary>
        /// The hierarchy serialization mode.
        /// </summary>
        public abstract TypeHierarchyMode TypeHierarchyMode { get; }

        /// <summary>
        /// If set to <see langword="true"/> the type descriptor is metadata. If set to <see langword="false"/> the type descriptor is represented by a property in the type and <see cref="TypeDescriptorPropertyName"/> should name the property.
        /// </summary>
        public virtual bool IsTypeDescriptorMetaData { get; } = true;

        /// <summary>
        /// The name of the property that acts as the type descriptor.
        /// </summary>
        /// <remarks>Defaults to __type__.</remarks>
        public virtual string TypeDescriptorPropertyName { get; } = "__type__";

        /// <summary>
        /// Determines if the deserialization should fallback to root type if type descriptor cannot be resolved to type.
        /// </summary>
        public virtual bool FallbackToRootType { get; } = false;

        /// <summary>
        /// Gets the type descriptor including the type hierarchy.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The type descriptor.</returns>
        public abstract string GetTypeDescriptor(Type type);

        /// <summary>
        /// Gets the parent type descriptor of this descriptor.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor.</param>
        /// <param name="parentTypeDescriptor">The parent type descriptor.</param>
        /// <returns><see langword="true"/> if parent descriptor could be resolved; otherwise <see langword="false"/>.</returns>
        public abstract bool TryGetParentDescriptor(string typeDescriptor, out string parentTypeDescriptor);
    }
}
