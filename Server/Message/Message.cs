using System.Text.Json;
using System.Text.Json.Serialization;

namespace Server.Message
{
    public interface IMessage
    {
        string Type { get; set; }
    }

    public class Response : IMessage
    {

        public string Type { get; set; } = "Response";

        public string? ErrorMessage { get; set; }

        public string? Data { get; set; }
    }

    public class Notification : IMessage
    {
        public string Type { get; set; } = "Notification";

        public string? TypeOfNotification { get; set; }

        public string? Data { get; set; }

    }

    public class IMessageConverter : JsonConverter<IMessage>
    {
        public override IMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                var root = document.RootElement;

                if (root.TryGetProperty("Type", out var typeProperty))
                {
                    string messageType = typeProperty.GetString()!;

                    switch (messageType)
                    {
                        case "Response":
                            return JsonSerializer.Deserialize<Response>(root.GetRawText())!;

                        case "Notification":
                            return JsonSerializer.Deserialize<Notification>(root.GetRawText())!;

                        default:
                            throw new NotSupportedException($"Unknown message type: {messageType}");
                    }
                }
                throw new JsonException("Type property not found in JSON.");
            }
        }

        public override void Write(Utf8JsonWriter writer, IMessage value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName("Type");
            writer.WriteStringValue(value.Type);

            if (value is Response response)
            {
                if (response.ErrorMessage != null)
                {
                    writer.WritePropertyName("ErrorMessage");
                    writer.WriteStringValue(response.ErrorMessage);
                }

                if (response.Data != null)
                {
                    writer.WritePropertyName("Data");
                    writer.WriteStringValue(response.Data);
                }
            }

            else if (value is Notification notification)
            {
                if (notification.TypeOfNotification != null)
                {
                    writer.WritePropertyName("TypeOfNotification");
                    writer.WriteStringValue(notification.TypeOfNotification);
                }
                
                if (notification.Data != null)
                {
                    writer.WritePropertyName("Data");
                    writer.WriteStringValue(notification.Data);
                }
            }
            else
            {
                throw new NotSupportedException($"Unknown message type: {value.GetType().Name}");
            }

            writer.WriteEndObject();
        }
    }

}
