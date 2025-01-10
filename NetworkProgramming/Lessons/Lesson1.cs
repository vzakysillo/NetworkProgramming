using System.Net.Sockets;
using System.Net;
using System.Text;

namespace NetworkProgramming.Lessons
{
    public class Lesson1 : ILesson
    {
        public void Run()
        {
            IPAddress iPAddress = IPAddress.Parse("192.168.1.1");
            iPAddress = new IPAddress(new byte[] { 192, 168, 1, 1 }); // 32 bit

            var google = Dns.GetHostEntry("www.google.com");

            IPAddress googleIP = google.AddressList[0];

            IPEndPoint iPEndPoint = new IPEndPoint(googleIP, 80); // ip + port

            var socket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(iPEndPoint);
            var page = new StringBuilder();
            if (socket.Connected)
            {
                string getRequest = "GET / HTTP/1.1\r\nHost: www.google.com\r\nConnection: close\r\n\r\n";
                var data = Encoding.UTF8.GetBytes(getRequest);
                var buffer = new byte[1024];
                socket.Send(data, SocketFlags.None);
                int bytes = 0;
                do
                {
                    bytes = socket.Receive(buffer, buffer.Length, 0);
                    page.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
                } while (bytes > 0);

                Console.WriteLine(page);
            }


            socket.Close();
        }
    }
}
