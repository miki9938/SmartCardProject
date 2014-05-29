using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerApplication
{
    class ServerSession
    {
        protected bool isConnected;
        private bool isReady;

        protected Configuration _Configuration;
        

        public ServerSession(Configuration configuration)
        {
            OutgoingBuffer = new PacketList<object>();

            OutgoingBuffer.OnAdded += outgoingBufferEventHandler;

            _Configuration = configuration;
        }

        public ConnecetedServerDelegate ConnectedToServer;

        public PacketList<object> OutgoingBuffer { get; set; }

        public NetworkStream NetworkStream { get; set; }

        public virtual bool IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;

                if (ConnectedToServer != null)
                    ConnectedToServer(value);

                if(value)
                    SendOutBuffer();
                else
                    IsReady = false;
            }
        }

        public bool IsReady
        {
            get
            {
                return isReady;
            }
            set
            {
                isReady = value;
                if (value)
                    SendOutBuffer();
            }
        }


        public bool IsSending { get; set; }

        /// <summary>
        ///     Obsługa kolejki wyjściowej
        /// </summary>
        private void outgoingBufferEventHandler(object sender, EventArgs eventArgs)
        {
            SendOutBuffer();
        }

        private void outgoingBufferProceed()
        {
            if (IsConnected && IsReady && !IsSending)
            {
                IsSending = true;
                lock (((ICollection)OutgoingBuffer).SyncRoot)
                {
                    int packetsLengthCount = 0;
                    while (OutgoingBuffer.Any())
                    {
                        object packet = OutgoingBuffer.First();
                        var data = (byte[])packet.GetType().GetProperty("RawPacket").GetValue(packet);
                        try
                        {
                            packetsLengthCount += data.Length;
                            if (packetsLengthCount > Configuration.MaxMessageBytes)
                            {
                                NetworkStream.Flush();
                                packetsLengthCount = data.Length;
                            }
                            NetworkStream.Write(data, 0, data.Length);
                            OutgoingBuffer.Remove(packet);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                    }
                }
                IsSending = false;
            }
        }

        /// <summary>
        ///  Wysłanie danych z kolejki wyjściowej
        /// </summary>
        public void SendOutBuffer()
        {
            var sendingThread = new Thread(outgoingBufferProceed);
            sendingThread.Start();
        }

        public class PacketList<T> : List<T>
        {
            public event EventHandler OnAdded;

            public void Add(T item)
            {
                base.Add(item);
                if (null != OnAdded)
                {
                    OnAdded(this, null);
                }
            }
        }
    }
}
