using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tables
{
    [Table("user_chat_users")]
    class UserChatUsers : BaseModel
    {

        [PrimaryKey("UserChatId", true)]
        public int UserChatId { get; set; }

        [PrimaryKey("UserId", true)]
        public int UserId { get; set; }

        [Column("LastMessage")]
        public string? LastMessage { get; set; }

        [Column("ChatName")]
        public string? ChatName { get; set; }

    }
}
