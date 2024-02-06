using Postgrest.Attributes;
using Postgrest.Models;

namespace Server.Tables
{
    [Table("Chats")]
    class Chats : BaseModel
    {
        [PrimaryKey("Id", false)]
        public int Id { get; set; }

        [Column("ChatType")]
        public TypesOfChat ChatType { get; set; }

        [Column("ChatName")]
        public string? ChatName { get; set; }

        [Column("Avatar")]
        public string? Avatar { get; set; }

    }
}
