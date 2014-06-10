using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

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
            int bytes = 0;
            string inFromClient = "";
            do
            {
                bytes = socket.Receive(bytesReceived, bytesReceived.Length, 0);
                inFromClient = inFromClient + Encoding.ASCII.GetString(bytesReceived, 0, bytes);
            }
            while (bytes > 0);
            Console.WriteLine(inFromClient);
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
        public void start()
        {
            try
            {
                TcpListener server = new TcpListener(local, port);
            }
            catch
            {
                Console.WriteLine("W_W");
                return;
            }
            
        }
        
    }
    
    class Program
    {
        static void Main(string[] args)
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
    }
}
