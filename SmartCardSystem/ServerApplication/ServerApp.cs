using System;
using System.Collections.Generic;
using System.IO;
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

              
              incomeMessage = Encoding.UTF8.GetString(message, 4, bytesRead);
              incomeMessageType = Encoding.UTF8.GetString(message, 0, 4);
              Console.WriteLine(incomeMessage);
              if (incomeMessageType == "WS1-")
              {
                  byte[] buffer = Encoding.UTF8.GetBytes("Serwer - OK");

                  clientStream.Write(buffer, 0, buffer.Length);
                  clientStream.Flush();

                  if (!File.Exists(configuration.dataFilePath))
                  {
                      using (StreamWriter sw = File.CreateText(configuration.dataFilePath))
                      {
                          sw.Write(incomeMessage + "\n");
                      }
                  }
                  else
                  {
                      using (StreamWriter sw = File.AppendText(configuration.dataFilePath))
                      {
                          sw.Write(incomeMessage + "\n");
                      }
                  }
              }
              else if (incomeMessageType == "CA1-")
              {
                  byte[] buffer = null;// = Encoding.UTF8.GetBytes("SR-OK" + System.IO.File.ReadAllText(configuration.dataFilePath));
                  Console.WriteLine(System.IO.File.ReadAllText(configuration.dataFilePath));
                  using (StreamReader sr = File.OpenText(configuration.dataFilePath))
                  {
                      string s = "";
                      while ((s = sr.ReadLine()) != null)
                      {
                          Console.WriteLine(s);
                          buffer = Encoding.UTF8.GetBytes("SR-OK" + s);
                      }
                  }

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
