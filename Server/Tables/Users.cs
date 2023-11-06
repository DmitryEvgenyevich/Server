using Postgrest.Attributes;
using Postgrest.Models;

namespace Server.Tables
{
    [Table("users")]
    public class Users : BaseModel
    {

        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("last_login")]
        public string Last_login { get; set; }

        [Column("avatar")]
        public string Avatar { get; set; }

        [Column("auth")]
        public bool Auth { get; set; }
        
        [Column("auth_code")]
        public int Auth_code { get; set; }

    }
}
