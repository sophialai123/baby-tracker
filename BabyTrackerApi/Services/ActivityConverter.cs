using System.Text.Json;
using System.Text.Json.Serialization;
using BabyTrackerApi.Models;

namespace BabyTrackerApi.Services;

public class ActivityConverter : JsonConverter<Activity>
{
    public override Activity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("type", out var typeProperty))
                throw new JsonException("Activity must have a 'type' property");

            var type = typeProperty.GetString()?.ToLower();

            Activity? activity = null;
            try
            {
                activity = type switch
                {
                    "sleep" => (Activity?)JsonSerializer.Deserialize<SleepSession>(root.GetRawText(), CreateOptionsWithoutConverter(options)),
                    "growth" => (Activity?)JsonSerializer.Deserialize<Growth>(root.GetRawText(), CreateOptionsWithoutConverter(options)),
                    "nappy" => (Activity?)JsonSerializer.Deserialize<Nappy>(root.GetRawText(), CreateOptionsWithoutConverter(options)),
                    "feeding" => (Activity?)JsonSerializer.Deserialize<Feeding>(root.GetRawText(), CreateOptionsWithoutConverter(options)),
                    _ => null
                };
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deserializing {type} activity: {ex.Message}");
                throw;
            }

            return activity ?? throw new JsonException($"Unknown activity type: {type}");
        }
    }

    public override void Write(Utf8JsonWriter writer, Activity value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }

    private JsonSerializerOptions CreateOptionsWithoutConverter(JsonSerializerOptions options)
    {
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Clear();
        return newOptions;
    }
}
