﻿using System.Net.Sockets;

namespace Server.OnlineUsers
{
    internal class OnlineUsers
    {
        static Dictionary<int, TcpClient> _onlineClients = new Dictionary<int, TcpClient>();

        static int _getUserByTcpClient(TcpClient clientSocket)
        {
            return _onlineClients.FirstOrDefault(x => x.Value == clientSocket).Key;
        }

        public static void AddUserToList_IfUserNotInOnlineList(int idUser, TcpClient clientSocket)
        {
            if (!_onlineClients.ContainsKey(idUser))
                _onlineClients.Add(idUser, clientSocket);
        }

        public static async void DeleteUserFromOnlineList(TcpClient clientSocket)
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