using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Message;
using Server.Tables;

namespace Server
{
    class Server
    {
        private static Dictionary<int, TcpClient> _onlineClients = new Dictionary<int, TcpClient>();

        private static async Task Main(string[] args)
        {
            DB.DBinit();
            await _startServerAsync();
        }

        static async Task _startServerAsync()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 8000);
            
            try
            {
                serverSocket.Start();
                Console.WriteLine("Server started. Waiting for clients...");

                while (true)
                {
                    TcpClient clientSocket = await serverSocket.AcceptTcpClientAsync();
                    _handleClient(clientSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                serverSocket.Stop();
            }
        }

        static async void _handleClient(TcpClient clientSocket)
        {
            try
            {
                Console.WriteLine("Client connected");

                using (var stream = clientSocket.GetStream())
                {
                    await _waitingForRequest(stream, clientSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                _deleteUserFromOnlineList(clientSocket);
            }
            finally
            {
                Console.WriteLine("Client disconnected.");
            }
        }

        static async void _deleteUserFromOnlineList(TcpClient clientSocket)
        {
            try
            {
                int key = _onlineClients.FirstOrDefault(x => x.Value == clientSocket).Key;
                await DB.SetNewLastLoginById(key, DateTimeOffset.Now);
                _onlineClients.Remove(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static async Task _waitingForRequest(NetworkStream stream, TcpClient clientSocket)
        {
            int bytesRead;
            byte[] buffer = new byte[1024];

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    Response response = await _tryToGetCommand(GlobalUtils.GlobalUtils.ConvertBytesToString(buffer, bytesRead), clientSocket);
                    await _sendRequest(stream, response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _sendRequest(stream, new Response { ErrorMessage = ex.Message });
            }
        }

        static async Task<Response> _tryToGetCommand(string json, TcpClient clientSocket)
        {
            try
            {
                string command = GlobalUtils.GlobalUtils.TryToGetCommandFromJson(json, "Command").ToString();

                if (GlobalUtils.GlobalUtils.isStringEmpty(command))
                {
                    return new Response { ErrorMessage = "Can not find this proparty" };
                }

                return await _findCommand(json, clientSocket, command);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);

                return new Response { ErrorMessage = ex.Message };
            }
        }

        static async Task _sendRequest(NetworkStream stream, IMessage message)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new IMessageConverter() }
            };

            string json = System.Text.Json.JsonSerializer.Serialize(message, options);

            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await stream.WriteAsync(bytes, 0, bytes.Length);
            await stream.FlushAsync();
        }

        private static async Task<Response> _findCommand(string json, TcpClient clientSocket, string nameElement)
        {
           switch (nameElement)
            {
                case "SignIn":
                    return await _signIn(clientSocket, json);

                case "SignUp":
                    return await _signUp(json);

                case "ForgotPassword":
                    return await _forgotPassword(json);

                case "SendMessageInChat":
                    return await _sendMessageInChat(json);

                case "SendMessageInGroup":
                    return await _sendMessageInGroup(json);

                case "SendNewCode":
                    return await _sendNewCode(json);
                
                case "AuthSucces":
                    return await _authSucces(clientSocket, json);

                case "GetMyContacts":
                    return await _getMyChats(json);

                case "GetMessagesByContact":
                    return await _getMessagesByContact(json);

                case "logOut":
                    return await Task.Run(Response () => { return _logOut(clientSocket); });

                case "CreateNewChat":
                    return await _createNewChat(json);

                case "FindUserByUsername":
                    return await _findUserByUsername(json);

                default:
                    return new Response { ErrorMessage = "Can not find this proparty" };
            }
        }

        static async Task<Response> _signIn(TcpClient clientSocket, string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Tables.Users>(json);

                var result = await DB.GetUserByEmailAndPassword(user?.Email!, user?.Password!);

                if(result?.Id == null)
                    return new Response { ErrorMessage = "We can not to find your user, Email or password is not right" };

                if (result.Auth)
                {
                    _onlineClients.Add(result.Id, clientSocket);
                }

                return new Response { Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = ex.Message };
            }
        }

        static async Task<Response> _signUp(string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Tables.Users>(json);
                HttpResponseMessage httpResponse = await DB.InsertUserToTableUsers(user!);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _getMyChats(string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Tables.Users>(json);

                var chatsId = await DB.GetChatsByUserId(user!.Id);

                List<IChatGroupModels> chats = new List<IChatGroupModels>();

                foreach (UserChatUsers chat in chatsId)
                {
                    var jsonArray = JArray.Parse(await DB.GetContactEmailByChatId(chat.UserChatId, user.Id));
                    
                    foreach (var contact in jsonArray)
                    {
                        if ((ChatType)Enum.ToObject(typeof(ChatType), (int)(contact["UserChats"]["ChatType"])) == ChatType.Chat)
                        {
                            DateTimeOffset result;
                            var temp = DateTimeOffset.TryParse(contact["Users"]?["LastLogin"]?.ToString(), out result);
                            chats.Add(new ChatModel { ChatId = chat.UserChatId, Contact = new ContactModel { Avatar = contact["Users"]["Avatar"].ToString(), Email = contact["Users"]["Email"].ToString(), Id = ((int)contact["Users"]["Id"]), Username = contact["Users"]!["Username"]!.ToString(), LastLogin = temp ? result : null}, ChatName = contact["Users"]!["Username"]!.ToString(), LastMessage = contact["UserChats"]["LastMessage"].ToString(), Type = (ChatType)Enum.ToObject(typeof(ChatType), ((int)contact["UserChats"]["ChatType"])) });
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
                                    ((GroupModel)chatFromChats).ContactsInGroup.Add(new ContactModel { Avatar = contact["Users"]["Avatar"].ToString(), Email = contact["Users"]["Email"].ToString(), Id = ((int)contact["Users"]["Id"]), LastLogin = temp ? result : null, Username = contact["Users"]!["Username"]!.ToString()});
                                    test = false;
                                    break;
                                }
                            }
                            if(test)
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

        static async Task<Response> _sendNewCode(string json)
        {
            try
            {
                Tables.Users myObject = JsonConvert.DeserializeObject<Tables.Users>(json)!;
                await DB.UpdateAuthCodeByEmail(myObject.Email, myObject.AuthCode);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _authSucces(TcpClient clientSocket, string json)
        {
            try
            {
                Tables.Users myObject = JsonConvert.DeserializeObject<Tables.Users>(json)!;

                await DB.UpdateAuthStatus(myObject.Email, myObject.Auth);
                    
                var user = await DB.GetUserByEmail(myObject.Email);
                
                _onlineClients.Add(user.Id, clientSocket);
                
                return new Response { Data = JsonConvert.SerializeObject(user) };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _forgotPassword(string json)
        {
            try
            {
                var myObject = JsonConvert.DeserializeObject<Tables.Users>(json)!;
                await DB.UpdatePassword(myObject);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _sendMessageInGroup(string json)
        {
            try
            {
                Tables.Messages message = JsonConvert.DeserializeObject<Tables.Messages>(json)!;

                var tests = await DB.GetContactsIdsByChatId(message.UserChatId, message.SenderId);
                JArray jsonArray = JArray.Parse(tests);


                    foreach (JObject item in jsonArray)
                {
                    int userId = item["Users"].Value<int>("Id");
                    TcpClient clientSocket;
                    _onlineClients.TryGetValue(userId, out clientSocket);
                    var stream = clientSocket?.GetStream();
                    if (stream != null)
                    {
                        var dataForRecipient = new
                        {
                            Command = "NewMessage",
                            Time = message.Time,
                            Message = message.Message,
                            ChatId = message.UserChatId,
                            Username = (await DB.GetUserById(message.SenderId)).Username
                        };

                        await _sendRequest(stream!, new Notification { Data = JsonConvert.SerializeObject(dataForRecipient) });
                    }


                    await DB.InsertMessageToTableMessages(message);
                }
                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _sendMessageInChat(string json)
        {
            try
            {
                Tables.Messages message = JsonConvert.DeserializeObject<Tables.Messages>(json)!;
                
                int recipientId = JObject.Parse(json).Value<int>("RecipientId");

                TcpClient clientSocket;
                _onlineClients.TryGetValue(recipientId, out clientSocket);
                var stream = clientSocket?.GetStream();

                if (stream != null)
                {
                    var dataForRecipient = new
                    {
                        Command = "NewMessage",
                        Time = message.Time,
                        Message = message.Message,
                        ChatId = message.UserChatId,
                        Username = (await DB.GetUserById(message.SenderId)).Username
                    };

                    await _sendRequest(stream!, new Notification { Data = JsonConvert.SerializeObject(dataForRecipient) });
                }

                await DB.InsertMessageToTableMessages(message);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _getMessagesByContact(string json)
        {
            try
            {
                Messages chatId = JsonConvert.DeserializeObject<Messages>(json)!;

                List<ChatMessages> chatMessages = new List<ChatMessages>();

                var jsonArray = JArray.Parse(await DB.GetMessages(chatId));

                // Пройтись по каждому элементу массива
                foreach (var item in jsonArray)
                {   
                    chatMessages.Add(new ChatMessages { Message = item["Message"].ToString(), Username = item["Users"]["Username"].ToString(), Time = DateTimeOffset.Parse(item["Time"].ToString())});
                }

                return new Response { Data = JsonConvert.SerializeObject(chatMessages) };

            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static Response _logOut(TcpClient clientSocket)
        {
            try 
            { 
                int key = _onlineClients.FirstOrDefault(x => x.Value == clientSocket).Key;
                _onlineClients.Remove(key);
                Console.WriteLine("Client disconnected.");
                return new Response { };
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return new Response { ErrorMessage = ex.Message };
            }
        } 

        static async Task<Response> _createNewChat(string json)
        {
            try
            {
                var dataForNewChat = JsonConvert.DeserializeObject<Tables.NewChat>(json);

                var newChat = await DB.CreateNewChat();

                var recipient = await DB.GetUserById(dataForNewChat.RecipientId);

                var models = new List<UserChatUsers>
                {
                    new UserChatUsers{UserChatId = (int)newChat.Model.Id, UserId = dataForNewChat.SenderId},
                    new UserChatUsers{UserChatId = (int)newChat.Model.Id, UserId = recipient.Id}
                };

                var chats = await DB.CreateChatConnection(models);

                TcpClient clientSocket;
                _onlineClients.TryGetValue(dataForNewChat.RecipientId, out clientSocket);
                var stream = clientSocket?.GetStream();
                
                if (stream != null)
                {
                    var dataForRecipient = new
                    {
                        Command = "NewChat",
                        Username = recipient.Username,
                        ChatId = (int)newChat.Model.Id
                    };
                    await _sendRequest(stream!, new Notification { Data = JsonConvert.SerializeObject(dataForRecipient) });
                }

                return new Response { Data = JsonConvert.SerializeObject(new ChatModel { ChatId = (int)newChat.Model.Id, Contact = new ContactModel { Avatar = recipient.Avatar, Email = recipient.Email, Id = recipient.Id, Username = recipient.Username, LastLogin = recipient.LastLogin }, ChatName = recipient.Username, LastMessage = newChat.Model.LastMessage, Type = ChatType.Chat }) };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _findUserByUsername(string json)
        {
            try
            {   
                var data = JsonConvert.DeserializeObject<Tables.Users>(json);

                var users = await DB.FindUsersByUsername(data);

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