using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Message;
using Server.Tables;
using Supabase;

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

        static Dictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();

        static async Task Main(string[] args)
        {
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

        static void _deleteUserFromList(TcpClient clientSocket)
        {
            try
            {
                string key = clients.FirstOrDefault(x => x.Value == clientSocket).Key;
                clients.Remove(key);
            }
            catch (Exception ex2)
            {
                Console.WriteLine("Error: " + ex2.Message);
            }
        }

        static async Task _waitingForReqest(NetworkStream stream, TcpClient clientSocket)
        {
            int bytesRead;
            byte[] buffer = new byte[256];

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    Response response = await _processCommand(_convertToString(buffer, bytesRead), clientSocket);
                    await _sendRequest(stream, response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _sendRequest(stream, new Response { ErrorMessage = ex.Message } );
            }
        }

        static async void _handleClient(TcpClient clientSocket)
        {
            try
            {
                Console.WriteLine("Client connected");

                using (var stream = clientSocket.GetStream())
                {
                    await _waitingForReqest(stream, clientSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                _deleteUserFromList(clientSocket);
            }
            finally
            {
                Console.WriteLine("Client disconnected.");
            }
        }

        static string _convertToString(byte[] buffer, int bytesRead)
        {
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
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

        static async Task<Response> _processCommand(string json, TcpClient clientSocket)
        {
            try
            {
                string nameElement = GlobalUtils.GlobalUtils._tryToGetCommandFromJson(json, "Command");

                if (nameElement.ToString() == string.Empty)
                    return new Response { ErrorMessage = "Can not find this proparty" };

                return await _findCommand(json, clientSocket, nameElement);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return new Response { ErrorMessage = ex.Message };
            }
        }

        private static async Task<Response> _findCommand(string json, TcpClient clientSocket, string nameElement)
        {
            switch (nameElement)
            {
                case "SignIn":
                    return await signIn(clientSocket, json);

                case "SignUp":
                    return await signUp(clientSocket, json);

                case "ForgotPassword":
                    return await _forgotPassword(clientSocket, json);

                case "SendMessage":
                    return await _sendMessage(clientSocket, json);
                
                case "SendNewCode":
                    return await _sendNewCode(clientSocket, json);
                
                case "AuthSucces":
                    return await _authSucces(clientSocket, json);

                case "GetMyContacts":
                    return await _getMyContacts(clientSocket, json);

                case "GetMessagesByContact":
                    return await _getMessagesByContact(clientSocket, json);

                case "logOut":
                    return _logOut(clientSocket, json);

                default:
                    return new Response { ErrorMessage = "Can not find this proparty" };
            }
        }
        
        static async Task<Response> signIn(TcpClient clientSocket, string json)
        {
            try
            {
                Tables.Users user = JsonConvert.DeserializeObject<Tables.Users>(json)!;

                var result = await DB.GetUserByEmailAndPasswordIsRight(user.Email, user.Password);

                if(result?.Id == null)
                    return new Response { ErrorMessage = "We can not to find your user, Email or password is not right" };

                clients.Add(result.Username, clientSocket);
                return new Response { Data = JsonConvert.SerializeObject(result) };
            }
            catch (Exception ex)
            {
                return new Response { ErrorMessage = ex.Message };
            }
        }

        static async Task<Response> _getMyContacts(TcpClient clientSocket, string json)
        {
            try
            {
                Tables.Users user = JsonConvert.DeserializeObject<Tables.Users>(json)!;

                var myUser = await DB.GetUserIdByEmail(user.Email);

                var chats = await DB.GetChatsByUserId(myUser.Id);

                List<UsersJoinUserChatsUsers> contacts = new List<UsersJoinUserChatsUsers>();

                foreach (UserChatUsers chat in chats)
                {
                    var contactId = await DB.GetContactEmailByChatId(chat.user_chat_id, myUser.Id);
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

        static async Task<Response> signUp(TcpClient clientSocket, string json)
        {
            try
            {
                Tables.Users myObject = JsonConvert.DeserializeObject<Tables.Users>(json)!;
                await DB.InsertUserToTableUsers(myObject);


                return new Response { };
            }
            catch (Exception ex) 
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _sendNewCode(TcpClient clientSocket, string json)
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
                var supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);
                await supabase.InitializeAsync();

                Tables.Users myObject = JsonConvert.DeserializeObject<Tables.Users>(json)!;

                var value = await supabase
                    .From<Tables.Users>()
                    .Where(x => x.Email == myObject.Email)
                    .Set(x => x.Auth, myObject.Auth)
                    .Update();

                clients.Add(myObject.Username, clientSocket);
                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _forgotPassword(TcpClient clientSocket, string json)
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

        static async Task<Response> _sendMessage(TcpClient clientSocket, string json)
        {
            try
            {
                var supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);
                await supabase.InitializeAsync();

                Tables.Messages myObject = JsonConvert.DeserializeObject<Tables.Messages>(json)!;
                myObject.FileId = null;
                await DB.InsertMessageToTableMessages(myObject);

                return new Response { };
            }
            catch (Exception ex)
            {
                return GlobalUtils.GlobalUtils.GetErrorMessage(ex);
            }
        }

        static async Task<Response> _getMessagesByContact(TcpClient clientSocket, string json)
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

        static Response _logOut(TcpClient clientSocket, string json)
        {
            try 
            { 
                string key = clients.FirstOrDefault(x => x.Value == clientSocket).Key;
                clients.Remove(key);
                Console.WriteLine("Client disconnected.");
                return new Response { Data = "_logOut" };
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return new Response { ErrorMessage = ex.Message };
            }
        }

    }
}