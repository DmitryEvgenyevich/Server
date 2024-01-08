using Postgrest.Attributes;
using Postgrest.Models;
using Server.Statuses;
using Server.Types;

namespace Server.Tables
{
    [Table("Messages")]
    internal class Messages : BaseModel
    {
        [PrimaryKey("Id", false)]
        public int Id { get; set; }

        [Column("SenderId")]
        public int SenderId { get; set; }

        [Column("Message")]
        public string Message { get; set; }

        [Column("Time")]
        public DateTime Time { get; set; }

        [Column("UserChatId")]
        public int UserChatId { get; set; }

        [Column("StatusOfMessage")]
        public MessageStatus StatusOfMessage { get; set; }

        [Column("TypeOfMessage")]
        public MessageTypes TypeOfMessage { get; set; }

        [Column("FileId")]
        public int? FileId { get; set; }
    }
}
