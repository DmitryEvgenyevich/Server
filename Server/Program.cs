using System.Net;
using System.Net.Sockets;
using System.Text;

Server.startServer();

class Server
{
    public static void startServer()
    {
        TcpListener serverSocket = new TcpListener(IPAddress.Any, 8000);
        Console.WriteLine("Server started. Waiting for clients...");

        serverSocket.Start();

        try
        {
            while (true)
            {
                TcpClient clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client connected");

                handleClient(clientSocket);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            serverSocket.Stop();
            Console.WriteLine("Server stoped");
        }
    }

    static void handleClient(TcpClient clientSocket)
    {
        try
        {
            using (NetworkStream stream = clientSocket.GetStream())
            {
                byte[] buffer = new byte[256];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)/// hello 
                {
                    string command = readCommand(buffer, bytesRead);
                    sendMessage(stream, processCommand(command));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            clientSocket.Close();
            Console.WriteLine("Client disconnected.");
        }
    }

    static string processCommand(string command)
    {
        return "Server response: " + command;

        //Call to 
        //Chat to 

    }

    static string readCommand(byte[] buffer, int bytesRead)
    { 
        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        Console.WriteLine("Received from client: " + message);
        return message;
    }

    static void sendMessage(NetworkStream stream, string message)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(message);

        stream.Write(bytes, 0, bytes.Length);
        stream.Flush();
    }
}