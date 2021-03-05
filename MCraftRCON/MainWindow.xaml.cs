using CoreRCON;
using CoreRCON.PacketFormats;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MCraftRCON
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RCON client;

        public MainWindow()
        {
            InitializeComponent();
            RconPwdBox.Password = "12345678";
        }

        private async void InputCmdTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                appendConsole(await client.SendCommandAsync(InputCmdTB.Text));
                InputCmdTB.Text = "";
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
           
            ConnectAsync();
        }

        private async void ConnectAsync()
        {
            ConsoleBox.IsReadOnly = true;
            if (!string.IsNullOrEmpty(HostTxtBox.Text))
            {
                string[] cInfo = HostTxtBox.Text.Split(':');

                string target = cInfo[0];
                // Resolve DNS.
                if (!isIPAddress(cInfo[0]))
                {
                    /*IPAddress[] addresslist = Dns.GetHostAddresses(cInfo[0]);
                    target = addresslist[0].ToString();
                    foreach (IPAddress theaddress in addresslist)
                    {
                        appendConsole(theaddress.ToString() + "\n");
                    }*/







                    IPHostEntry hostEntry;

                    hostEntry = Dns.GetHostEntry(cInfo[0]);

                    //you might get more than one ip for a hostname since 
                    //DNS supports more than one record

                    if (hostEntry.AddressList.Length > 0)
                    {
                        target = hostEntry.AddressList[0].ToString();
                        //var ip = hostEntry.AddressList[0];
                        //Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                        //s.Connect(ip, 80);
                        foreach (IPAddress ips in hostEntry.AddressList)
                            {
                            appendConsole(ips.ToString());
                        }
                    }
                }

                

                
                try
                {
                    MinecraftQueryInfo status;
                    if (cInfo.Length < 2)
                    {
                        // If port not provided
                        appendConsole($"[Info] Pinging to... {target}");
                        status = await ServerQuery.Info(IPAddress.Parse(target), ushort.Parse(cInfo[1]), ServerQuery.ServerType.Minecraft) as MinecraftQueryInfo;
                    }
                    else
                    {
                        appendConsole($"[Info] Pinging to... {target} Port: {ushort.Parse(cInfo[1])}");
                        status = await ServerQuery.Info(IPAddress.Parse(target), ushort.Parse(cInfo[1]), ServerQuery.ServerType.Minecraft) as MinecraftQueryInfo;
                    }

                    appendConsole("Info has been fetched:");
                    appendConsole("---------------------------------");

                   
                    IList<PropertyInfo> props = new List<PropertyInfo>(typeof(MinecraftQueryInfo).GetProperties());
                    foreach (var prop in props)
                    {
                        if (prop.Name == "Players")
                        {
                            appendConsole("Players: ");
                            foreach (var player in (List<string>)prop.GetValue(status))
                            {
                             
                                appendConsole("- " + player);
                            }
                        }
                        else
                        {
                            appendConsole(prop.Name + ": " + prop.GetValue(status));
                        }
                    }

                    appendConsole("---------------------------------");
                    appendConsole($"Connecting RCON --> {target + UInt16.Parse(cInfo[2])}:");
                    client = new RCON(IPAddress.Parse(target), UInt16.Parse(cInfo[2]), RconPwdBox.Password);
                    InputCmdTB.IsEnabled = false;
                    var cmd = await client.SendCommandAsync("say Connected");
                    appendConsole(cmd);
                    InputCmdTB.IsEnabled = true;
                    client.OnDisconnected += Client_OnDisconnected;
                }
                catch (Exception ex)
                {
                    appendConsole($"[Error] Failed to connect.\n{ex}");
                }
            }
            else
            {
                appendConsole("[Error] Please fill hostname.");
            }
        }

        private void Client_OnDisconnected()
        {
            appendConsole("Disconnected!");
        }

        /*public void Log(string text, string color)
        {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(console, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    bc.ConvertFromString(color));
            }
            catch (FormatException) { }
        }*/

        private bool isIPAddress(string input)
        {
            return IPAddress.TryParse(input, out IPAddress ip);
        }

        private void appendConsole(string text)
        {
            TextRange tr = new TextRange(ConsoleBox.Document.ContentEnd, ConsoleBox.Document.ContentEnd);

            tr.Text = text + Environment.NewLine;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
        }

        private void ConsoleBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private async void StopServerBtn_Click(object sender, RoutedEventArgs e)
        {
            appendConsole(await client.SendCommandAsync("stop"));
            InputCmdTB.Text = "";
        }

        private void SayBtn_Click(object sender, RoutedEventArgs e)
        {
            InputCmdTB.AppendText("say ");
            appendConsole("Complete this command at input box.");
        }
    }
}