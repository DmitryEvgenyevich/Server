using System.IO;
using System;
using System.IO.Pipes;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.VisualBasic;
using NAudio.Wave;

class Server
{
    static BufferedWaveProvider bufferedWaveProvider;
    static WaveOutEvent waveOut;

    static Dictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();
    
    static async Task Main(string[] args)
    {
        await _startServerAsync();
    }

    static async Task _startServerAsync()
    {
        TcpListener serverSocket = new TcpListener(IPAddress.Any, 8000);
        bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
        waveOut = new WaveOutEvent();
        waveOut.Init(bufferedWaveProvider);
        try
        {
            serverSocket.Start();
            Console.WriteLine("Server started. Waiting for clients...");

            while (true)
            {
                TcpClient clientSocket = await serverSocket.AcceptTcpClientAsync();
                _ = _handleClient(clientSocket);
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

    static async Task _handleClient(TcpClient clientSocket)
    {
        try
        {
            Console.WriteLine("Client connected");
            
            using (NetworkStream stream = clientSocket.GetStream())
            {
                byte[] buffer = new byte[128];
                int bytesRead;


                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    _ = _processCommand(_convertToString(buffer, bytesRead), clientSocket);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            string key = clients.FirstOrDefault(x => x.Value == clientSocket).Key;
            clients.Remove(key);
            Console.WriteLine("Client disconnected.");
        }
    }

    static string _convertToString(byte[] buffer, int bytesRead)
    {
        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
    }

    static async Task _sendMessage(NetworkStream stream, string message)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(message);

        await stream.WriteAsync(bytes, 0, bytes.Length);
        await stream.FlushAsync();
    }
    
    static async Task _processCommand(string command, TcpClient clientSocket)
    {
        
        switch (command)
        {
            case "add":
                byte[] buffer = new byte[128];
                int bytesRead;
                NetworkStream stream = clientSocket.GetStream();
                await _sendMessage(stream, "send id ");
                stream = clientSocket.GetStream();
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string id = _convertToString(buffer, bytesRead);
                stream = clientSocket.GetStream();
                clients.Add(id, clientSocket);
                await _sendMessage(stream, "you are aded");
                break;
            case "SignIn":
                //_ = await signIn(clientSocket);
                break;
            case "signUp":
                //_ = await signUp(clientSocket);
                break;
            case "forgotPass":
                //_ = await _forgotPass(clientSocket);
                break;

            case "sendSMS":
                //_ = await _sendSMS(clientSocket);
                break;

            case "call":
                //byte[] buffer1 = new byte[128];
                //int bytesRead1;
                //NetworkStream stream1 = clientSocket.GetStream();
                //await _sendMessage(stream1, "write to");
                //stream1 = clientSocket.GetStream();
                //bytesRead1 = await stream1.ReadAsync(buffer1, 0, buffer1.Length);
                //stream1 = clientSocket.GetStream();
                //string id1 = _convertToString(buffer1, bytesRead1);
                //TcpClient clientTo = clients[id1];


                NetworkStream stream3 = clientSocket.GetStream();

                byte[] buffer3 = new byte[8192];
                while (true)
                {
                    int bytesRead3 = stream3.Read(buffer3, 0, buffer3.Length);
                    bufferedWaveProvider.AddSamples(buffer3, 0, bytesRead3);
                    waveOut.Play();
                }
                break;

            case "logOut":
                //_ = await _logOut(clientSocket);
                break;

            default:
                break;
        }
    }

    static async Task signIn(TcpClient clientSocket)
    {
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
        //await _sendMessage(stream2, "done");
    }

    static async Task signUp(TcpClient clientSocket)
    {
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
        //await _sendMessage(stream2, "done");
    }

    static async Task _forgotPass(TcpClient clientSocket)
    {
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
        //await _sendMessage(stream2, "done");
    }

    static async Task _sendSMS(TcpClient clientSocket)
    {
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
        //await _sendMessage(stream2, "done");
    }

    static async Task _call(TcpClient clientSocket)
    {
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
        //await _sendMessage(stream2, "done");
    }

    static async Task _logOut(TcpClient clientSocket)
    {
        string key = clients.FirstOrDefault(x => x.Value == clientSocket).Key;
        clients.Remove(key);
        Console.WriteLine("Client disconnected.");
    }

}