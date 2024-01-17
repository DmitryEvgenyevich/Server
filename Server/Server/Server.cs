using Server.Message;
using Server.MessengerFunctionality;
using System.Net.Sockets;
using System.Net;
using System.Reflection;

namespace Server.Server
{
    static class Server
    {
        public static async Task StartServerAsync()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 8000);

            try
            {
                tcpListener.Start();
                Console.WriteLine($"Server started. Waiting for clients...");

                while (true)
                {
                    TcpClient clientSocket = await tcpListener.AcceptTcpClientAsync();
                    _ = _handleClient(clientSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        private static async Task _handleClient(TcpClient clientSocket)
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

        private static async Task _waitingForRequest(NetworkStream stream, TcpClient clientSocket)
        {
            int bytesRead;
            byte[] buffer = new byte[1024];

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    _ = _processCommandAndSendResponse(GlobalUtilities.GlobalUtilities.ConvertBytesToString(buffer, bytesRead), stream, clientSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _ = GlobalUtilities.GlobalUtilities.SendRequest(stream, new Response { ErrorMessage = ex.Message });
            }
        }

        private static async Task _processCommandAndSendResponse(string json, NetworkStream stream, TcpClient clientSocket)
        {
            string command = GlobalUtilities.GlobalUtilities.TryToGetValueFromJsonByProperty(json, "Command");

            if (GlobalUtilities.GlobalUtilities.isStringEmpty(command))
            {
                _ = GlobalUtilities.GlobalUtilities.SendRequest(stream, new Response { ErrorMessage = "Can not find this property" });
            }
            else
            {
                Response response = await _executeCommandAndGetResponse(json, clientSocket, command);
                if (response.SendToClient)
                {
                    _ = GlobalUtilities.GlobalUtilities.SendRequest(stream, response);
                }
            }
        }

        private static async Task<Response> _executeCommandAndGetResponse(string json, TcpClient clientSocket, string nameElement)
        {
            MethodInfo method = typeof(MessengerFunctionalityDesktop).GetMethod(nameElement, BindingFlags.Public | BindingFlags.Instance)!;

            if (method == null)
            {
                return new Response { ErrorMessage = $"Method '{nameElement}' not found in MessengerFunctionalityDesktop" };
            }

            var messengerFunctionalityInstance = new MessengerFunctionalityDesktop();

            object[] parameters = { clientSocket, json };

            Task<Response> resultTask = (Task<Response>)method.Invoke(messengerFunctionalityInstance, parameters)!;

            return await resultTask.ConfigureAwait(false);
        }
    }
}
