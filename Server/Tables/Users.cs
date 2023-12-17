using Postgrest.Attributes;
using Postgrest.Models;

namespace Server.Tables
{
    [Table("Users")]
    public class Users : BaseModel
    {

        [PrimaryKey("Id", false)]
        public int Id { get; set; }

        [Column("Username")]
        public string Username { get; set; }

        [Column("Email")]
        public string Email { get; set; }

        [Column("Password")]
        public string Password { get; set; }

        [Column("LastLogin")]
        public DateTimeOffset? LastLogin { get; set; }

        [Column("Avatar")]
        public string Avatar { get; set; }

        [Column("Auth")]
        public bool Auth { get; set; }
        
        [Column("AuthCode")]
        public int AuthCode { get; set; }

    }
}
