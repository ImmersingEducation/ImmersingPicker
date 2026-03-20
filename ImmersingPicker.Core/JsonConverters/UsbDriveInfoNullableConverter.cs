using System.Text.Json;
using System.Text.Json.Serialization;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Core.JsonConverters;

public class UsbDriveInfoNullableConverter : JsonConverter<UsbDriveInfo?>
{
    public override UsbDriveInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var usbDriveInfo = JsonSerializer.Deserialize<UsbDriveInfo>(ref reader, options);
        return usbDriveInfo;
    }

    public override void Write(Utf8JsonWriter writer, UsbDriveInfo? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
