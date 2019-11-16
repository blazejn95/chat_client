using System;
using System.Net;
using System.Net.Sockets;
using MyData;
using System.Threading;
namespace client
{

    class Client
    {
        public static Socket MainSocket;
        public static string IDfromServer;
        public static bool Logged = false;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello! Enter server's IP");
            string servip = Console.ReadLine();
            MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint Ip = new IPEndPoint(IPAddress.Parse(servip), 111);
            MainSocket.Connect(Ip);
            Thread MyThread = new Thread(Input);
            MyThread.Start();


            while (!Logged)
            {
                Thread.Sleep(50);
            }

            while (true)
            {

                Console.Write(">");
                string MsgText = Console.ReadLine();
                Data broadcast = new Data(IDfromServer, "regular");
                broadcast.data.Add(MsgText);

                MainSocket.Send(broadcast.ToBytes());
            }
        }

        public static void Input()
        {

            byte[] Buffer = new byte[MainSocket.SendBufferSize];
            int Read;
            while (true)
            {
                try
                {
                    Read = MainSocket.Receive(Buffer);
                    if (Read > 0)
                    {
                        Data d = new Data(Buffer);
                        DataManager(d);
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Server is down");
                }
            }

        }
        public static void DataManager(Data RecData)
        {
            switch (RecData.Type)
            {
                case "registration":
                    IDfromServer = RecData.SenderID;
                    FillIn();

                    break;
                case "regular":
                    Console.WriteLine(RecData.data[0]);
                    Console.Write(">");
                    break;
                case "confirmreg":
                    Console.WriteLine(RecData.data[0]);
                    Logged = true;
                    break;
                case "confirmlogin":
                    Console.WriteLine(RecData.data[0]);
                    Logged = true;

                    break;
                case "refused":
                    Console.WriteLine("user with this login alrdy exists or username/pass invalid");
                    FillIn();

                    break;
            }
        }
        public static void FillIn()
        {
            bool LoopCondition = true;

            while (LoopCondition)
            {
                Console.WriteLine(">login or register?");

                string MsgText = Console.ReadLine();
                Data Initial;
                string Login;
                string Pass;

                switch (MsgText)
                {
                    case "login":
                        Console.WriteLine("login?");
                        Login = Console.ReadLine();
                        Console.WriteLine("pass?");
                        Pass = Console.ReadLine();
                        Initial = new Data(IDfromServer, "login", Login, Pass);
                        Initial.data.Add(MsgText);
                        MainSocket.Send(Initial.ToBytes());
                        LoopCondition = false;
                        break;
                    case "register":
                        Console.WriteLine("login?");
                        Login = Console.ReadLine();
                        Console.WriteLine("pass?");
                        Pass = Console.ReadLine();
                        Initial = new Data(IDfromServer, "register", Login, Pass);
                        Initial.data.Add(MsgText);
                        MainSocket.Send(Initial.ToBytes());
                        LoopCondition = false;
                        break;
                }
                if (LoopCondition == true)
                    Console.WriteLine("Please either log in or register");
            }
        }
    }
}
