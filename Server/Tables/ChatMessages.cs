using Server.Enum;

namespace Server.Tables
{
    public class ChatMessages
    {   
        public string? Username { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset? Time { get; set; }
        public StatusesOfMessage StatusOfMessage { get; set; }
    }
}
