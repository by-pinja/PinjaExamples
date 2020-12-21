using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// JSON converter for <see cref="TimeSpan"/> values to be used with <see cref="System.Text.Json.JsonSerializer"/> that uses the constant invariant format (format string "c").
    /// </summary>
    public class ConstantInvariantTimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        private const string FormatString = "c";

        /// <inheritdoc />
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new FormatException($"Time span values should be formatted in constant invariant string representation. Encountered toke '{reader.TokenType}.'");
            }

            var stringValue = reader.GetString();
            if (!TimeSpan.TryParseExact(stringValue, FormatString, CultureInfo.InvariantCulture, TimeSpanStyles.None, out var result))
            {
                throw new FormatException($"The time span value '{stringValue}' is in an invalid format.");
            }

            return result;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(FormatString, CultureInfo.InvariantCulture));
        }
    }
}
