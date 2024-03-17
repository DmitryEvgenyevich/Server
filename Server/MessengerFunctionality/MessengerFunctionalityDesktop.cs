using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Server.Message;
using Server.Tables;
using System.Net.Sockets;
using Server.Enum;

namespace Server.MessengerFunctionality
{
    public class MessengerFunctionalityDesktop: IMessengerFunctionality
    {
        public async Task<Response> SignInByToken(TcpClient clientSocket, string json)
        {
            try
            {
                var deserialized_devise = JsonConvert.DeserializeObject<Devises>(json);
                var result = await Database.Database.GetUserByDeviseToken(deserialized_devise!.token!);

                if (result?.Id == null)
                    return new Response { };

                if (System.IO.File.Exists(@$"avatars\{result.Id}.png"))
                {
                    byte[] imageData = File.ReadAllBytes(@$"avatars\{result.Id}.png");
                    result.Avatar = imageData;
                }

                return new Response { Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = $"Error from server {ex.Message}" };
            }
        }

        public async Task<Response> SignIn(TcpClient clientSocket, string json)
        {
            try
            {
                var deserialized_user = JsonConvert.DeserializeObject<Users>(json);
                var result = await Database.Database.GetUserByEmailAndPassword(deserialized_user!.Email!, deserialized_user?.Password!);

                if (result?.Id == null)
                    return new Response { ErrorMessage = "We can not to find this user, Email or password is not right" };

                return new Response { Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = $"Error from server {ex.Message}" };
            }
        }

        public async Task<Response> SignUp(TcpClient clientSocket, string json)
        {
            try
            {
                var deserialized_user = JsonConvert.DeserializeObject<Users>(json);
                var new_user = await Database.Database.InsertUserToTableUsers(deserialized_user!);
                var serialized_new_user = JsonConvert.SerializeObject(new_user);

                return new Response { Data = serialized_new_user };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = $"Error from server {ex.Message}" };
            }
        }

