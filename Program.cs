using MySockets;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ConsoleApp1
{
    public class User
    {
        public string Login;
        public string Password;
    }

    class Program
    {


        static void ClientPart()
        {
            Net local = new Net();
            local._Sock = local.CreateSocket();
            local.Connect("127.0.0.1");
            Console.WriteLine("Сокет соединяется с {0} ", local._Sock.RemoteEndPoint.ToString());
            string input;
            byte[] buf = new byte[1024];
            byte[] nullByte = new byte[1];
            nullByte[0] = 0;
            bool isAccessed = false;
            local.Send(nullByte);
            while (!isAccessed)
            {
                int bytesRec = local.Receive(buf);
                Console.Write(Encoding.UTF8.GetString(buf, 0, bytesRec));
                input = Console.ReadLine();
                local.Send(Encoding.UTF8.GetBytes("admin\0"));

                bytesRec = local.Receive(buf);
                Console.Write(Encoding.UTF8.GetString(buf, 0, bytesRec));
                input = Console.ReadLine();
                local.Send(Encoding.UTF8.GetBytes("admin"));

                bytesRec = local.Receive(buf);
                Console.Write(Encoding.UTF8.GetString(buf, 0, bytesRec));
                input = Console.ReadLine();
                local.Send(Encoding.UTF8.GetBytes("vt100/9600"));

                bytesRec = local.Receive(buf);
                Console.Write(Encoding.UTF8.GetString(buf, 0, bytesRec));

                bytesRec = local.Receive(buf);
                Console.Write(Encoding.UTF8.GetString(buf, 0, bytesRec));
                input = Console.ReadLine();
                local.Send(Encoding.UTF8.GetBytes("admin"));
                bytesRec = local.Receive(buf);
                if (Encoding.UTF8.GetString(buf, 0, bytesRec) == "Incorrect login or password, wanna try again?[Y/N]")
                {
                    input = Console.ReadLine();
                    input = input.ToUpper();
                    while (input != "N" || input != "Y")
                    {
                        input = Encoding.UTF8.GetString(buf, 0, bytesRec);
                        input = input.ToUpper();
                    }
                    local.Send(Encoding.UTF8.GetBytes(input));
                    if (input == "N")
                        return;
                }
                else
                {
                    isAccessed = true;
                    Console.WriteLine("Connected");
                }
            }
            for (; ; )
            {
                buf = new byte[1024];
                input = Console.ReadLine();
                if (input == "") input = "\n";
                local.Send(Encoding.UTF8.GetBytes(input));
                int bytesRec = local.Receive(buf);
                Console.WriteLine(Encoding.UTF8.GetString(buf, 0, bytesRec).Trim());
                //bytesRec = local.Receive(buf);
                //Console.WriteLine(Encoding.UTF8.GetString(buf, 0, bytesRec).Trim());
            }
            local.ShutDown(3);
            local.Close();
        }

        static List<User> AcceptedUsers;

        static void WorkWithClient(object x)
        {
            Socket dest = x as Socket;
            Console.WriteLine("Сокет соединился с {0} ", dest.RemoteEndPoint.ToString());
            byte[] buf = new byte[1024];
            int bytesRec = dest.Receive(buf);
            Console.WriteLine(Encoding.UTF8.GetString(buf, 0, bytesRec));

            bool Valid = false;
            while (!Valid)
            {
                dest.Send(Encoding.UTF8.GetBytes("Local name: "));
                bytesRec = dest.Receive(buf);
                string localName = Encoding.UTF8.GetString(buf, 0, bytesRec);
                Console.WriteLine(dest.RemoteEndPoint.ToString(), localName);

                dest.Send(Encoding.UTF8.GetBytes("Hots name: "));
                bytesRec = dest.Receive(buf);
                string login = Encoding.UTF8.GetString(buf, 0, bytesRec);
                Console.WriteLine(dest.RemoteEndPoint.ToString(), login);

                dest.Send(Encoding.UTF8.GetBytes("Console Type and Speed: "));
                bytesRec = dest.Receive(buf);
                string typeAndSpeed = Encoding.UTF8.GetString(buf, 0, bytesRec);
                Console.WriteLine(dest.RemoteEndPoint.ToString(), typeAndSpeed);
                byte[] nullByte = new byte[1];
                nullByte[0] = 0;

                dest.Send(nullByte);

                dest.Send(Encoding.UTF8.GetBytes("Password: "));
                bytesRec = dest.Receive(buf);
                string password = Encoding.UTF8.GetString(buf, 0, bytesRec);
                Console.WriteLine(password);

                for (int i = 0; i < AcceptedUsers.Count; i++)
                {
                    if (AcceptedUsers[i].Login == login && AcceptedUsers[i].Password == password)
                    {
                        Valid = true;
                        dest.Send(Encoding.UTF8.GetBytes("Valid connect"));
                        break;
                    }
                }

                if (!Valid)
                {
                    dest.Send(Encoding.UTF8.GetBytes("Incorrect login or password, wanna try again?[Y/N]"));

                    bytesRec = dest.Receive(buf);
                    string answer = Encoding.UTF8.GetString(buf, 0, bytesRec);
                    
                    if (answer == "N")
                        return;
                }

            }
            string cmd = "/c";
            for (; ; )
            {
                cmd = "/c ";
                buf=new byte[1024];
                bytesRec = dest.Receive(buf);
                //dest.Send(buf);
                Console.WriteLine(cmd += Encoding.UTF8.GetString(buf, 0, bytesRec));
                //string cmd = "/c dir";
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.Arguments = cmd;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                if (output == "")
                {
                    output = "error";
                }
                    buf = Encoding.UTF8.GetBytes(output);
                    dest.Send(buf);
            }
            return;
        }

        static void ServerPart()
        {
            Net local = new Net();
            local._Sock = local.CreateSocket();
            local.Bind();
            List<Thread> myThread = new List<Thread>();
            List<Socket> clients = new List<Socket>();
            for (; ; )
            {
                Socket dest = local.Accept();
                myThread.Add(new Thread(new ParameterizedThreadStart(WorkWithClient)));
                myThread[^1].Start(dest);
                clients.Add(dest);
            }

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
            AcceptedUsers = new List<User>();
            AcceptedUsers.Add(new User()
            {
                Login = "admin",
                Password = "admin"
            });
            Console.WriteLine("[1]Server, [2] Client");
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
