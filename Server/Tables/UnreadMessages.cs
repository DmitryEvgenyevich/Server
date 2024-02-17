using Postgrest.Attributes;
using Postgrest.Models;

namespace Server.Tables
{
    [Table("UnreadMessages")]
    internal class UnreadMessages : BaseModel
    {
        [PrimaryKey("MessageId", true)]
        public int MessageId { get; set; }
        
        [PrimaryKey("UnreadUserId", true)]
        public int UserId { get; set; }
    }
}
