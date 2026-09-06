using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GameSaveCenter.Worker.Persistence;

internal static class IpcPayloadFingerprint
{
    public static string Compute(string? payloadJson)
    {
        var text = payloadJson ?? string.Empty;
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(text) ? "null" : text);
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
                WriteCanonical(document.RootElement, writer);
            return Convert.ToHexString(SHA256.HashData(buffer.WrittenSpan));
        }
        catch (JsonException)
        {
            // Malformed payloads are still replayable with the exact same bytes; the
            // dispatcher remains responsible for returning the typed payload error.
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.Trim())));
        }
    }

    private static void WriteCanonical(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonical(property.Value, writer);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray()) WriteCanonical(item, writer);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }
}
