using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Class name based type descriptor policy.
    /// </summary>
    public class ClassNameBasedTypeDescriptorPolicy : TypeDescriptorPolicy
    {
        private static readonly Type ObjectType = typeof(object);
        private readonly bool _useFullNames;

        /// <inheritdoc />
        public override string TypeSeparator { get; }

        /// <inheritdoc />
        public override TypeHierarchyMode TypeHierarchyMode { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ClassNameBasedTypeDescriptorPolicy"/>.
        /// </summary>
        /// <param name="typeSeparator">The separator to use when serializing the whole type hierarchy to the type descriptor.</param>
        /// <param name="typeHierarchyMode">The hierarchy serialization mode.</param>
        /// <param name="useFullNames">Determines if full type names with namespaces should be used.</param>
        public ClassNameBasedTypeDescriptorPolicy(string typeSeparator, TypeHierarchyMode typeHierarchyMode, bool useFullNames)
        {
            _useFullNames = useFullNames;
            TypeSeparator = typeSeparator;
            TypeHierarchyMode = typeHierarchyMode;
        }

        protected string GetTypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return _useFullNames ? type.FullName : type.Name;
            }

            var genericTypeParameters = type.GetGenericArguments();
            return _useFullNames
                ? $"{type.Namespace}.{type.Name}<{string.Join(",", genericTypeParameters.Select(GetTypeName))}>"
                : $"{type.Name}<{string.Join(",", genericTypeParameters.Select(GetTypeName))}>";
        }

        /// <inheritdoc />
        public override string GetTypeDescriptor(Type type)
        {
            return TypeHierarchyMode == TypeHierarchyMode.DerivedFirst
                ? GetTypeDescriptorDerivedFirst(type)
                : GetTypeDescriptorRootFirst(type);
        }

        /// <inheritdoc />
        public override bool TryGetParentDescriptor(string typeDescriptor, out string parentTypeDescriptor)
        {
            if (TypeHierarchyMode == TypeHierarchyMode.DerivedFirst)
            {
                var tokenIndex = typeDescriptor.IndexOf(TypeSeparator, StringComparison.Ordinal);
                if (tokenIndex == -1)
                {
                    parentTypeDescriptor = null;
                    return false;
                }

                int startIndex = tokenIndex +1;
                parentTypeDescriptor = typeDescriptor.Substring(startIndex, typeDescriptor.Length - startIndex);
                return true;
            }
            else
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

        protected string GetTypeDescriptorDerivedFirst(Type type)
        {
            var builder = new StringBuilder();
            builder.Append(GetTypeName(type));
            var currentType = type.BaseType;
            while (currentType != null && currentType != ObjectType)
            {
                builder.Append(TypeSeparator);
                builder.Append(GetTypeName(currentType));

                currentType = currentType.BaseType;
            }

            return builder.ToString();
        }

        protected string GetTypeDescriptorRootFirst(Type type)
        {
            var tokens = new List<string>
            {
                GetTypeName(type)
            };
            var currentType = type.BaseType;
            while (currentType != null && currentType != ObjectType)
            {
                tokens.Insert(0, GetTypeName(currentType));
                currentType = currentType.BaseType;
            }

            return string.Join(TypeSeparator, tokens);
        }
    }
}
