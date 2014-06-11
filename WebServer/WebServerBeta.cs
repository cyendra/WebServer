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
    class WebServerBeta
    {
    }
    class MyWebServer
    {
        private TcpListener myListener;
        private int port = 5050; // 可以任意选择空闲的端口
        //生成TcpListener的构建器开始监听给定的端口，它还启动调用StartListen()方法的一个线程
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
        public MyWebServer()
        {
            try
            {
                //开始监听给定的端口
                myListener = new TcpListener(GetLocalIp(),port);
                myListener.Start();
                Console.WriteLine("Web Server Running... Press ^C to Stop...");
                //启动调用StartListen方法的线程
                Thread th = new Thread(new ThreadStart(StartListen));
                th.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred while Listening :" + e.ToString());
            }
        }
        public string GetTheDefaultFileName(string sLocalDirectory)
        {
            StreamReader sr;
            String sLine = "";
            try
            {
                //打开default.dat，获得缺省清单
                sr = new StreamReader("data\\Default.Dat");
                while ((sLine = sr.ReadLine()) != null)
                {
                    //在web服务器的根目录下查找缺少文件
                    if (File.Exists(sLocalDirectory + sLine) == true)
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred : " + e.ToString());
            }
            if (File.Exists(sLocalDirectory + sLine) == true)
                return sLine;
            else
                return "";
        }
        public string GetLocalPath(string sMyWebServerRoot, string sDirName)
        {
            StreamReader sr;
            String sLine = "";
            String sVirtualDir = "";
            String sRealDir = "";
            int iStartPos = 0;
            //删除多余的空格
            sDirName.Trim();
            // 转换成小写
            sMyWebServerRoot = sMyWebServerRoot.ToLower();
            // 转换成小写
            sDirName = sDirName.ToLower();
            try
            {
                //打开Vdirs.dat文件，获得虚拟目录
                sr = new StreamReader("data\\VDirs.Dat");
                while ((sLine = sr.ReadLine()) != null)
                {
                    //删除多余的空格
                    sLine.Trim();
                    if (sLine.Length > 0)
                    {
                        //找到分割符
                        iStartPos = sLine.IndexOf(";");
                        // 转换成小写
                        sLine = sLine.ToLower();
                        sVirtualDir = sLine.Substring(0, iStartPos);
                        sRealDir = sLine.Substring(iStartPos + 1);
                        if (sVirtualDir == sDirName)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred : " + e.ToString());
            }
            if (sVirtualDir == sDirName)
                return sRealDir;
            else
                return "";
        }

        public string GetMimeType(string sRequestedFile)
        {
            StreamReader sr;
            String sLine = "";
            String sMimeType = "";
            String sFileExt = "";
            String sMimeExt = "";
            // 转换成小写
            sRequestedFile = sRequestedFile.ToLower();
            int iStartPos = sRequestedFile.IndexOf(".");
            sFileExt = sRequestedFile.Substring(iStartPos);
            try
            {
                //打开Vdirs.dat文件，获得虚拟目录
                sr = new StreamReader("data\\Mime.Dat");
                while ((sLine = sr.ReadLine()) != null)
                {
                    sLine.Trim();
                    if (sLine.Length > 0)
                    {
                        //找到分割符
                        iStartPos = sLine.IndexOf(";");
                        // 转换成小写
                        sLine = sLine.ToLower();
                        sMimeExt = sLine.Substring(0, iStartPos);
                        sMimeType = sLine.Substring(iStartPos + 1);
                        if (sMimeExt == sFileExt)
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred : " + e.ToString());
            }
            if (sMimeExt == sFileExt)
                return sMimeType;
            else
                return "";
        }

        public void SendHeader(string sHttpVersion,
          string sMIMEHeader,
          int iTotBytes,
          string sStatusCode,
          ref Socket mySocket)
        {
            String sBuffer = "";
            //如果用户没有提供Mime类型，则将其缺省地设置为text/html
            if (sMIMEHeader.Length == 0)
            {
                sMIMEHeader = "text/html"; // Default Mime Type is text/html
            }
            sBuffer = sBuffer + sHttpVersion + sStatusCode + "\r\n";
            sBuffer = sBuffer + "Server: cx1193719-b\r\n";
            sBuffer = sBuffer + "Content-Type: " + sMIMEHeader + "\r\n";
            sBuffer = sBuffer + "Accept-Ranges: bytes\r\n";
            sBuffer = sBuffer + "Content-Length: " + iTotBytes + "\r\n\r\n";
            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);
            SendToBrowser(bSendData, ref mySocket);
            Console.WriteLine("Total Bytes : " + iTotBytes.ToString());
        }

        public void SendToBrowser(String sData, ref Socket mySocket)
        {
            SendToBrowser(Encoding.ASCII.GetBytes(sData), ref mySocket);
        }
        public void SendToBrowser(Byte[] bSendData, ref Socket mySocket)
        {
            int numBytes = 0;
            try
            {
                if (mySocket.Connected)
                {
                    if ((numBytes = mySocket.Send(bSendData, bSendData.Length, 0)) == -1)
                        Console.WriteLine("Socket Error cannot Send Packet");
                    else
                    {
                        Console.WriteLine("No. of bytes send {0}", numBytes);
                    }
                }
                else
                    Console.WriteLine("Connection Dropped....");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Occurred : {0} ", e);
            }
        }

        public void StartListen()
        {
            int iStartPos = 0;
            String sRequest;
            String sDirName;
            String sRequestedFile;
            String sErrorMessage;
            String sLocalDir;
            String sMyWebServerRoot = "C:\\MyWebServerRoot\\";
            String sPhysicalFilePath = "";
            String sFormattedMessage = "";
            String sResponse = "";
            while (true)
            {
                //接受一个新的连接
                Socket mySocket = myListener.AcceptSocket();
                Console.WriteLine("Socket Type " + mySocket.SocketType);
                if (mySocket.Connected)
                {
                    Console.WriteLine("\nClient Connected!!\n==================\nCLient IP {0}\n", mySocket.RemoteEndPoint);
                    //生成一个字节数组，从客户端接收数据
                    Byte[] bReceive = new Byte[1024];
                    int i = mySocket.Receive(bReceive, bReceive.Length, 0);
                    //将字节型数据转换为字符串
                    string sBuffer = Encoding.ASCII.GetString(bReceive);
                    //上前我们将只处理GET类型
                    if (sBuffer.Substring(0, 3) != "GET")
                    {
                        Console.WriteLine("Only Get Method is supported..");
                        mySocket.Close();
                        return;
                    }
                    // 查找HTTP请求
                    iStartPos = sBuffer.IndexOf("HTTP", 1);
                    // 获取“HTTP”文本和版本号，例如，它会返回“HTTP/1.1”
                    string sHttpVersion = sBuffer.Substring(iStartPos, 8);
                    //解析请求的类型和目录/文件
                    sRequest = sBuffer.Substring(0, iStartPos - 1);
                    //如果存在\符号，则使用/替换
                    sRequest.Replace("\\", "/");
                    //如果提供的文件名中没有/，表明这是一个目录，我们解危需要查找缺省的文件名
                    if ((sRequest.IndexOf(".") < 1) && (!sRequest.EndsWith("/")))
                    {
                        sRequest = sRequest + "/";
                    }
                    //解析请求的文件名
                    iStartPos = sRequest.LastIndexOf("/") + 1;
                    sRequestedFile = sRequest.Substring(iStartPos);
                    //解析目录名
                    sDirName = sRequest.Substring(sRequest.IndexOf("/"), sRequest.LastIndexOf("/") - 3);

                    // 确定物理目录
                    if (sDirName == "/")
                        sLocalDir = sMyWebServerRoot;
                    else
                    {
                        //获得虚拟目录
                        sLocalDir = GetLocalPath(sMyWebServerRoot, sDirName);
                    }
                    Console.WriteLine("Directory Requested : " + sLocalDir);
                    //如果物理目录不存在，则显示出错信息
                    if (sLocalDir.Length == 0)
                    {
                        sErrorMessage = "〈H2〉Error!! Requested Directory does not exists〈/H2〉〈Br〉";
                        //sErrorMessage = sErrorMessage + "Please check data\\Vdirs.Dat";
                        //对信息进行格式化
                        SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found", ref mySocket);
                        //向浏览器发送信息
                        SendToBrowser(sErrorMessage, ref mySocket);
                        mySocket.Close();
                        continue;
                    }
                    //如果文件名不存在，则查找缺省文件列表
                    if (sRequestedFile.Length == 0)
                    {
                        // 获取缺省的文件名
                        sRequestedFile = GetTheDefaultFileName(sLocalDir);
                        if (sRequestedFile == "")
                        {
                            sErrorMessage = "〈H2〉Error!! No Default File Name Specified〈/H2〉";
                            SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found",
                            ref mySocket);
                            SendToBrowser(sErrorMessage, ref mySocket);
                            mySocket.Close();
                            return;
                        }
                    }
                    String sMimeType = GetMimeType(sRequestedFile);
                    //构建物理路径
                    sPhysicalFilePath = sLocalDir + sRequestedFile;
                    Console.WriteLine("File Requested : " + sPhysicalFilePath);

                    if (File.Exists(sPhysicalFilePath) == false)
                    {
                        sErrorMessage = "〈H2〉404 Error! File Does Not Exists...〈/H2〉";
                        SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found", ref mySocket);
                        SendToBrowser(sErrorMessage, ref mySocket);
                        Console.WriteLine(sFormattedMessage);
                    }
                    else
                    {
                        int iTotBytes = 0;
                        sResponse = "";
                        FileStream fs = new FileStream(sPhysicalFilePath, FileMode.Open, FileAccess.Read,
                        FileShare.Read);
                        // 创建一个能够从FileStream中读取字节数据的reader
                        BinaryReader reader = new BinaryReader(fs);
                        byte[] bytes = new byte[fs.Length];
                        int read;
                        while ((read = reader.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            // 从文件中读取数据，并将数据发送到网络上
                            sResponse = sResponse + Encoding.ASCII.GetString(bytes, 0, read);
                            iTotBytes = iTotBytes + read;
                        }
                        reader.Close();
                        fs.Close();
                        SendHeader(sHttpVersion, sMimeType, iTotBytes, " 200 OK", ref mySocket);
                        SendToBrowser(bytes, ref mySocket);
                        //mySocket.Send(bytes, bytes.Length,0);
                    }
                    mySocket.Close();
                }
            }
        }
    }
}

