using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Subsembly.SmartCard;
using System.Text.RegularExpressions;

namespace WindowsServiceTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            CardTerminalManager.Singleton.CardInsertedEvent +=
                new CardTerminalEventHandler(InsertedEvent);
            CardTerminalManager.Singleton.CardRemovedEvent +=
                new CardTerminalEventHandler(RemovedEvent);
            CardTerminalManager.Singleton.CardTerminalLostEvent +=
                new CardTerminalEventHandler(TerminalLostEvent);
            CardTerminalManager.Singleton.CardTerminalFoundEvent +=
                new CardTerminalEventHandler(TerminalFoundEvent);

            StartupCardTerminalManager();
        }

        void StartupCardTerminalManager()
        {
            try
            {
                CardTerminalManager.Singleton.Startup(true);

                if (CardTerminalManager.Singleton.SlotCount == 0)
                {
                    MessageBox.Show("No reader available",
                    "WARNING: Singleton Startup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop,
                    MessageBoxDefaultButton.Button1);
                }
            }
            catch (Exception x)
            {

                MessageBox.Show(
                    "Unable to run CardTerminalConfigurator. Will " +
                    "exit this application.",
                    "SmartCardAPI sample application",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop,
                    MessageBoxDefaultButton.Button1);
            }
        }

        public void InsertedEvent(object aSender, CardTerminalEventArgs aEventArgs)
        {
            if (base.InvokeRequired)
            {
                object[] vParms = new object[2];
                vParms[0] = aSender;
                vParms[1] = aEventArgs;
                base.BeginInvoke(new CardTerminalEventHandler(InsertedEvent),
                    vParms);
            }
            else
            {
                // 01APR2011
                // We catch any exceptions during card I/O. This is particularly important
                // for fuzzy communication conditions. Example: a contactless card that 
                // is not in the field throughout the whole I/O might cause an error within 
                // the unmanaged Windows API code. SmartCardAPI catches this in a general 
                // exception.
                try
                {
                    this.AnalyzeCard(aEventArgs.Slot);
                }
                catch
                {
                    //DisplayText("last read failed");
                }
            }
        }

        public void RemovedEvent(object aSender, CardTerminalEventArgs aEventArgs)
        {
            if (base.InvokeRequired)
            {
                object[] vParms = new object[2];
                vParms[0] = aSender;
                vParms[1] = aEventArgs;
                base.BeginInvoke(new CardTerminalEventHandler(RemovedEvent),
                    vParms);
            }
            else
            {
                //this.PromptAnyCard(aEventArgs.Slot.CardTerminalName);
            }
        }

        public void AnalyzeCard(CardTerminalSlot aCardSlot)
        {
            // Acquire any processor card (T=0 or T=1) that may be present in the given card
            // terminal slot
            string readerName = aCardSlot.CardTerminalName;

            CardActivationResult nActivationResult;
            textBox1.Text += "Reader Name: " + readerName + Environment.NewLine;

            //aCardSlot.CardTerminal.Config
            CardHandle aCard = aCardSlot.AcquireCard((CardTypes.T0 | CardTypes.T1), out nActivationResult);
            if (nActivationResult != CardActivationResult.Success)
            {
                switch (nActivationResult)
                {
                    case CardActivationResult.NoCard:
                        textBox1.Text += readerName + ": Please insert card ..." + Environment.NewLine;
                        break;
                    case CardActivationResult.UnresponsiveCard:
                        textBox1.Text += readerName + ": Unresponsive card." + Environment.NewLine;
                        break;
                    case CardActivationResult.InUse:
                        textBox1.Text += readerName + ": Card in use" + Environment.NewLine;
                        break;
                    default:
                        textBox1.Text += readerName + ": Can't power up card!" + Environment.NewLine;
                        break;
                }
                return;
            }
            textBox1.Text += aCardSlot.CardTerminalName + ": Found card" + Environment.NewLine;
            textBox1.Text += "Found card in reader " + aCardSlot.CardTerminalName + Environment.NewLine;

            aCardSlot.BeginTransaction();
            try

            // We are doing a few things here that any card system should support.
            // Note that the CardHandle represents the combination of card terminal and 
            // powered-up card.
            {
                // =========================== ATR DETECTION ======================================
                // Every card accessed through PC/SC must return an Answer To Reset (ATR). 
                // So let's see what we've got here.
                byte[] atr = aCard.GetATR();
                if (atr.Length == 0) throw new Exception("Invalid ATR");
                textBox1.Text += "ATR: " + CardHex.FromByteArray(atr, 0, atr.Length) + Environment.NewLine;
                // ================================================================================

                byte[] asd = new byte[7];
                asd[0] = 0xD6;
                asd[1] = 0x16;
                asd[2] = 0x00;
                asd[3] = 0x00;
                asd[4] = 0x30;
                asd[5] = 0x01;
                asd[6] = 0x01;

                byte[] data = new Byte[7];

                CardCommandAPDU aCmdAPDU = new CardCommandAPDU(0x00, 0xA4, 0x04, 0x00, asd, 0);

                CardResponseAPDU aRespAPDU = aCard.SendCommand(aCmdAPDU);

                aCmdAPDU = new CardCommandAPDU(0x00, 0xA4, 0x00, 0x00, new byte[2] { 0x00, 0x02 });

                aRespAPDU = aCard.SendCommand(aCmdAPDU);

                aCmdAPDU = new CardCommandAPDU(0x00, 0xB0, 0x00, 0x6D, 36);

                aRespAPDU = aCard.SendCommand(aCmdAPDU);

                string odpowiedz = System.Text.Encoding.UTF8.GetString(aRespAPDU.GetData());
                string odpConvert = "";

                for (int i = 0; i < odpowiedz.Length; i++)
                {
                    if (check(odpowiedz[i]).Equals(false))
                        odpConvert += " ";
                    else
                        odpConvert += odpowiedz[i];
                }
                

                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex(@"[ ]{2,}", options);
                odpConvert = regex.Replace(odpConvert, @" ");
                odpConvert = odpConvert.Trim();

                string[] studentData = odpConvert.Split(' ');

                /*
                int a = convert("ą");
                int c = convert("ć");
                int e = convert("ę");
                int l = convert("ł");
                int n = convert("ń");
                int o = convert("ó");
                int s = convert("ś");
                int rz = convert("ż");
                int zi = convert("ź");
                */


                string dataToSend = studentData[0] + " " + studentData[1];

                textBox1.Text = dataToSend;

                sendData(dataToSend);


            }
            catch (Exception x)
            {
                textBox1.Text += x.Data + Environment.NewLine;
            }
            finally
            {
                aCard.Dispose();
            }
        }

        static public bool sendData(string data)
        {
            TcpClient client = new TcpClient();

            client.Connect(new IPEndPoint(IPAddress.Parse(Configuration.ServerIP), Configuration.PortNo));

            NetworkStream clientStream = client.GetStream();

            Console.WriteLine("Connected? :" + client.Connected.ToString());

            byte[] buffer = Encoding.UTF8.GetBytes("WS1-" + data);

            clientStream.Write(buffer, 0, buffer.Length);

            clientStream.Flush();
            try
            {
                clientStream.ReadTimeout = 29000;

                byte[] message = new byte[Configuration.MaxMessageBytes];
                int bytesRead = 0;

                bytesRead = clientStream.Read(message, 0, Configuration.MaxMessageBytes);

                string msg = Encoding.UTF8.GetString(message, 0, bytesRead);

                if (msg == "Serwer - OK")
                    return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("exception: " + e);
                return false;
            }

            return false;
        }

        public void TerminalFoundEvent(object aSender, CardTerminalEventArgs aEventArgs)
        {
            if (base.InvokeRequired)
            {
                object[] vParms = new object[2];
                vParms[0] = aSender;
                vParms[1] = aEventArgs;
                base.BeginInvoke(new CardTerminalEventHandler(TerminalFoundEvent), vParms);
            }
            else
            {
                if (CardTerminalManager.Singleton.StartedUp)
                {
                    textBox1.Text += "Found reader: " + aEventArgs.Slot.CardTerminalName + Environment.NewLine;
                    textBox1.Text += "Insert card ..." + Environment.NewLine;
                }
            }

        }

        public void TerminalLostEvent(object aSender, CardTerminalEventArgs aEventArgs)
        {
            if (base.InvokeRequired)
            {
                object[] vParms = new object[2];
                vParms[0] = aSender;
                vParms[1] = aEventArgs;
                base.BeginInvoke(new CardTerminalEventHandler(TerminalLostEvent), vParms);
            }
            else
            {
                if (CardTerminalManager.Singleton.StartedUp)
                {
                    textBox1.Text += "Lost reader: " + aEventArgs.Slot.CardTerminalName + Environment.NewLine;

                    // CardTerminalManager.Singleton.Shutdown();
                    // update number of readers
                    CardTerminalManager.Singleton.DelistCardTerminal(aEventArgs.Slot.CardTerminal); // remove from monitored list of readers

                    if (CardTerminalManager.Singleton.SlotCount == 0)
                    {
                        textBox1.Text = "Connect reader ...";
                        // start looking for reader insertion
                        // done automatically by the singleton. The singleton raises a "new reader" event if it 
                        // finds a new reader.

                    }
                }
            }
        }

        public int convert(string sign)
        {
            byte[] a = System.Text.Encoding.UTF8.GetBytes(sign);
            return System.Text.Encoding.UTF8.GetString(a)[0];
        }
        
        public bool check(char sign)
        {
            int[] signTab = new int[10] { 45, 261, 263, 281, 322, 324, 243, 347, 380, 378 };

            foreach (int a in signTab)
            {
                if (sign == a)
                    return true;
            }

            if (sign > 64 && sign < 91)
                return true;

            if (sign > 96 && sign < 123)
                return true;

            //if (sign > 47 && sign < 58)
                //return true;

            return false;
        }
        
    }
}
