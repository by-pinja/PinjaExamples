using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Factory for creating <see cref="ExtensibleJsonConverter{TExtensible}"/> converters.
    /// </summary>
    public class ExtensibleJsonConverterFactory : JsonConverterFactory
    {
        private readonly TypeDescriptorPolicy _defaultTypeDescriptor;

        private delegate JsonConverter ConverterConstructorDelegate(ITypeMappings typeMappings);

        private readonly Type _interfaceType = typeof(IExtensible);

        private readonly Type _converterGenericType = typeof(ExtensibleJsonConverter<>);

        private readonly ConcurrentDictionary<Type, ConverterConstructorDelegate> _constructors = new ConcurrentDictionary<Type, ConverterConstructorDelegate>();
        private readonly ConcurrentDictionary<Type, ITypeMappings> _allTypeMappings;

        /// <summary>
        /// Initializes a new instance of <see cref="ExtensibleJsonConverterFactory"/>.
        /// </summary>
        /// <param name="typeMappings">The type mappings.</param>
        /// <param name="defaultTypeDescriptor">The default type descriptor policy.</param>
        public ExtensibleJsonConverterFactory(IEnumerable<ITypeMappings> typeMappings, TypeDescriptorPolicy defaultTypeDescriptor)
        {
            _defaultTypeDescriptor = defaultTypeDescriptor;
            _allTypeMappings = new ConcurrentDictionary<Type, ITypeMappings>();
            foreach (var typeMapping in typeMappings)
            {
                _allTypeMappings[typeMapping.RootType] = typeMapping;
            }
        }

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return _interfaceType.IsAssignableFrom(typeToConvert);
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var ctor = GetConstructor(_converterGenericType.MakeGenericType(typeToConvert));

            var rootType = typeToConvert.GetProperty(nameof(IExtensible.ExtensionData), typeof(ExtensionData))?.DeclaringType;
            if (rootType == null)
            {
                throw new ArgumentException($"Cannot create an extensible JSON converter for type {typeToConvert} because it does not implement the {nameof(IExtensible)} interface.");
            }

            if (!_allTypeMappings.TryGetValue(rootType, out var typeMappings))
            {
                // Type mapping not configured yet for one reason or another.
                // Configure type mappings by scanning assemblies where there are types that are assignable to the root type.
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetTypes().Any(rootType.IsAssignableFrom));

                typeMappings = SerializationHelpers.CreateConstructor<SerializationHelpers.DefaultTypeMappingsConstructorDelegate>(typeof(DefaultTypeMappings<>).MakeGenericType(rootType))(assemblies, _defaultTypeDescriptor);
                _allTypeMappings[rootType] = typeMappings;
            }

            return ctor(typeMappings);
        }

        private ConverterConstructorDelegate GetConstructor(Type converterType)
        {
            if (!_constructors.TryGetValue(converterType, out var ctor))
            {
                ctor = _constructors.GetOrAdd(converterType, SerializationHelpers.CreateConstructor<ConverterConstructorDelegate>(converterType));
            }

            return ctor;
        }
    }
}
