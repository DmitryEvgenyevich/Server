using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Message;
using Server.Tables;
using Server.GlobalUtils;
using Supabase;
using Supabase.Gotrue;

namespace Server
{
    class Server
    {
        static string supabaseUrl = "https://xqtbulboyjkpozsnyttc.supabase.co";

        static string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhxdGJ1bGJveWprcG96c255dHRjIiwicm9sZSI6ImFub24iLCJpYXQiOjE2OTIxMTM1MzcsImV4cCI6MjAwNzY4OTUzN30.qPD7zjTMELHmJV7Tkynn8WwyLFmh2uO0-_tU3EQk_H0";

        static SupabaseOptions options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        private static Dictionary<string, TcpClient> _onlineClients = new Dictionary<string, TcpClient>();

        private static async Task Main(string[] args)
        {
            await _startServerAsync();
           // await _createNewChat("");
        }//refacted

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
        }//refacted

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
        }//refacted

        static void _deleteUserFromOnlineList(TcpClient clientSocket)
        {
            try
            {
                string key = _onlineClients.FirstOrDefault(x => x.Value == clientSocket).Key;
                _onlineClients.Remove(key);
            }
            catch (Exception ex2)
            {
                Console.WriteLine("Error: " + ex2.Message);
            }
        } //refacted

        static async Task _waitingForRequest(NetworkStream stream, TcpClient clientSocket)
        {
            int bytesRead;
            byte[] buffer = new byte[512];

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
        }//refacted

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
        }//refacted

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
        }//refacted

        private static async Task<Response> _findCommand(string json, TcpClient clientSocket, string nameElement)
        {
            switch (nameElement)
            {
                case "SignIn":
                    return await _signIn(clientSocket, json);

                case "SignUp":
                    return await _signUp(json);

                case "ForgotPassword":
                    //return await _forgotPassword(json);

                case "SendMessage":
                    //return await _sendMessage(json);
                
                case "SendNewCode":
                    return await _sendNewCode(json);
                
                case "AuthSucces":
                    return await _authSucces(clientSocket, json);

                case "GetMyContacts":
                    return await _getMyContacts(json);

                case "GetMessagesByContact":
                    //return await _getMessagesyContact(json);

                case "logOut":
                    return await Task.Run(Response () => { return _logOut(clientSocket); });

                case "CreateNewChat":
                    return await _createNewChat(json);

                default:
                    return new Response { ErrorMessage = "Can not find this proparty" };
            }
        }//refacted

        static async Task<Response> _signIn(TcpClient clientSocket, string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Tables.Users>(json);

                var result = await DB.GetUserByEmailAndPassword(user?.Email!, user?.Password!);

                if(result?.Id == null)
                    return new Response { ErrorMessage = "We can not to find your user, Email or password is not right" };

                _onlineClients.Add(result.Username, clientSocket);
                return new Response { Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = ex.Message };
            }
        }//refacted

        static async Task<Response> _signUp(string json)
        {
            try
            {
                var myObject = JsonConvert.DeserializeObject<Tables.Users>(json);
                HttpResponseMessage httpResponse = await DB.InsertUserToTableUsers(myObject!); //// TO DO

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }//TO DO

        static async Task<Response> _getMyContacts(string json)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<Tables.Users>(json);

                var chats = await DB.GetChatsByUserId(user!.Id);

                List<UsersJoinUserChatsUsers> contacts = new List<UsersJoinUserChatsUsers>();

                foreach (UserChatUsers chat in chats)
                {
                    var contactId = await DB.GetContactEmailByChatId(chat.user_chat_id, user.Id);
                    var contact = await DB.GetUserById(contactId);

                    contacts.Add(new UsersJoinUserChatsUsers
                    { 
                        Avatar = contact.Avatar, 
                        ChatId = chat.user_chat_id, 
                        LastLogin = contact.Last_login, 
                        Username = contact.Username 
                    });
                }

                return new Response { Data = JsonConvert.SerializeObject(contacts) };

            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _sendNewCode(string json)
        {
            try
            {
                Tables.Users myObject = JsonConvert.DeserializeObject<Tables.Users>(json)!;
                await DB.UpdateAuthCodeByEmail(myObject.Email, myObject.Auth_code);

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
                
                _onlineClients.Add(user.Username, clientSocket);
                
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
                var supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);
                await supabase.InitializeAsync();

                Tables.Users myObject = JsonConvert.DeserializeObject<Tables.Users>(json)!;

                var value = await supabase
                    .From<Tables.Users>()
                    .Where(x => x.Email == myObject.Email)
                    .Set(x => x.Auth, myObject.Auth)
                    .Set(x => x.Password, myObject.Password)
                    .Update();

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _sendMessage(string json)
        {
            try
            {
                var supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);
                await supabase.InitializeAsync();

                Tables.Messages myObject = JsonConvert.DeserializeObject<Tables.Messages>(json)!;
                myObject.FileId = null;
                await DB.InsertMessageToTableMessages(myObject);

                int userId = await DB.GetContactEmailByChatId(myObject.UserChatId, myObject.SenderId);
                var user = await DB.GetUserById(userId);

                try
                {
                    TcpClient clientSocket;
                    _onlineClients.TryGetValue(user.Username, out clientSocket);
                    var stream = clientSocket?.GetStream();
                    await _sendRequest(stream!, new Notification { Type = "new message" });
                }
                catch
                {

                }

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

                var supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);
                await supabase.InitializeAsync();

                var messages = await supabase
                    .From<Messages>()
                    .Select("users:sender_id(username), time, message")
                    .Where(x => x.UserChatId == chatId.UserChatId)
                    .Get();

                var jsonArray = JArray.Parse(messages.Content);

                // Пройтись по каждому элементу массива
                foreach (var item in jsonArray)
                {
                    DateTimeOffset.TryParse(item["time"].ToString(), out DateTimeOffset result);
                    DateTime dateTime = result.DateTime;

                    chatMessages.Add(new ChatMessages { Message = item["message"].ToString(), Username = item["users"]["username"].ToString(), Time = dateTime });
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
                string key = _onlineClients.FirstOrDefault(x => x.Value == clientSocket).Key;
                _onlineClients.Remove(key);
                Console.WriteLine("Client disconnected.");
                return new Response { };
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return new Response { ErrorMessage = ex.Message };
            }
        } //refacted

        static async Task<Response> _createNewChat(string json)
        {
            try
            {
                var dataForNewChat = JsonConvert.DeserializeObject<Tables.NewChat>(json);

                var supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);
                await supabase.InitializeAsync();

                var newChat = await supabase.From<UserChats>().Insert(new UserChats());

                var recipient = await supabase.From<Users>().Select(x => new object[] { x.Id }).Where(x => x.Username == dataForNewChat.RecipientUsername).Single();

                var models = new List<UserChatUsers>
                {
                    new UserChatUsers{user_chat_id = 7, user_id = dataForNewChat.SenderId},
                    new UserChatUsers{user_chat_id = 7, user_id = recipient.Id}
                };

                var chats = await supabase.From<UserChatUsers>().Insert(models);

                // return new Response { Data = JsonConvert.SerializeObject(contact) };
                return new Response();
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }
    }
}