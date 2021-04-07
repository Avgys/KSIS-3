using MySockets;
using System.Threading;
using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ConsoleApp1
{
    class Program
    {

        //public static void Send(object x)
        //{
        //    Net local = x as Net;
        //    string message = "1";
        //    byte[] msg = Encoding.UTF8.GetBytes(message);
        //    while (true)
        //    {
        //        int bytesSent = local.Send(msg);
        //    }
        //}

        //public static void Receive(object x)
        //{
        //    Net local = x as Net;
        //    byte[] bytes = new byte[1024];
        //    while (true)
        //    {
        //        int bytesRec = local.Receive(bytes);
        //        Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));
        //    }
        //}



        static void ClientPart()
        {
            Net local = new Net();
            local._Sock = local.CreateSocket();
            local.Connect("127.0.0.1");
            Console.WriteLine("Сокет соединяется с {0} ", local._Sock.RemoteEndPoint.ToString());
            local.Send(Encoding.UTF8.GetBytes(""));
            local.Send(Encoding.UTF8.GetBytes("bostic"));
            local.Send(Encoding.UTF8.GetBytes("kbostix"));
            local.Send(Encoding.UTF8.GetBytes("vt100/9600"));
            byte[] buf = new byte[1024];
            bytesRec = local.Receive(buf);
            //Thread sendThread = new Thread(new ParameterizedThreadStart(Send));
            //sendThread.Start(local); // запускаем поток

            //Thread receiveThread = new Thread(new ParameterizedThreadStart(Receive));
            //receiveThread.Start(local); // запускаем 
            //sendThread.Join();
            //receiveThread.Join();
            local.ShutDown(3);
            local.Close();
        }
        static void ServerPart()
        {
            Net local = new Net();
            local._Sock = local.CreateSocket();
            local.Bind();
            List<Socket> clients = new List<Socket>();
            Socket dest = local.Accept();
            byte[] buf = new byte[1024];
            local.Receive(buf);
            int bytesRec = local.Receive(buf);
            string localName = Encoding.UTF8.GetString(buf, 0, bytesRec);
            bytesRec = local.Receive(buf);
            string serverName = Encoding.UTF8.GetString(buf, 0, bytesRec);
            bytesRec = local.Receive(buf);
            string typeAndSpeed = Encoding.UTF8.GetString(buf, 0, bytesRec);
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Shutdown(SocketShutdown.Both);
                clients[i].Close();
            }
            local.ShutDown();
            local.Close();
        }


        static void Main(string[] args)
        {
            var input = Console.ReadLine();
            switch (Int32.Parse(input))
            {
                case 1:
                    ServerPart();
                    break;
                case 2:
                    ClientPart();
                    break;
            }
            
        }
    }
}
