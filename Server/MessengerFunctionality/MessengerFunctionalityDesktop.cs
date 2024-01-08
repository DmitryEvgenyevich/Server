using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Server.Message;
using Server.Tables;
using System.Net.Sockets;

namespace Server.MessengerFunctionality
{
    public class MessengerFunctionalityDesktop : IMessengerFunctionality
    {
        public async Task<Response> SignIn(TcpClient clientSocket, string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Users>(json);

                var result = await Database.Database.GetUserByEmailAndPassword(user?.Email!, user?.Password!);

                if (result?.Id == null)
                    return new Response { ErrorMessage = "We can not to find your user, Email or password is not right" };

                if (result.Auth)
                    _ = Task.Run(() => OnlineUsers.OnlineUsers.AddUserToList_IfUserNotInOnlineList(result.Id, clientSocket));

                return new Response { Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = ex.Message };
            }
        }

        public async Task<Response> SignUp(TcpClient clientSocket, string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Users>(json);
                _ = Task.Run(() => OnlineUsers.OnlineUsers.AddUserToList_IfUserNotInOnlineList(user.Id, clientSocket));
                HttpResponseMessage httpResponse = await Database.Database.InsertUserToTableUsers(user!);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        public async Task<Response> GetMyContacts(TcpClient clientSocket, string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Tables.Users>(json);

                _ = Task.Run(() => OnlineUsers.OnlineUsers.AddUserToList_IfUserNotInOnlineList(user.Id, clientSocket));

                var chatsId = await Database.Database.GetChatsByUserId(user!.Id);

                List<IChatGroupModels> chats = new List<IChatGroupModels>();

                foreach (UserChatUsers chat in chatsId)
                {
                    var jsonArray = JArray.Parse(await Database.Database.GetContactEmailByChatId(chat.UserChatId, user.Id));

                    foreach (var contact in jsonArray)
                    {
                        if ((ChatType)Enum.ToObject(typeof(ChatType), (int)(contact["UserChats"]["ChatType"])) == ChatType.Chat)
                        {
                            DateTimeOffset result;
                            var temp = DateTimeOffset.TryParse(contact["Users"]?["LastLogin"]?.ToString(), out result);
                            chats.Add(new ChatModel { ChatId = chat.UserChatId, Contact = new ContactModel { Avatar = contact["Users"]["Avatar"].ToString(), Email = contact["Users"]["Email"].ToString(), Id = ((int)contact["Users"]["Id"]), Username = contact["Users"]!["Username"]!.ToString(), LastLogin = temp ? result : null }, ChatName = contact["Users"]!["Username"]!.ToString(), LastMessage = contact["UserChats"]["LastMessage"].ToString(), Type = (ChatType)Enum.ToObject(typeof(ChatType), ((int)contact["UserChats"]["ChatType"])) });
                        }
                        else if (((int)contact["UserChats"]["ChatType"]) == ((int)ChatType.Group))
                        {
                            bool test = true;
                            foreach (var chatFromChats in chats)
                            {
                                if (chatFromChats.ChatId == chat.UserChatId && chatFromChats.GetType() == typeof(GroupModel))
                                {
                                    DateTimeOffset result;
                                    var temp = DateTimeOffset.TryParse(contact["Users"]?["LastLogin"]?.ToString(), out result);
                                    ((GroupModel)chatFromChats).ContactsInGroup.Add(new ContactModel { Avatar = contact["Users"]["Avatar"].ToString(), Email = contact["Users"]["Email"].ToString(), Id = ((int)contact["Users"]["Id"]), LastLogin = temp ? result : null, Username = contact["Users"]!["Username"]!.ToString() });
                                    test = false;
                                    break;
                                }
                            }
                            if (test)
                            {
                                DateTimeOffset result;
                                var temp = DateTimeOffset.TryParse(contact["Users"]?["LastLogin"]?.ToString(), out result);
                                chats.Add(new GroupModel { ChatId = chat.UserChatId, Avatar = contact["UserChats"]["Avatar"].ToString(), ChatName = contact["UserChats"]["ChatName"].ToString(), ContactsInGroup = new List<ContactModel> { new ContactModel { Avatar = contact["Users"]["Avatar"].ToString(), Email = contact["Users"]["Email"].ToString(), Id = ((int)contact["Users"]["Id"]), LastLogin = temp ? result : null, Username = contact["Users"]!["Username"]!.ToString() } }, LastMessage = contact["UserChats"]["LastMessage"].ToString(), Type = (ChatType)Enum.ToObject(typeof(ChatType), ((int)contact["UserChats"]["ChatType"])) });
                            }
                        }
                    }
                }

                return new Response { Data = JsonConvert.SerializeObject(chats) };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }//TODO

        public async Task<Response> SendNewCode(TcpClient clientSocket, string json)
        {
            try
            {
                Tables.Users myObject = JsonConvert.DeserializeObject<Tables.Users>(json)!;

                _ = Task.Run(() => OnlineUsers.OnlineUsers.AddUserToList_IfUserNotInOnlineList(myObject.Id, clientSocket));

                _ = Database.Database.UpdateAuthCodeByEmail(myObject.Email, myObject.AuthCode);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        public async Task<Response> AuthSucces(TcpClient clientSocket, string json)
        {
            try
            {
                Tables.Users myObject = JsonConvert.DeserializeObject<Tables.Users>(json)!;

                await Database.Database.UpdateAuthStatus(myObject.Email, myObject.Auth);

                var user = await Database.Database.GetUserByEmail(myObject.Email);

                _ = Task.Run(() => OnlineUsers.OnlineUsers.AddUserToList_IfUserNotInOnlineList(myObject.Id, clientSocket));

                return new Response { Data = JsonConvert.SerializeObject(user) };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        public async Task<Response> ForgotPassword(TcpClient clientSocket, string json)
        {
            try
            {
                var myObject = JsonConvert.DeserializeObject<Tables.Users>(json)!;

                _ = Task.Run(() => OnlineUsers.OnlineUsers.AddUserToList_IfUserNotInOnlineList(myObject.Id, clientSocket));

                await Database.Database.UpdatePassword(myObject);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        public async Task<Response> SendMessageInGroup(TcpClient clientSocket, string json)
        {
            try
            {
                Tables.Messages message = JsonConvert.DeserializeObject<Tables.Messages>(json)!;
                _ = Task.Run(() => OnlineUsers.OnlineUsers.AddUserToList_IfUserNotInOnlineList(message.SenderId, clientSocket));
                var tests = await Database.Database.GetContactsIdsByChatId(message.UserChatId, message.SenderId);
                JArray jsonArray = JArray.Parse(tests);


                foreach (JObject item in jsonArray)
                {
                    int userId = item["Users"].Value<int>("Id");
                    TcpClient clientSocketRecipient;

                    OnlineUsers.OnlineUsers.TryToGetValue(userId, out clientSocketRecipient);
                    var stream = clientSocketRecipient?.GetStream();
                    if (stream != null)
                    {
                        var dataForRecipient = new
                        {
                            Command = "NewMessage",
                            Time = message.Time,
                            Message = message.Message,
                            ChatId = message.UserChatId,
                            Username = (await Database.Database.GetUserById(message.SenderId)).Username
                        };

                        _ = Server.Server._sendRequest(stream!, new Notification { Data = JsonConvert.SerializeObject(dataForRecipient) });
                    }


                    _ = Database.Database.InsertMessageToTableMessages(message);
                }

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        public async Task<Response> SendMessageInChat(TcpClient clientSocket, string json)
        {
            try
            {
                Tables.Messages message = JsonConvert.DeserializeObject<Tables.Messages>(json)!;
                _ = Task.Run(() => OnlineUsers.OnlineUsers.AddUserToList_IfUserNotInOnlineList(message.SenderId, clientSocket));
                int recipientId = JObject.Parse(json).Value<int>("RecipientId");

                TcpClient clientSocketRecipient;
                OnlineUsers.OnlineUsers.TryToGetValue(recipientId, out clientSocketRecipient);
                var stream = clientSocketRecipient?.GetStream();

                if (stream != null)
                {
                    var dataForRecipient = new
                    {
                        Command = "NewMessage",
                        Time = message.Time,
                        Message = message.Message,
                        ChatId = message.UserChatId,
                        Username = (await Database.Database.GetUserById(message.SenderId)).Username
                    };

                    _ = Server.Server._sendRequest(stream!, new Notification { Data = JsonConvert.SerializeObject(dataForRecipient) });
                }

                _ = Database.Database.InsertMessageToTableMessages(message);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        public async Task<Response> GetMessagesByContact(TcpClient clientSocket, string json)
        {
            try
            {
                Messages chatId = JsonConvert.DeserializeObject<Messages>(json)!;

                //_ = Task.Run(() => OnlineUsers.AddUserToList_IfUserNotInOnlineList(_getUserByTcpClient(clientSocket), clientSocket));//does not work

                List<ChatMessages> chatMessages = new List<ChatMessages>();

                var jsonArray = JArray.Parse(await Database.Database.GetMessages(chatId));

                // Пройтись по каждому элементу массива
                foreach (var item in jsonArray)
                {
                    chatMessages.Add(new ChatMessages { Message = item["Message"].ToString(), Username = item["Users"]["Username"].ToString(), Time = DateTimeOffset.Parse(item["Time"].ToString()) });
                }

                return new Response { Data = JsonConvert.SerializeObject(chatMessages) };

            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        public Response logOut(TcpClient clientSocket, string json)
        {
            try
            {
                OnlineUsers.OnlineUsers.DeleteUserFromOnlineList(clientSocket);
                Console.WriteLine("Client disconnected.");
                return new Response { };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Response { ErrorMessage = ex.Message };
            }
        }

        public async Task<Response> CreateNewChat(TcpClient clientSocket, string json)
        {
            try
            {
                var dataForNewChat = JsonConvert.DeserializeObject<Tables.NewChat>(json);
                _ = Task.Run(() => OnlineUsers.OnlineUsers.AddUserToList_IfUserNotInOnlineList(dataForNewChat.SenderId, clientSocket));
                var newChat = await Database.Database.CreateNewChat();

                var recipient = await Database.Database.GetUserById(dataForNewChat.RecipientId);

                var models = new List<UserChatUsers>
                {
                    new UserChatUsers{UserChatId = (int)newChat.Model.Id, UserId = dataForNewChat.SenderId},
                    new UserChatUsers{UserChatId = (int)newChat.Model.Id, UserId = recipient.Id}
                };

                var chats = await Database.Database.CreateChatConnection(models);

                TcpClient clientSocketRecipient;
                OnlineUsers.OnlineUsers.TryToGetValue(dataForNewChat.RecipientId, out clientSocketRecipient);

                var stream = clientSocketRecipient?.GetStream();

                if (stream != null)
                {
                    var dataForRecipient = new
                    {
                        Command = "NewChat",
                        Username = recipient.Username,
                        ChatId = (int)newChat.Model.Id
                    };
                    _ = Server.Server._sendRequest(stream!, new Notification { Data = JsonConvert.SerializeObject(dataForRecipient) });
                }

                return new Response { Data = JsonConvert.SerializeObject(new ChatModel { ChatId = (int)newChat.Model.Id, Contact = new ContactModel { Avatar = recipient.Avatar, Email = recipient.Email, Id = recipient.Id, Username = recipient.Username, LastLogin = recipient.LastLogin }, ChatName = recipient.Username, LastMessage = newChat.Model.LastMessage, Type = ChatType.Chat }) };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        public async Task<Response> FindUserByUsername(TcpClient clientSocket, string json)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Tables.Users>(json);
                //_ = Task.Run(() => OnlineUsers.AddUserToList_IfUserNotInOnlineList(_getUserByTcpClient(clientSocket), clientSocket)); //does not work
                var users = await Database.Database.FindUsersByUsername(data);

                var jsonArray = JArray.Parse(users.Content);
                List<object> findUsers = new List<object>();

                // Пройтись по каждому элементу массива
                foreach (var item in jsonArray)
                {
                    findUsers.Add(new { ChatName = item["Username"].ToString() });
                }

                return new Response { Data = JsonConvert.SerializeObject(findUsers) };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }
    }
}
