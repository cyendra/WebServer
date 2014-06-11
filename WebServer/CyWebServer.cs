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
    }
}
