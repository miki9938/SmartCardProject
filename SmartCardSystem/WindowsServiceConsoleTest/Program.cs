using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Subsembly.SmartCard;

namespace WindowsServiceConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CardTerminalManager.Singleton.CardInsertedEvent +=
                new CardTerminalEventHandler(InsertedEvent);
            CardTerminalManager.Singleton.CardTerminalLostEvent +=
                new CardTerminalEventHandler(TerminalLostEvent);
            CardTerminalManager.Singleton.CardTerminalFoundEvent +=
                new CardTerminalEventHandler(TerminalFoundEvent);

            StartupCardTerminalManager();
        }

        static void StartupCardTerminalManager()
        {
            CardTerminalManager.Singleton.Startup(true);

            if (CardTerminalManager.Singleton.SlotCount == 0)
            {
                Console.WriteLine("Brak czytnika");
            }
        }

        static public void InsertedEvent(object aSender, CardTerminalEventArgs aEventArgs)
        {
            AnalyzeCard(aEventArgs.Slot);
        }

        static public void AnalyzeCard(CardTerminalSlot aCardSlot)
        {
            string readerName = aCardSlot.CardTerminalName;

            CardActivationResult nActivationResult;
            Console.WriteLine("Reader Name: " + readerName + Environment.NewLine);
            CardHandle aCard = aCardSlot.AcquireCard((CardTypes.T0 | CardTypes.T1), out nActivationResult);
            if (nActivationResult != CardActivationResult.Success)
            {
                switch (nActivationResult)
                {
                    case CardActivationResult.NoCard:
                        Console.WriteLine(readerName + ": Please insert card ..." + Environment.NewLine);
                        break;
                    case CardActivationResult.UnresponsiveCard:
                        Console.WriteLine(readerName + ": Unresponsive card." + Environment.NewLine);
                        break;
                    case CardActivationResult.InUse:
                        Console.WriteLine(readerName + ": Card in use" + Environment.NewLine);
                        break;
                    default:
                        Console.WriteLine(readerName + ": Can't power up card!" + Environment.NewLine);
                        break;
                }
                return;
            }
            Console.WriteLine(aCardSlot.CardTerminalName + ": Found card" + Environment.NewLine);
            Console.WriteLine("Found card in reader " + aCardSlot.CardTerminalName + Environment.NewLine);

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
                Console.WriteLine("ATR: " + CardHex.FromByteArray(atr, 0, atr.Length) + Environment.NewLine);
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

                aCmdAPDU = new CardCommandAPDU(0x00, 0xB0, 0x00, 0x45, 128);

                aRespAPDU = aCard.SendCommand(aCmdAPDU);

                string odpowiedz = System.Text.Encoding.UTF8.GetString(aRespAPDU.GetData());
                Console.WriteLine(odpowiedz);
                Console.ReadKey();

            }
            catch (Exception x)
            {
                Console.WriteLine(x.Data + Environment.NewLine);
            }
            finally
            {
                aCard.Dispose();
            }
        }

        static public void TerminalFoundEvent(object aSender, CardTerminalEventArgs aEventArgs)
        {
            if (CardTerminalManager.Singleton.StartedUp)
            {
                Console.WriteLine("Found reader: " + aEventArgs.Slot.CardTerminalName + Environment.NewLine);
                Console.WriteLine("Insert card ..." + Environment.NewLine);
            }


        }

        static public void TerminalLostEvent(object aSender, CardTerminalEventArgs aEventArgs)
        {

            if (CardTerminalManager.Singleton.StartedUp)
            {
                Console.WriteLine("Lost reader: " + aEventArgs.Slot.CardTerminalName + Environment.NewLine);

                // CardTerminalManager.Singleton.Shutdown();
                // update number of readers
                CardTerminalManager.Singleton.DelistCardTerminal(aEventArgs.Slot.CardTerminal); // remove from monitored list of readers

                if (CardTerminalManager.Singleton.SlotCount == 0)
                {
                    Console.WriteLine("Connect reader ...");
                    // start looking for reader insertion
                    // done automatically by the singleton. The singleton raises a "new reader" event if it 
                    // finds a new reader.

                }
            }
        }
    }
}
