using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;
using Server.Message;
using System.Net.Sockets;

namespace Server.Tables
{
    [Table("users")]
    internal class Users : BaseModel
    {

        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("username")]
        public string? Username { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("last_login")]
        public DateTimeOffset? LastLogin { get; set; }

        public byte[]? Avatar { get; set; }

        static public async Task TryToSendToUsers(List<Users> usersList, Messages message, string Username)
        {
            await Task.Run(() =>
            {
                foreach (var item in usersList)
                {
                    _ = TryToSendToUser(item.Id, message, Username);
                }
            });
        }

        static public async Task TryToSendToUser(int id, Messages message, string Username)
        {
            await Task.Run(() =>
            {
                TcpClient clientSocketRecipient;

                OnlineUsers.OnlineUsers.TryToGetValue(id, out clientSocketRecipient);
                var stream = clientSocketRecipient?.GetStream();

                if (stream != null)
                {
                    var dataForRecipient = new
                    {
                        Time = message.Time,
                        Message = message.Message,
                        ChatId = message.UserChatId,
                        Username = Username
                    };

                    _ = GlobalUtilities.GlobalUtilities.SendRequest(stream!, new Notification { Data = JsonConvert.SerializeObject(dataForRecipient), TypeOfNotification = Enum.NotificationTypes.NewMessage });
                }
            });
        }

        static public async Task TryToSendNotification(int recipientId, int chatId, string recipientUsername)
        {
            await Task.Run(() =>
            {
                TcpClient clientSocketRecipient;
                OnlineUsers.OnlineUsers.TryToGetValue(recipientId, out clientSocketRecipient);

                var stream = clientSocketRecipient?.GetStream();

                if (stream != null)
                {
                    var dataForRecipient = new
                    {
                        ChatName = recipientUsername,
                        ChatId = chatId
                    };
                    _ = GlobalUtilities.GlobalUtilities.SendRequest(stream!, new Notification { Data = JsonConvert.SerializeObject(dataForRecipient), TypeOfNotification = Enum.NotificationTypes.NewChat });
                }
            });
        }
    }

    internal class Wrapper
    {
        public Users? Users { get; set; }
    }

}
