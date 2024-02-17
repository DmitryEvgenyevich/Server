using Postgrest.Attributes;
using Postgrest.Models;

namespace Server.Tables
{
    [Table("UsersChats")]
    class UsersChats : BaseModel
    {
        [PrimaryKey("ChatId", true)]
        public int ChatId { get; set; }

        [PrimaryKey("UserId", true)]
        public int UserId { get; set; }

        [Column("LastMessage")]
        public int? LastMessage { get; set; }
    }
}
