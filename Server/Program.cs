using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Server.Message;

namespace Server
{
    class Server
    {
        static Dictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();

        static async void test(NetworkStream stream)
        {
            while (true)
            {
                string tet = Console.ReadLine();
                //await _sendMessage(stream, tet, "Notification");
            }
        }

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

        static void deleteUserFromList(TcpClient clientSocket)
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

        static async Task waitingForReqest(NetworkStream stream, TcpClient clientSocket)
        {
            int bytesRead;
            byte[] buffer = new byte[256];

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {

                    Response response = await _processCommand(_convertToString(buffer, bytesRead), clientSocket);
                    await _sendMessage(stream, response, "Response");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _sendMessage(stream, new Response { ErrorMessage = ex.Message }, "Response");
            }
        }

        static async void _handleClient(TcpClient clientSocket)
        {
            try
            {
                Console.WriteLine("Client connected");

                using (var stream = clientSocket.GetStream())
                {
                    Thread myThread = new Thread(() => test(stream)); // TODO Delete
                    myThread.Start(); // TODO delete

                    await waitingForReqest(stream, clientSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                deleteUserFromList(clientSocket);
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

        static async Task _sendMessage(NetworkStream stream, IMessage message, string temp = "")
        {
            string json = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true });

            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await stream.WriteAsync(bytes, 0, bytes.Length);
            await stream.FlushAsync();
        }

        static string tryToGetCommandFromJson(string json)
        { 
            JsonDocument jsonDocument = JsonDocument.Parse(json);
            JsonElement root = jsonDocument.RootElement;
            root.TryGetProperty("Command", out JsonElement nameElement);
            return nameElement.ToString();
        }

        static async Task<Response> _processCommand(string json, TcpClient clientSocket)
        {
            try
            {
                string nameElement = tryToGetCommandFromJson(json);

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

                case "logOut":

                    return _logOut(clientSocket, json);

                default:
                    return new Response { ErrorMessage = "Can not find this proparty" };
            }
        }

        static async Task<Response> signIn(TcpClient clientSocket, string json)
        {
            return new Response { Data = "signIn" };

        }

        static async Task<Response> signUp(TcpClient clientSocket, string json)
        {
            return new Response { Data = "signUp" };
        }

        static async Task<Response> _forgotPassword(TcpClient clientSocket, string json)
        {
            return new Response { Data = "_forgotPassword" };
        }

        static async Task<Response> _sendMessage(TcpClient clientSocket, string json)
        {
            /* //It works
            //byte[] buffer1 = new byte[128];
            //int bytesRead1;
            //NetworkStream stream1 = clientSocket.GetStream();
            //await _sendMessage(stream1, "write to");
            //stream1 = clientSocket.GetStream();
            //bytesRead1 = await stream1.ReadAsync(buffer1, 0, buffer1.Length);
            //stream1 = clientSocket.GetStream();
            //string id1 = _convertToString(buffer1, bytesRead1);
            //await _sendMessage(stream1, $"write to {id1}");

            //byte[] buffer2 = new byte[128];
            //int bytesRead2;
            //NetworkStream stream2 = clientSocket.GetStream();
            //bytesRead2 = await stream2.ReadAsync(buffer2, 0, buffer2.Length);

            //string message = _convertToString(buffer2, bytesRead2);

            //TcpClient clientTo = clients[id1];
            //await _sendMessage(clientTo.GetStream(), $"message from {message}");
            //stream2 = clientSocket.GetStream();
            //await _sendMessage(stream2, "done");*/


            return new Response { Data = "_sendMessage" };
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