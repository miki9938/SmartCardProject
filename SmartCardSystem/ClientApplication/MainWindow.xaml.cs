using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void downloadClick(object sender, RoutedEventArgs e)
        {
            requestData();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public void requestData()
        {
            TcpClient client = new TcpClient();

            client.Connect(new IPEndPoint(IPAddress.Parse(Configuration.ServerIP), Configuration.PortNo));

            NetworkStream clientStream = client.GetStream();

            if (client.Connected == true)
                TextBox1.Text = "Połączono z serwerem";

            byte[] buffer = Encoding.UTF8.GetBytes("CA1-request");

            clientStream.Write(buffer, 0, buffer.Length);

            clientStream.Flush();
            try
            {
                clientStream.ReadTimeout = 29000;

                byte[] message = new byte[Configuration.MaxMessageBytes];
                int bytesRead = 0;

                bytesRead = clientStream.Read(message, 0, Configuration.MaxMessageBytes);

                string msgRaw = Encoding.UTF8.GetString(message, 5, bytesRead);
                string msgCode = Encoding.UTF8.GetString(message, 0, 5);
                string msg = "";

                for (int i = 0; i < msgRaw.Length; i++)
                {
                    if (check(msgRaw[i]).Equals(false))
                        msg += " ";
                    else
                        msg += msgRaw[i];
                }


                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex(@"[ ]{2,}", options);
                msg = regex.Replace(msg, @" ");
                msg = msg.Trim();


                if (msgCode == "SR-OK")
                {
                    RichTextBox2.Visibility = Visibility.Visible;
                    FlowDocument myFlowDoc = new FlowDocument();
                    myFlowDoc.Blocks.Add(new Paragraph(new Run(msg.Replace("%", Environment.NewLine))));

                    RichTextBox2.Document = myFlowDoc;

                    TextBox2.Text = DateTime.Now.ToString();
                }
                else
                {
                    RichTextBox1.Visibility = Visibility.Visible;

                    FlowDocument myFlowDoc = new FlowDocument();
                    myFlowDoc.Blocks.Add(new Paragraph(new Run("błąd serwera: " + msgCode)));

                    RichTextBox1.Document = myFlowDoc;
                }
            }
            catch (Exception e)
            {
                RichTextBox1.Visibility = Visibility.Visible;

                FlowDocument myFlowDoc = new FlowDocument();
                myFlowDoc.Blocks.Add(new Paragraph(new Run("exception: " + e)));

                RichTextBox1.Document = myFlowDoc;
            }
        }
        public bool check(char sign)
        {
            int[] signTab = new int[11] { 37, 45, 261, 263, 281, 322, 324, 243, 347, 380, 378 };

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
