using Postgrest.Attributes;
using Postgrest.Models;

namespace Server.Tables
{
    [Table("devises")]
    class Devises : BaseModel
    {
        [PrimaryKey("id", false)]
        public int id { get; set; }

        [Column("user_id")]
        public int user_id { get; set; }

        [Column("token")]
        public string? token { get; set; }
    }
}
