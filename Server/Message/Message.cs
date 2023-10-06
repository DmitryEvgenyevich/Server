using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Message
{
    public interface IMessage
    {
        string Type { get; set; }
    }

    public class Response : IMessage
    {

        public string Type { get; set; } = "Response";

        public string? ErrorMessage { get; set; }

        public string? Data { get; set; }
    }

    public class Notification : IMessage
    {
        public string Type { get; set; } = "Notification";

        public string? TypeOfNotification { get; set; }

        public string? Data { get; set; }
    }
}