        public async Task<Response> GetMyContacts(TcpClient clientSocket, string json)
        {
            try
            {
                var userId = JObject.Parse(json).Value<int>("Id");

                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(userId, clientSocket);
                return new Response { Data = await Database.Database.GetChatsByUserId(userId) };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = $"Error from server {ex.Message}" };
            }
        }

        public async Task<IMessage> SendNewCode(TcpClient clientSocket, string json)
        {
            try
            {       
                var userId = JObject.Parse(json).Value<int>("Id");

                await Authentication.Authentication.UpdateOrAddNewUser(userId, GlobalUtilities.GlobalUtilities.CreateRandomNumber(1000000, 9999999));

                return new Response { SendToClient = false };
            }
            catch (Exception ex)
            {
                return new Notification { Data = $"Error from server {ex.Message}", TypeOfNotification = Enum.NotificationTypes.Error };
            }
        }

        public async Task<Response> IsCodeRight(TcpClient clientSocket, string json)
        {
            try
            {
                var deserializedUser = JsonConvert.DeserializeObject<Users>(json)!;
                var authenticationCode = JObject.Parse(json).Value<int>("AuthenticationCode");

                var error = Authentication.Authentication.IsCodeRight_DeleteFromList(deserializedUser.Id, authenticationCode).ErrorMessage;

                if (error != null)
                {
                    return new Response { ErrorMessage = error };
                }

                var token = Guid.NewGuid().ToString();
                _ = Database.Database.AddNewDevise(new Devises{ token = token, user_id = deserializedUser.Id });

                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(deserializedUser.Id, clientSocket);

                return new Response { Data = token };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = $"Error from server {ex.Message}" };
            }
        }

        //public async Task<Response> UpdatePassword(TcpClient clientSocket, string json)
        //{
        //    try
        //    {
        //        var user = JsonConvert.DeserializeObject<Users>(json)!;

        //        await Database.Database.UpdatePassword(user);

        //        return new Response { };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Response { ErrorMessage = GlobalUtilities.GlobalUtilities.GetErrorMessage(ex) };
        //    }
        //}

        public async Task<Response> SendMessage(TcpClient clientSocket, string json)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<Messages>(json)!;

                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(JObject.Parse(json).Value<int>("SenderId"), clientSocket);

                var usersList = await Database.Database.GetContactsIdsByUserChatId(message.UserChatId, JObject.Parse(json).Value<int>("SenderId"));

                JArray jsonArray = JArray.Parse(usersList);
                int[] userIds = new int[jsonArray.Count];

                for (int i = 0; i < jsonArray.Count; i++)
                {
                    userIds[i] = (int)jsonArray[i]["user_id"];
                }

                message.SentAt = DateTimeOffset.Now;
                message.type_id = TypesOfMessage.TEXT;
                await Users.TryToSendToUsers(userIds!, message, JObject.Parse(json).Value<string>("SenderUsername")!, JObject.Parse(json).Value<int>("chat_id"));
                await Database.Database.SetLastMessage(message.UserChatId, (await Database.Database.InsertMessageToTableMessages(message)).Model!.Id);

                return new Response { SendToClient = false };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = GlobalUtilities.GlobalUtilities.GetErrorMessage(ex) };
            }
        }

        //public async Task<IMessage> SendMessageInChat(TcpClient clientSocket, string json)
        //{
        //    try
        //    {
        //        var message = JsonConvert.DeserializeObject<Messages>(json)!;

        //        _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(message.SenderId, clientSocket);
        //        _ = Users.TryToSendToUser(JObject.Parse(json).Value<int>("RecipientId"), message, JObject.Parse(json).Value<string>("SenderUsername")!);

        //        _ = Database.Database.SetLastMessage(message.UserChatId, (await Database.Database.InsertMessageToTableMessages(message)).Model!.Id, message.SenderId);

        //        return new Response { SendToClient = false };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Notification { Data = GlobalUtilities.GlobalUtilities.GetErrorMessage(ex), TypeOfNotification = Enum.NotificationTypes.Error };
        //    }
        //}

        public async Task<Response> GetMessagesByChat(TcpClient clientSocket, string json)
        {
            try
            {
                _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(JObject.Parse(json).Value<int>("UserId"), clientSocket);

                var temp = await Database.Database.GetMessagesByChatId(JObject.Parse(json).Value<int>("user_chat_id"));

                return new Response { Data = temp };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = GlobalUtilities.GlobalUtilities.GetErrorMessage(ex) };
            }
        }

        //public Response logOut(TcpClient clientSocket, string json)
        //{
        //    try
        //    {
        //        _ = OnlineUsers.OnlineUsers.DeleteUserFromOnlineList(clientSocket);
        //        Console.WriteLine("Client disconnected.");

        //        return new Response { SendToClient = false };
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return new Response { ErrorMessage = ex.Message, SendToClient = false };
        //    }
        //}

        //public async Task<Response> CreateNewChat(TcpClient clientSocket, string json)
        //{
        //    try
        //    {
        //        var dataForNewChat = JsonConvert.DeserializeObject<NewChat>(json);

        //        _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(dataForNewChat!.SenderId, clientSocket);

        //        var newChat = await Database.Database.CreateNewChat();

        //        var recipient = await Database.Database.GetUserByUsername(dataForNewChat.RecipientUsername!);

        //        _ = Database.Database.CreateChatConnection(new List<UsersChats>
        //            {
        //                new UsersChats{ChatId = (int)newChat.Model!.Id, UserId = dataForNewChat.SenderId},
        //                new UsersChats{ChatId = (int)newChat.Model.Id, UserId = recipient.Id}
        //            }
        //        );

        //        _ = Users.TryToSendNotification(recipient.Id, (int)newChat.Model.Id, (await Database.Database.GetUserById(dataForNewChat.SenderId)).Username!);

        //        return new Response { Data = JsonConvert.SerializeObject(new ChatModel
        //        {
        //            ChatId = (int)newChat.Model.Id,
        //            Contact = new ContactModel(recipient),
        //            ChatName = recipient.Username,
        //            Type = TypesOfChat.CHAT
        //        }) };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Response { ErrorMessage = GlobalUtilities.GlobalUtilities.GetErrorMessage(ex) };
        //    }
        //}

        //public async Task<Response> FindUserByUsername(TcpClient clientSocket, string json)
        //{
        //    try
        //    {
        //        var user = JsonConvert.DeserializeObject<Users>(json);

        //        _ = OnlineUsers.OnlineUsers.AddUserToList_IfUserIsNotInOnlineList(user!.Id, clientSocket);

        //        var users = await Database.Database.FindUsersByUsername(user.Username!, user.Id);

        //        List<object> findUsers = new List<object>();

        //        foreach (var item in JArray.Parse(users.Content!))
        //        {
        //            findUsers.Add(new { ChatName = item["Username"]!.ToString() });
        //        }

        //        return new Response { Data = JsonConvert.SerializeObject(findUsers) };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Response { ErrorMessage = GlobalUtilities.GlobalUtilities.GetErrorMessage(ex) };
        //    }
        //}
    }
}
