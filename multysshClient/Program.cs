using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace multysshClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MultySSH Client Running...");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep1 = new IPEndPoint(IPAddress.Broadcast, 10001);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            byte[] buffer = Encoding.ASCII.GetBytes("MultySSH");
            while (true)
            {
                socket.SendTo(buffer, iep1);
                Thread.Sleep(300); 
            }
        }
    }
}
