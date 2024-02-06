using System.Text.Json;
using System.Text.Json.Serialization;
using Server.Enum;

namespace Server.Message
{
    public interface IMessage
    {
        TypesIMessage Type { get; set; }
        string? Data { get; set; }
    }

    public class Response : IMessage
    {
        public TypesIMessage Type { get; set; } = TypesIMessage.Response;
        public string? ErrorMessage { get; set; }
        public bool SendToClient { get; set; } = true;
        public string? Data { get; set; }
    }

    public class Notification : IMessage
    {
        public TypesIMessage Type { get; set; } = TypesIMessage.Notification;
        public NotificationTypes TypeOfNotification { get; set; }
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
                    System.Enum.TryParse<TypesIMessage>(typeProperty.ToString(), out var messageType);

                    switch (messageType)
                    {
                        case TypesIMessage.Response:
                            return JsonSerializer.Deserialize<Response>(root.GetRawText())!;

                        case TypesIMessage.Notification:
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

            writer.WriteStartObject(value.GetType().Name);

            writer.WritePropertyName("Type");
            writer.WriteStringValue(value.Type.ToString());

            if (value is Response response)
            {
                writer.WritePropertyName("ErrorMessage");
                writer.WriteStringValue(response.ErrorMessage);

                writer.WritePropertyName("Data");
                writer.WriteStringValue(response.Data);
            }
            else if (value is Notification notification)
            {
                writer.WritePropertyName("TypeOfNotification");
                writer.WriteStringValue(notification.TypeOfNotification.ToString());

                writer.WritePropertyName("Data");
                writer.WriteStringValue(notification.Data);
            }
            else
            {
                throw new NotSupportedException($"Unknown message type: {value.GetType().Name}");
            }

            writer.WriteEndObject();
        }
    }

}
