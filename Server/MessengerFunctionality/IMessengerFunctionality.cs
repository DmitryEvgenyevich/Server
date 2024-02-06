using Server.Message;
using System.Net.Sockets;

namespace Server.MessengerFunctionality
{
    internal interface IMessengerFunctionality
    {
        Task<Response> SignIn(TcpClient clientSocket, string json);

        Task<Response> SignUp(TcpClient clientSocket, string json);

        Task<Response> GetMyContacts(TcpClient clientSocket, string json);

        Task<IMessage> SendNewCode(TcpClient clientSocket, string json);

        Task<Response> IsCodeRight(TcpClient clientSocket, string json);

        Task<Response> UpdatePassword(TcpClient clientSocket, string json);

        Task<Response> SendMessageInGroup(TcpClient clientSocket, string json);

        Task<IMessage> SendMessageInChat(TcpClient clientSocket, string json);

        Task<Response> GetMessagesByContact(TcpClient clientSocket, string json);

        Response logOut(TcpClient clientSocket, string json);

        Task<Response> CreateNewChat(TcpClient clientSocket, string json);

        Task<Response> FindUserByUsername(TcpClient clientSocket, string json);
    }
}
