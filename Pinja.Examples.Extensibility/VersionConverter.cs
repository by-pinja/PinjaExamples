using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// JSON converter for <see cref="Version"/>.
    /// </summary>
    public class VersionConverter : JsonConverter<Version>
    {
        /// <inheritdoc />
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new FormatException("Version must be serialized as a string.");
            }

            var stringValue = reader.GetString();
            if (!Version.TryParse(stringValue, out var result))
            {
                throw new FormatException($"Cannot convert '{stringValue}' to {nameof(Version)}.");
            }

            return result;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
