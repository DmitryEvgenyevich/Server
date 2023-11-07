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

        [PrimaryKey("user_chat_id", true)]
        public int user_chat_id { get; set; }

        [PrimaryKey("user_id", true)]
        public int user_id { get; set; }

    }
}
