using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// JSON converter for extensible data structures.
    /// </summary>
    /// <typeparam name="TExtensible">The type of the data structure that implements <see cref="IExtensible"/>.</typeparam>
    public class ExtensibleJsonConverter<TExtensible> : JsonConverter<TExtensible>
        where TExtensible : IExtensible
    {
        private readonly ITypeMappings _typeMappings;

        private readonly Type _rootType = typeof(TExtensible);
        private readonly PropertyAccessor _descriptorPropertyAccessor;

        /// <summary>
        /// Initializes a new instance of <see cref="ExtensibleJsonConverter{TExtensible}"/>.
        /// </summary>
        /// <param name="typeMappings">An interface providing mapping between type descriptors and implementing types.</param>
        public ExtensibleJsonConverter(ITypeMappings typeMappings)
        {
            _typeMappings = typeMappings;
            if (!_typeMappings.TypeDescriptorPolicy.IsTypeDescriptorMetaData)
            {
                _descriptorPropertyAccessor = new PropertyAccessor(_rootType.GetProperty(_typeMappings.TypeDescriptorPolicy.TypeDescriptorPropertyName), _rootType);
            }
        }

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return _rootType.IsAssignableFrom(typeToConvert);
        }

        /// <inheritdoc />
        public override TExtensible Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Invalid token ({reader.TokenType}), expected {JsonTokenType.StartObject} because extensible data must be represented as an object.");
            }

            if (!reader.Read())
            {
                throw new JsonException($"Object ({typeof(TExtensible).Name}) data terminated prematurely after {JsonTokenType.StartObject} token.");
            }

            var typeAccessor = CreateAccessorAndInstance(ref reader, typeToConvert, out var instance, out var index);

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException($"Invalid token ({reader.TokenType}), expected {JsonTokenType.PropertyName} when parsing members of type {typeAccessor.Type.FullName}.");
                }

                var propertyName = reader.GetString();
                if (!typeAccessor.TrySetMember(instance, propertyName, ref reader, options))
                {
                    reader.Read();
                    instance.ExtensionData.ExtensionMembers[index] = new ExtensionMember
                    {
                        Index = index,
                        Name = propertyName,
                        Value = GetExtensionMemberValue(ref reader, options)
                    };
                }

                ++index;
            }

            return instance;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, TExtensible value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            var actualType = value.GetType();
            var typeAccessor = TypeAccessorCache.GetTypeAccessor(actualType, GetPropertyFilter());

            int index = WriteTypeDescriptor(writer, value, options, actualType);

            WriteProperties(writer, value, options, typeAccessor, index);

            writer.WriteEndObject();
        }

        private void WriteProperties(Utf8JsonWriter writer, TExtensible value, JsonSerializerOptions options, TypeAccessor typeAccessor, int index)
        {
            foreach (var propertyWriter in typeAccessor.GetPropertyWriters(value, writer, options))
            {
                // Preserve unknown members in their original location when re-serializing.
                if (value.ExtensionData != null)
                {
                    while (value.ExtensionData.ExtensionMembers.TryGetValue(index, out var extensionMember))
                    {
                        WriteExtensionMember(writer, extensionMember, options);
                        ++index;
                    }
                }

                // No unknown member located at this index. Write property with the current property writer.
                propertyWriter.Write();
                ++index;
            }

            if (value.ExtensionData != null)
            {
                // Write the rest of the extension members if any.
                foreach (var (_, extensionMember) in value.ExtensionData.ExtensionMembers.Where(kv => kv.Key >= index).OrderBy(kv => kv.Key))
                {
                    WriteExtensionMember(writer, extensionMember, options);
                }
            }
        }

        private int WriteTypeDescriptor(Utf8JsonWriter writer, TExtensible value, JsonSerializerOptions options, Type actualType)
        {
            int index = 0;
            var typeDescriptorMemberName = GetTypeDescriptorMember(out var propertyAccessor);

            // Type descriptor is represented by a property in the type.
            if (propertyAccessor != null)
            {
                writer.WritePropertyName(ConvertPropertyName(options, propertyAccessor.PropertyName));
                writer.WriteStringValue((string) propertyAccessor.GetValue(value));
                // Start at index 1 since the property assigned as the type descriptor has already been written.
                index = 1;
            }
            else
            {
                // Preserve type descriptor from extension data in case it is more specific that the currently running application is aware of.
                // Forwards compatibility on re-serialization.
                if (value.ExtensionData?.TypeDescriptor != null)
                {
                    writer.WritePropertyName(ConvertPropertyName(options, typeDescriptorMemberName));
                    writer.WriteStringValue(value.ExtensionData.TypeDescriptor);
                }
                else if (_typeMappings != null && _typeMappings.IsHierarchyMapped(actualType, out var typeDescriptor))
                {
                    writer.WritePropertyName(ConvertPropertyName(options, typeDescriptorMemberName));
                    writer.WriteStringValue(typeDescriptor);
                }
            }

            return index;
        }

        private static string ConvertPropertyName(JsonSerializerOptions options, string propertyName)
        {
            return options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
        }

        protected virtual string GetTypeDescriptorMember(out PropertyAccessor propertyAccessor)
        {
            propertyAccessor = _descriptorPropertyAccessor;
            return _typeMappings.TypeDescriptorPolicy.TypeDescriptorPropertyName;
        }

        /// <summary>
        /// Creates type accessor and instantiates a new instance to populate.
        /// </summary>
        /// <param name="reader">The JSON reader that can be used to read members that affect the type selection.</param>
        /// <param name="typeToConvert">The original type to convert passed to the converter.</param>
        /// <param name="instance">The creates object instance.</param>
        /// <param name="startingIndex">The starting index for assigning properties.</param>
        /// <returns>The type accessor for the instantiated type.</returns>
        private TypeAccessor CreateAccessorAndInstance(ref Utf8JsonReader reader, Type typeToConvert, out TExtensible instance, out int startingIndex)
        {
            var typeDescriptorMember = GetTypeDescriptorMember(out var propertyAccessor);

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Invalid token ({reader.TokenType}), expected {JsonTokenType.PropertyName}.");
            }

            var memberName = reader.GetString();
            string typeDescriptor = null;
            if (string.Equals(memberName, typeDescriptorMember, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!reader.Read())
                {
                    throw new JsonException($"{typeToConvert.Name} data terminated prematurely after {memberName} member name");
                }

                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException($"{typeToConvert.Name} {memberName} value must be a string");
                }

                typeDescriptor = reader.GetString();

                if (_typeMappings != null && _typeMappings.TryGetClosestType(typeDescriptor, out var closestType))
                {
                    typeToConvert = closestType;
                }

                reader.Read();
            }

            var typeAccessor = TypeAccessorCache.GetTypeAccessor(typeToConvert, GetPropertyFilter());
            instance = (TExtensible) typeAccessor.CreateInstance();
            instance.ExtensionData = new ExtensionData();
            // Type descriptor is not represented as a property of the class itself, so it must be stored in the extension data.
            if (propertyAccessor == null)
            {
                instance.ExtensionData.TypeDescriptor = typeDescriptor;
            }

            startingIndex = 0;

            if (typeDescriptor != null)
            {
                if (propertyAccessor != null)
                {
                    propertyAccessor.SetValue(instance, typeDescriptor);
                    startingIndex = 1;
                }
                else
                {
                    instance.ExtensionData.TypeDescriptor = typeDescriptor;
                }
            }

            return typeAccessor;
        }

        /// <summary>
        /// Gets a filter that filters out properties to create accessors for.
        /// </summary>
        /// <returns>A filter to filter out properties.</returns>
        protected virtual Func<PropertyInfo, bool> GetPropertyFilter()
        {
            if (_descriptorPropertyAccessor != null)
            {
                return pi => !(pi.Name == nameof(IExtensible.ExtensionData) && pi.PropertyType == typeof(ExtensionData)) &&
                             !(pi.Name == _descriptorPropertyAccessor.PropertyName && pi.PropertyType == _descriptorPropertyAccessor.PropertyType);
            }
            return pi => !(pi.Name == nameof(IExtensible.ExtensionData) && pi.PropertyType == typeof(ExtensionData));
        }

        private object GetExtensionMemberValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            while (reader.TokenType == JsonTokenType.Comment)
            {
                reader.Read();
            }

            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    return CreateExtensionData(ref reader, options);
                case JsonTokenType.StartArray:
                    return CreateList(ref reader, options);
                case JsonTokenType.String:
                    var str = reader.GetString();
                    reader.Read();
                    return str;
                case JsonTokenType.Number:
                    // Assume decimal
                    var dec = reader.GetDecimal();
                    reader.Read();
                    return dec;
                case JsonTokenType.True:
                case JsonTokenType.False:
                    var b = reader.GetBoolean();
                    reader.Read();
                    return b;
                case JsonTokenType.Null:
                    reader.Read();
                    return null;
                default:
                    throw new JsonException($"Invalid token ({reader.TokenType}), expected a value, start object or start array.");
            }
        }

        private List<object> CreateList(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            reader.Read();
            var list = new List<object>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                list.Add(GetExtensionMemberValue(ref reader, options));
            }

            reader.Read();

            return list;
        }

        private ExtensionData CreateExtensionData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Invalid token ({reader.TokenType}), expected {JsonTokenType.StartObject}.");
            }

            reader.Read();

            var extensionData = new ExtensionData();
            int index = 0;

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException($"Invalid token ({reader.TokenType}), expected {JsonTokenType.PropertyName}.");
                }

                var memberName = reader.GetString();
                reader.Read();

                extensionData.ExtensionMembers[index] = new ExtensionMember
                {
                    Index = index,
                    Name = memberName,
                    Value = GetExtensionMemberValue(ref reader, options)
                };
                ++index;
            }

            reader.Read();
            return extensionData;
        }

        private void WriteExtensionMember(Utf8JsonWriter writer, ExtensionMember extensionMember, JsonSerializerOptions options)
        {
            if (extensionMember.Value == null && options.IgnoreNullValues)
            {
                return;
            }

            writer.WritePropertyName(ConvertPropertyName(options, extensionMember.Name));
            if (extensionMember.Value is ExtensionData extensionData)
            {
                WriteExtensionData(writer, extensionData, options);
            }
            else if (extensionMember.Value is List<object> extensionList)
            {
                writer.WriteStartArray();

                foreach (var item in extensionList)
                {
                    if (item is ExtensionData extensionItem)
                    {
                        WriteExtensionData(writer, extensionItem, options);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, item, item.GetType(), options);
                    }
                }

                writer.WriteEndArray();
            }
            else
            {
                if (extensionMember.Value == null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    JsonSerializer.Serialize(writer, extensionMember.Value, extensionMember.Value.GetType(), options);
                }
            }
        }

        private void WriteExtensionData(Utf8JsonWriter writer, ExtensionData extensionData, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var (_, member) in extensionData.ExtensionMembers.OrderBy(kv => kv.Key))
            {
                WriteExtensionMember(writer, member, options);
            }

            writer.WriteEndObject();
        }
    }
}
