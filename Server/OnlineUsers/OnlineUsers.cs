using System.Net.Sockets;

namespace Server.OnlineUsers
{
    internal class OnlineUsers
    {
        private static Dictionary<int, TcpClient> _onlineClients = new Dictionary<int, TcpClient>();

        private static int _getUserByTcpClient(TcpClient clientSocket)
        {
            return _onlineClients.FirstOrDefault(x => x.Value == clientSocket).Key;
        }

        public static async Task AddUserToList_IfUserIsNotInOnlineList(int idUser, TcpClient clientSocket)
        {
            await Task.Run(() =>
            {
                if (!_onlineClients.ContainsKey(idUser))
                    _onlineClients.Add(idUser, clientSocket);

            });
        }

        public static async Task DeleteUserFromOnlineList(TcpClient clientSocket)
        {
            try
            {
                int key = _getUserByTcpClient(clientSocket);
                await Database.Database.SetNewLastLoginById(key, DateTimeOffset.Now);
                _onlineClients.Remove(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public static void TryToGetValue(int recipientId, out TcpClient clientSocketRicipient)
        {
            _onlineClients.TryGetValue(recipientId, out clientSocketRicipient!);
        }
    }
}
