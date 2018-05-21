using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace HTTP_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            string ipAddress = "127.0.0.1";
            int port = 8888;

            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipendpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            listenerSocket.Bind(ipendpoint);

            while (true)
            {
                listenerSocket.Listen(0);
                Socket clientSocket = listenerSocket.Accept();

                Thread clientThread = new Thread(() => ClientConnection(clientSocket));
                clientThread.Start();
            }
        }

        private static void ClientConnection(Socket clientSocket)
        {
            string statusLine;
            string responseHeader;
            string responseBody;

            byte[] buffer = new byte[clientSocket.SendBufferSize];

            int readByte;

            readByte = clientSocket.Receive(buffer);
            byte[] rData = new byte[readByte];
            Array.Copy(buffer, rData, readByte);
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(rData));

            string page = "";

            try
            {
                string indexPage = ConfigurationManager.AppSettings.Get("IndexPage");
                page = System.IO.File.ReadAllText(indexPage);
            }
            catch (Exception e)
            {
                statusLine = "HTTP/1.1 404 Not Found\r\n";
                responseHeader = "Content-Type: text/html\r\n";
                responseBody = "<html><head><title>Error</title></head><body><div>Page not found</div></body></html>";

                SendResponse(clientSocket, statusLine, responseHeader, responseBody);
                clientSocket.Close();
                return;
            }

            statusLine = "HTTP/1.1 200 OK\r\n";
            responseHeader = "Content-Type: text/html\r\n";
            responseBody = page;

            SendResponse(clientSocket, statusLine, responseHeader, responseBody);
            clientSocket.Close();

            Console.WriteLine("disconnected");
            Console.ReadKey();
        }

        public static void SendResponse(Socket clientSocket, string statusLine, string responseHeader, string responseBody)
        {
            clientSocket.Send(Encoding.UTF8.GetBytes(statusLine));
            clientSocket.Send(Encoding.UTF8.GetBytes(responseHeader));
            clientSocket.Send(Encoding.UTF8.GetBytes("\r\n"));
            clientSocket.Send(Encoding.UTF8.GetBytes(responseBody));
        }
    }
}
