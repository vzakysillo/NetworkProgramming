using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgramming.Lessons
{
    public class Lesson3 : ILesson
    {
        static public void ReceiveMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("-> " + message);
            }
        }

        static public void SendMessage(NetworkStream stream)
        {
            while (true)
            {
                string message = Console.ReadLine() ?? "";
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

        static public void ServerChat()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 12345);
            server.Start();

            Console.WriteLine("Server started and waiting for connections...");

            TcpClient client = server.AcceptTcpClient();

            Console.WriteLine("Client connected!");

            NetworkStream stream = client.GetStream();
            Task.Run(() => ReceiveMessage(stream));
            SendMessage(stream);

            client.Close();
            server.Stop();
        }

        static public void ClientChat()
        {
            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Loopback, 12345);
            Console.WriteLine("Connected to the server!");

            NetworkStream stream = client.GetStream();
            Task.Run(() => ReceiveMessage(stream));
            SendMessage(stream);

            client.Close();
        }

        public void Run()
        {
            var role = Console.ReadLine();

            if (role?.ToLower() == "server")
                ServerChat();
            else if (role?.ToLower() == "client")
                ClientChat();
            else
                Console.WriteLine("Invalid input.");
        }

    }
}
