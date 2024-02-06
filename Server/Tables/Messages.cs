using Postgrest.Attributes;
using Postgrest.Models;
using Server.Enum;

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
        public string? Message { get; set; }

        [Column("Time")]
        public DateTime Time { get; set; }

        [Column("UserChatId")]
        public int UserChatId { get; set; }

        [Column("StatusOfMessage")]
        public StatusesOfMessage StatusOfMessage { get; set; }

        [Column("TypeOfMessage")]
        public TypesOfMessage TypeOfMessage { get; set; }

        [Column("FileId")]
        public int? FileId { get; set; }
    }
}
