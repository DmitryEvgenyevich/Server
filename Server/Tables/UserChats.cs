using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tables
{
    [Table("user_chats")]
    class UserChats : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }
    }
}
