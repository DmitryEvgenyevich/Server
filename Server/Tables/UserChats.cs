using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public ChatType? ChatType { get; set; }
    }
}
