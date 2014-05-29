using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ConsoleClientApplication
{
    class ClientApp
    {
        public bool sendData(string data)
        {
            TcpClient client = new TcpClient();

            client.Connect(new IPEndPoint(IPAddress.Parse(Configuration.ServerIP), Configuration.PortNo));

            NetworkStream clientStream = client.GetStream();

            Console.WriteLine("Connected? :" + client.Connected.ToString());
            
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes("WS1-"+data);

            clientStream.Write(buffer, 0, buffer.Length);

            clientStream.Flush();
            try
            {
                clientStream.ReadTimeout = 29000;

                byte[] message = new byte[Configuration.MaxMessageBytes];
                int bytesRead = 0;

                bytesRead = clientStream.Read(message, 0, Configuration.MaxMessageBytes);

                string msg = encoder.GetString(message, 0, bytesRead);

                if (msg == "Serwer - OK")
                    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exception: " + e);
                return false;  
            }
            
            return false;                         
        }

        static void Main(string[] args)
        {
            ClientApp ca = new ClientApp();

            bool b = ca.sendData("pierwsze dane");

            Console.WriteLine(b);
            Console.ReadKey();            
        }
    }
}
