using Postgrest.Attributes;
using Postgrest.Models;

namespace Server.Tables
{
    [Table("UserChatUsers")]
    class UserChatUsers : BaseModel
    {

        [PrimaryKey("UserChatId", true)]
        public int UserChatId { get; set; }

        [PrimaryKey("UserId", true)]
        public int UserId { get; set; }

        [Column("LastMessage")]
        public int? LastMessage { get; set; }

    }
}
