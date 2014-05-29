using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    public class Configuration
    {
        public Configuration()
        {
            ServerIP = "192.168.1.2";
            PortNo = 8001;
            MaxMessageBytes = 9216;
            PacketEndSign = "--SCSPE|";
            dataFilePath = @"D:\serverData";
        }

        public  string ServerIP { get; set; }

        public  int PortNo { get; set; }

        public static int MaxMessageBytes { get; set; }

        public string PacketEndSign { get; set; }

        public string PacketSplitSign { get; set; }

        public string dataFilePath { get; set; }
    }
}
