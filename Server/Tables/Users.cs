using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;
using Server.Message;
using System.Net.Sockets;

namespace Server.Tables
{
    [Table("Users")]
    internal class Users : BaseModel
    {

        [PrimaryKey("Id", false)]
        public int Id { get; set; }

        [Column("Username")]
        public string? Username { get; set; }

        [Column("Email")]
        public string? Email { get; set; }

        [Column("Password")]
        public string? Password { get; set; }

        [Column("LastLogin")]
        public DateTimeOffset? LastLogin { get; set; }

        [Column("Avatar")]
        public string? Avatar { get; set; }

        [Column("AuthenticationStatus")]
        public bool AuthenticationStatus { get; set; }

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
