using Postgrest.Attributes;
using Postgrest.Models;
using Server.Enum;

namespace Server.Tables
{
    [Table("messages")]
    internal class Messages : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("user_chat_id")]
        public int UserChatId { get; set; }

        [Column("text")]
        public string? Text { get; set; }

        [Column("sent_at")]
        public DateTimeOffset SentAt { get; set; }

        [Column("type_id")]
        public TypesOfMessage type_id { get; set; }

    }
}
