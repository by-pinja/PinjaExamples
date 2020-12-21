using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Default type mappings.
    /// </summary>
    /// <typeparam name="TRootType">The root type of the extensible data structure.</typeparam>
    public class DefaultTypeMappings<TRootType> : ITypeMappings where TRootType : class
    {
        private readonly IReadOnlyDictionary<string, Type> _descriptorToType;
        private readonly IReadOnlyDictionary<Type, string> _typeToDescriptor;

        /// <summary>
        /// The root type of the type hierarchy this type mapping governs.
        /// </summary>
        public Type RootType => typeof(TRootType);

        /// <inheritdoc />
        public TypeDescriptorPolicy TypeDescriptorPolicy { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultTypeMappings{TRootType}"/>.
        /// </summary>
        /// <param name="assemblies">The assemblies that contain types that are assignable to the root type.</param>
        /// <param name="typeDescriptorPolicy">The type descriptor policy to use.</param>
        public DefaultTypeMappings(IEnumerable<Assembly> assemblies, TypeDescriptorPolicy typeDescriptorPolicy)
        {
            TypeDescriptorPolicy = typeDescriptorPolicy;
            var descriptorToType = new Dictionary<string, Type>();
            var typeToDescriptor = new Dictionary<Type, string>();
            var root = typeof(TRootType);

            var types = assemblies.SelectMany(a => a.GetTypes().Where(root.IsAssignableFrom));

            foreach (var type in types)
            {
                var descriptor = typeDescriptorPolicy.GetTypeDescriptor(type);
                if (descriptor == null)
                {
                    if (type == RootType && typeDescriptorPolicy.FallbackToRootType)
                    {
                        continue;
                    }

                    throw new ArgumentException($"Type descriptor policy could not map type: {type}");
                }
                descriptorToType[descriptor] = type;
                typeToDescriptor[type] = descriptor;
            }

            _descriptorToType = descriptorToType;
            _typeToDescriptor = typeToDescriptor;
        }

        /// <inheritdoc />
        public bool IsHierarchyMapped(Type type, out string typeDescriptor)
        {
            if (_typeToDescriptor.TryGetValue(type, out typeDescriptor))
            {
                return true;
            }

            typeDescriptor = default;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetClosestType(string typeDescriptor, out Type type)
        {
            var currentDescriptor = typeDescriptor;
            while (!_descriptorToType.TryGetValue(currentDescriptor, out type))
            {
                if (!TypeDescriptorPolicy.TryGetParentDescriptor(currentDescriptor, out var parentTypeDescriptor))
                {
                    return false;
                }

                currentDescriptor = parentTypeDescriptor;
            }

            if (type != null)
            {
                return true;
            }

            if (!TypeDescriptorPolicy.FallbackToRootType)
            {
                return false;
            }

            type = RootType;
            return true;
        }
    }
}
