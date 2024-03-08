using Postgrest.Attributes;
using Postgrest.Models;

enum TypesOfChat
{
    PRIVATE_CHAT = 1,
    GROUP = 2,
    CHANNEL = 3
}

namespace Server.Tables
{
    [Table("chats")]
    class Chats : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("type_id")]
        public TypesOfChat type { get; set; }
    }
}
