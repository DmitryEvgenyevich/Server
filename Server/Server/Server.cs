using Server.Message;
using Server.MessengerFunctionality;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Server.Server
{
    static class Server
    {
        public static async Task _startServerAsync()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 8000);

            try
            {
                serverSocket.Start();
                Console.WriteLine($"Server started. Waiting for clients...");

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
            }
            finally
            {
                _ = OnlineUsers.OnlineUsers.DeleteUserFromOnlineList(clientSocket);
                Console.WriteLine("Client disconnected.");
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
                    Response response = await _tryToGetCommand(GlobalUtilities.GlobalUtilities.ConvertBytesToString(buffer, bytesRead), clientSocket);
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
                string command = GlobalUtilities.GlobalUtilities.TryToGetCommandFromJson(json, "Command").ToString();

                if (GlobalUtilities.GlobalUtilities.isStringEmpty(command))
                {
                    return new Response { ErrorMessage = "Can not find this property" };
                }

                return await _callCommand(json, clientSocket, command);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);

                return new Response { ErrorMessage = ex.Message };
            }
        }

        public static async Task _sendRequest(NetworkStream stream, IMessage message)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new IMessageConverter() }
            };

            string json = JsonSerializer.Serialize(message, options);

            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await stream.WriteAsync(bytes, 0, bytes.Length);
            await stream.FlushAsync();
        }

        static async Task<Response> _callCommand(string json, TcpClient clientSocket, string nameElement)
        {
            MethodInfo method = typeof(MessengerFunctionalityDesktop).GetMethod(nameElement, BindingFlags.Public | BindingFlags.Instance)!;

            if (method == null)
            {
                return new Response { ErrorMessage = "Can not find this property" };
            }

            var messengerFunctionalityInstance = new MessengerFunctionalityDesktop();

            object[] parameters = { clientSocket, json };

            Task<Response> resultTask = (Task<Response>)method.Invoke(messengerFunctionalityInstance, parameters)!;

            return await resultTask;
        }
    }
}
