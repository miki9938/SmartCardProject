using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerApplication
{
    public delegate void PacketRecievedDelegate(object packet, object session);

    public delegate void ConnecetedServerDelegate(bool isConnected);

    class ServerApp
    {
        private Configuration configuration;

        ServerApp()
        {
            configuration = new Configuration();
        }

        private void serverStartListening()
        {
            var tcpListener = new TcpListener(IPAddress.Parse(configuration.ServerIP), configuration.PortNo);
            tcpListener.Start();
            Console.WriteLine("The local End point is  :" + tcpListener.LocalEndpoint);

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(serverClientCommunicationHandler));
                clientThread.Start(client);
                
                //Socket socket = tcpListener.AcceptSocket();
                //Console.WriteLine("Connection accepted from " + socket.RemoteEndPoint);

                //byte[] b = new byte[100];
                //int k = socket.Receive(b);
                //Console.WriteLine("Recieved...");
                //for (int i = 0; i < k; i++)
                //    Console.Write(Convert.ToChar(b[i]));

                //ASCIIEncoding asen = new ASCIIEncoding();
                //socket.Send(asen.GetBytes("The string was recieved by the server."));
                //Console.WriteLine("\nSent Acknowledgement");

                //socket.Close();
            }
        }

        private void serverClientCommunicationHandler(object client)
        {
            var tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            string incomeMessage;
            string incomeMessageType;

            //var configuration = ((Configuration) _Configuration);
            //var session = new ClientSession((Configuration)_Configuration);
            //session.NetworkStream = tcpClient.GetStream();
            //session.IsConnected = true;

          while (true)
          {
            var message = new byte[Configuration.MaxMessageBytes];
            int bytesRead = 0;

            try
            {
                //bytesRead = session.NetworkStream.Read(message, 0, Configuration.MaxMessageBytes);
                bytesRead = clientStream.Read(message, 0, Configuration.MaxMessageBytes);
            }
            catch
            {
                //session.IsConnected = false;
                break;
            }

            if (bytesRead == 0)
            {
                //session.IsConnected = false;
                break;
            }

            //var messages = ByteArray.SplitByteArray(message, Encoding.ASCII.GetBytes(configuration.PacketEndMarker), false);
            //messages.Remove(messages.Last());
            //foreach (var _message in messages)
            //{
            //    object packet = RawPacket.RawPacketToBuffer(_message, configuration);
            //    Console.WriteLine("odebrano: " + packet.ToString() + " od: " + session.IMEI);
            //    if (packet != null)
            //        if ((bool)packet.GetType().GetMethod("IsPacketComplete").Invoke(packet, null))
            //        {
            //            PacketReceivedDelegate packetReceivdHandler = PacketReceivedHandlers[
            //                RawPacket.GetPacketType(
            //                    (byte[])packet.GetType().GetProperty("RawPacket").GetValue(packet))];
            //            if (packetReceivdHandler != null)
            //            {
            //                packetReceivdHandler(
            //                    packet.GetType().GetMethod("GetPacketDataObject").Invoke(packet, null), session);
            //            }
            //        }
            //}
            //session.NetworkStream.FlushAsync();

              
              ASCIIEncoding encoder = new ASCIIEncoding();
              incomeMessage = encoder.GetString(message, 3, bytesRead);
              incomeMessageType = encoder.GetString(message, 0, 4);
              Console.WriteLine(incomeMessage);
              if (incomeMessageType == "WS1-")
              {
                  byte[] buffer = encoder.GetBytes("Serwer - OK");

                  clientStream.Write(buffer, 0, buffer.Length);
                  clientStream.Flush();
              }
              else if (incomeMessageType == "CA1-")
              {
                  byte[] buffer = encoder.GetBytes("Serwer - lista");

                  clientStream.Write(buffer, 0, buffer.Length);
                  clientStream.Flush();
              }
          }
        }

        public object _Configuration { get; set; }


        static void Main(string[] args)
        {
            ServerApp sa = new ServerApp();

            sa.serverStartListening();

            Console.ReadKey();
        }
    }
}
