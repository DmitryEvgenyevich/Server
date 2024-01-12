using Server.Message;
using System.Net.Sockets;

namespace Server.MessengerFunctionality
{
    internal interface IMessengerFunctionality
    {

        Task<Response> SignIn(TcpClient clientSocket, string json);

        Task<Response> SignUp(TcpClient clientSocket, string json);

        Task<Response> GetMyContacts(TcpClient clientSocket, string json);

        Response SendNewCode(TcpClient clientSocket, string json);

        Task<Response> AuthSuccess(TcpClient clientSocket, string json);

        Task<Response> ForgotPassword(TcpClient clientSocket, string json);

        Task<Response> SendMessageInGroup(TcpClient clientSocket, string json);

        Task<Response> SendMessageInChat(TcpClient clientSocket, string json);

        Task<Response> GetMessagesByContact(TcpClient clientSocket, string json);

        Response logOut(TcpClient clientSocket, string json);

        Task<Response> CreateNewChat(TcpClient clientSocket, string json);

        Task<Response> FindUserByUsername(TcpClient clientSocket, string json);

    }
}
