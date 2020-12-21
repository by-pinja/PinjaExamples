using System.Text.Json;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// A writer wrapper that handles writing a specific property to the JSON writer.
    /// </summary>
    public class PropertyWriter
    {
        private readonly string _memberName;
        private readonly PropertyAccessor _accessor;
        private readonly object _instance;
        private readonly Utf8JsonWriter _writer;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of <see cref="PropertyWriter"/>.
        /// </summary>
        /// <param name="memberName">The member name to serialize.</param>
        /// <param name="accessor">The property accessor that provides the property value.</param>
        /// <param name="instance">The instance to get the property value from.</param>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
        public PropertyWriter(string memberName, PropertyAccessor accessor, object instance, Utf8JsonWriter writer, JsonSerializerOptions jsonSerializerOptions)
        {
            _memberName = memberName;
            _accessor = accessor;
            _instance = instance;
            _writer = writer;
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        /// <summary>
        /// Writes the property value to the writer.
        /// </summary>
        public void Write()
        {
            object value = _accessor.GetValue(_instance);
            // Do not write null values when they should be ignored
            if (value == null && _jsonSerializerOptions.IgnoreNullValues)
            {
                return;
            }

            _writer.WritePropertyName(_jsonSerializerOptions.PropertyNamingPolicy?.ConvertName(_memberName) ?? _memberName);
            JsonSerializer.Serialize(_writer, value, _accessor.PropertyType, _jsonSerializerOptions);
        }
    }
}
