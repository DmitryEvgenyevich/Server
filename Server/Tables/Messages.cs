using Postgrest.Attributes;
using Postgrest.Models;

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

        [Column("StatusMessageId")]
        public int StatusMessageId { get; set; }

        [Column("TypeOfMessageId")]
        public int TypeOfMessageId { get; set; } = 1;

        [Column("FileId")]
        public int? FileId { get; set; }
    }
}
