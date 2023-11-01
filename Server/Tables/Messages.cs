using Postgrest.Attributes;
using Postgrest.Models;

namespace Server.Tables
{
    [Table("Messages")]
    internal class Messages : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("sender_id")]
        public int SenderId { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("user_chat_id")]
        public int UserChatId { get; set; }

        [Column("status_message_id")]
        public int StatusMessageId { get; set; }

        [Column("type_of_message_id")]
        public int TypeOfMessageId { get; set; } = 1;

        [Column("file_id")]
        public int? FileId { get; set; }
    }
}
