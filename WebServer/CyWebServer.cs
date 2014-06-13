using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace WebServer
{
    class CyWebServer
    {
        private TcpListener tcpListener;
        private int port;
        public CyWebServer()
        {
            port = 8888;
            try
            {
                tcpListener = new TcpListener(GetLocalIp(), port);
                tcpListener.Start();
                Console.WriteLine("Web 服务器已启动...");
                Thread th = new Thread(new ThreadStart(startListen));
                th.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private IPAddress GetLocalIp()
        {
            string hostname;
            IPHostEntry localhost;
            IPAddress localaddr;
            hostname = Dns.GetHostName();
            localhost = Dns.GetHostEntry(hostname);
            localaddr = localhost.AddressList[0];
            return localaddr;
        }
        public void startListen()
        {
            Console.WriteLine("开始监听...");
            while (true)
            {
                Socket socket = tcpListener.AcceptSocket();
                Console.WriteLine("套接字类型：" + socket.SocketType);
                if (socket.Connected)
                {
                    Console.WriteLine("客户端已连接 IP: {0}", socket.RemoteEndPoint);
                    Byte[] bytesReceived = new Byte[1024];
                    int bytes = socket.Receive(bytesReceived, bytesReceived.Length, 0);
                    string inFromClient = Encoding.ASCII.GetString(bytesReceived);
                    Console.WriteLine(inFromClient);
                    if (inFromClient.Substring(0, 3) != "GET")
                    {
                        Console.WriteLine("只接受 GET 请求...");
                        socket.Close();
                        return;
                    }
                    int pos = inFromClient.IndexOf("HTTP", 1);
                    string httpVersion = inFromClient.Substring(pos, 8);
                    string request = inFromClient.Substring(0, pos-1);
                    request.Replace("\\","/");
                    if (request.IndexOf('.') < 0 && request.EndsWith("/"))
                    {
                        request = request + '/';
                    }
                    //string requestFile = request.Substring(request.LastIndexOf('/') + 1);
                    //string requestPath = request.Substring(request.IndexOf('/'), request.LastIndexOf('/'));
                    
                }
            }
        }
    }
}
