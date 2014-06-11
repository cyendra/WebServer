using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WebServer
{
    class MySocket
    {
        public Socket ConnectSocket(string server, int port)
        {
            Socket s = null;
            IPHostEntry hostEntry = null;
            hostEntry = Dns.GetHostEntry(server);
            foreach(IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                tempSocket.Connect(ipe);
                if (tempSocket.Connected)
                {
                    s = tempSocket;
                    break;
                }
                else
                {
                    continue;
                }
            }
            return s;
        }
        public string SocketSendReceive(string server, int port)
        {
            string request = "GET / HTTP/1.1\r\nHost:" + server + "\r\nConnection:Close\r\n\r\n";
            Byte[] bytesSent = Encoding.ASCII.GetBytes(request);
            Byte[] bytesReceived = new Byte[256];
            // 创建一个套接字
            Socket s = ConnectSocket(server, port);
            if (s == null) return ("Connection failed");
            s.Send(bytesSent, bytesSent.Length, 0);
            int bytes = 0;
            string page = "Default HTML page on " + server + ":\r\n";
            do 
            {
                bytes = s.Receive(bytesReceived, bytesReceived.Length, 0);
                page = page + Encoding.ASCII.GetString(bytesReceived, 0, bytes);
            }
            while (bytes > 0);
            return page;
        }
    }
    class MyServerConnection
    {
        public MyServerConnection(Socket socket)
        {
            this.socket = socket;
        }
        Socket socket;
        Byte[] bytesSent;
        Byte[] bytesReceived = new Byte[1024];
        public void run()
        {
            Console.WriteLine("在线程中进行通信...");
            while (true)
            {
                int bytes = 0;
                string inFromClient = "";
                Console.WriteLine("准备接受客户端的信息...");
                do
                {
                    //Console.WriteLine("socket.Available = " + socket.Available);
                    bytes = socket.Receive(bytesReceived, bytesReceived.Length, 0);
                    inFromClient = inFromClient + Encoding.ASCII.GetString(bytesReceived, 0, bytes);
                    //Console.WriteLine("收到 " + bytes + "字节");
                    //Console.WriteLine("socket.Available = " + socket.Available);
                }
                while (socket.Available > 0);
                string ops = "Server get messege: " + inFromClient;
                Console.WriteLine("收到来自客户端的信息：");
                Console.WriteLine(inFromClient);
                bytesSent = Encoding.ASCII.GetBytes(ops);
                socket.Send(bytesSent, bytesSent.Length, 0);
                if (inFromClient.Equals("exit"))
                {
                    Console.WriteLine("客户端准备关闭连接...");
                    break;
                }
            }
            socket.Close();
            Console.WriteLine("连接已关闭...");
        }
    }
    class MyServer
    {
        public MyServer()
        {
            port = 3721;
            local = GetLocalIp();
        }
        int port;
        IPAddress local;
        private IPAddress GetLocalIp()
        {
            string hostname;
            IPHostEntry localhost;
            IPAddress localaddr;
            hostname = System.Net.Dns.GetHostName();
            localhost = System.Net.Dns.GetHostEntry(hostname);
            localaddr = localhost.AddressList[0];
            return localaddr;
        }
        public void Start()
        {
            try
            {
                TcpListener server = new TcpListener(local, port);
                Console.WriteLine("准备启动服务器...");
                Console.WriteLine("本机IP：" + local.ToString() + " 监听端口号：" + port);
                server.Start();
                Console.WriteLine("服务器启动...");
                while (true)
                {
                    Socket socket = server.AcceptSocket();
                    Console.WriteLine("收到一个连接...");
                    MyServerConnection conn = new MyServerConnection(socket);
                    Thread thd = new Thread(new ThreadStart(conn.run));
                    thd.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("W_W:" + e.ToString());
                return;
            }
        }
    }
    class MyClient
    {
        public MyClient()
        {
            //port = 2567;
            port = 3721;
            local = GetLocalIp();
        }
        int port;
        IPAddress local;
        Socket socket;
        string sentStr;
        string receivedStr;
        Byte[] bytesSent;
        Byte[] bytesReceived = new Byte[1024];
        private IPAddress GetLocalIp()
        {
            string hostname;
            IPHostEntry localhost;
            IPAddress localaddr;
            hostname = System.Net.Dns.GetHostName();
            localhost = System.Net.Dns.GetHostEntry(hostname);
            localaddr = localhost.AddressList[0];
            return localaddr;
        }
        public void Start()
        {
            Console.WriteLine("准备开始连接服务器...");
            Console.WriteLine("服务器IP：" + local.ToString() + " 端口号：" + port);
            IPEndPoint ipe = new IPEndPoint(local, port);
            try
            {
                socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipe);
            }
            catch (Exception e)
            {
                Console.WriteLine("连接失败！");
                Console.WriteLine(e.Message);
                return;
            }
            Console.WriteLine("服务器已连接...");
            while (true)
            {
                Console.WriteLine("请输入要发送的信息：");
                sentStr = Console.ReadLine();
                bytesSent = Encoding.ASCII.GetBytes(sentStr);
                Console.WriteLine("准备发送 信息：" + sentStr + " 到服务器...");
                socket.Send(bytesSent, bytesSent.Length, 0);
                Console.WriteLine("成功了！终于发出去了！");
                int bytes = 0;
                receivedStr = "";
                Console.WriteLine("准备接受服务器的信息...");
                do
                {
                    bytes = socket.Receive(bytesReceived, bytesReceived.Length, 0);
                    receivedStr = receivedStr + Encoding.ASCII.GetString(bytesReceived, 0, bytes);
                    Console.WriteLine("收到 " + bytes + "字节");
                } while (socket.Available > 0);
                Console.WriteLine("收到服务器发来的信息：");
                Console.WriteLine(receivedStr);
                if (sentStr.Equals("exit"))
                {
                    Console.WriteLine("准备关闭客户端...");
                    break;
                }
            }
            socket.Close();
            Console.WriteLine("客户端已关闭...");
        }
    }
    class Program
    {
        public static void useTest()
        {
            MySocket my = new MySocket();
            string host;
            int port = 80;
            Console.WriteLine("Hello World!");
            host = Console.ReadLine();
            if (host.Length == 0) host = Dns.GetHostName();
            string result = my.SocketSendReceive(host, port);
            Console.WriteLine(result);
            Console.ReadLine();
        }
        public static void useServer()
        {
            MyServer server = new MyServer();
            server.Start();
        }
        public static void useClient()
        {
            MyClient client = new MyClient();
            client.Start();
        }
        static void Main(string[] args)
        {
            string cmd;
            Console.WriteLine("0-Test, 1-Server, 2-Client");
            cmd = Console.ReadLine();
            if (cmd.Equals("1"))
            {
                useServer();
            }
            else if (cmd.Equals("2"))
            {
                useClient();
            }
            else if (cmd.Equals("0"))
            {
                useTest();
            }
        }
    }
}
