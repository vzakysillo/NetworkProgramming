using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgramming.Lessons
{
    public class Lesson4 : ILesson
    {
        static public void ReceiveMessage(UdpClient udpClient)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                byte[] buffer = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"-> [{remoteEndPoint}]: {message}");
            }
        }

        static public void SendMessage(UdpClient udpClient, IPEndPoint targetEndPoint)
        {
            while (true)
            {
                string message = Console.ReadLine() ?? "";
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, targetEndPoint);
            }
        }

        static public void ServerChat()
        {
            UdpClient server = new UdpClient(12345);
            Console.WriteLine("Server started and waiting for messages...");

            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Loopback, 12346);
            Task.Run(() => ReceiveMessage(server));
            SendMessage(server, clientEndPoint);

            server.Close();
        }

        static public void ClientChat()
        {
            UdpClient client = new UdpClient(12346);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, 12345);

            Console.WriteLine("Connected to the server!");

            Task.Run(() => ReceiveMessage(client));
            SendMessage(client, serverEndPoint);

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

    class Message
    {
        public IPAddress IP{  get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }

        public Message(IPAddress ip, string text, DateTime time)
        {
            IP = ip;
            Text = text;
            Time = time;
        }


        public override string ToString()
        {
            return $"{IP.ToString()} at {Time:HH:mm}: {Text}";
        }
    }

    public class LocalChat : ILesson
    {
        private static int port = 12345;
        private UdpClient udpClient;
        private IPAddress broadcastAddress = IPAddress.Parse("255.255.255.255");
        private IPAddress myip = IPAddress.Parse("192.168.0.173");

        List<Message> messages = new List<Message>();
        string _input = "";

        public LocalChat()
        {
            udpClient = new UdpClient(port);
            udpClient.EnableBroadcast = true;
        }

        public void SendToSpecificIP(string message, IPAddress targetIp)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                IPEndPoint endPoint = new IPEndPoint(targetIp, port);
                udpClient.Send(data, data.Length, endPoint);

                messages.Add(new Message(myip, message, DateTime.Now));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SendBroadcast(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                IPEndPoint endPoint = new IPEndPoint(broadcastAddress, port);
                udpClient.Send(data, data.Length, endPoint);
                messages.Add(new Message(broadcastAddress, message, DateTime.Now));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ReceiveMessages()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                while (true)
                {
                    byte[] receivedData = udpClient.Receive(ref endPoint);
                    string message = Encoding.UTF8.GetString(receivedData);
                    messages.Add(new Message(endPoint.Address, message, DateTime.Now));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }

        public void PrintAllMessages()
        {
            Console.Clear();

            Console.SetCursorPosition(0, 0);
            foreach (Message i in messages)
            {
                Console.WriteLine(i);
            }

            Console.WriteLine(_input);
        }

        bool flag = false;

        public void Run()
        {
            Console.Write("Enter an ip: ");
            string ipInput = Console.ReadLine() ?? "";
            //string ipInput = "192.168.0.119";

            
            Task.Run(ReceiveMessages);

            while (true)
            {

                Thread.Sleep(100);

                PrintAllMessages();
                Input();

                if (!flag)
                    continue;

                if (string.IsNullOrEmpty(_input))
                    continue;

                if (string.IsNullOrEmpty(ipInput))
                    SendBroadcast(_input);
                else
                    SendToSpecificIP(_input, IPAddress.Parse(ipInput));

                _input = "";
            }
        }

        public void Input()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                char k = cki.KeyChar;
                if (cki.Key == ConsoleKey.Enter)
                {
                    flag = true;
                }
                else
                {
                    _input += k;
                    flag = false;
                }
            }
        }


    }
}
