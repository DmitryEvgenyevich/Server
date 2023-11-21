using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tables
{
    [Table("UserChatUsers")]
    class UserChatUsers : BaseModel
    {

        [PrimaryKey("UserChatId", true)]
        public int UserChatId { get; set; }

        [PrimaryKey("UserId", true)]
        public int UserId { get; set; }

    }
}
