using System.Text.Json.Serialization;
using System.Text.Json;

namespace ScanEventsWorker.Helpers
{
    public class SafeEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
            {
                return result;
            }

            // Default to Unknown if it exists
            if (Enum.TryParse<T>("Unknown", out var unknown))
            {
                return unknown;
            }

            throw new JsonException($"Unable to convert \"{value}\" to enum {typeof(T)}.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
