using Postgrest.Attributes;
using Postgrest.Models;

namespace Server.Tables
{
    [Table("UserChats")]
    class UserChats : BaseModel
    {
        [PrimaryKey("Id", false)]
        public int Id { get; set; }

        [Column("LastMessage")]
        public string? LastMessage { get; set; }

        [Column("ChatType")]
        public ChatType ChatType { get; set; }

        [Column("ChatName")]
        public string? ChatName { get; set; }

        [Column("Avatar")]
        public string? Avatar { get; set; }

    }
}
