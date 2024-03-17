using Postgrest.Attributes;
using Postgrest.Models;

namespace Server.Tables
{
    [Table("user_chats")]
    class UsersChats : BaseModel
    {
        [PrimaryKey("chat_id", true)]
        public int chat_id { get; set; }

        [PrimaryKey("user_id", true)]
        public int user_id { get; set; }

        [Column("first_unread_message_id")]
        public int? first_unread_message_id { get; set; }
    }
}
