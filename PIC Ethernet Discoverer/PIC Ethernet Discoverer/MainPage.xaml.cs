using PIC_Ethernet_Discoverer.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Net.Sockets;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Linq;
using System.Text;
using System.Net;
using System;

namespace PIC_Ethernet_Discoverer
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<DiscoveredDevice> _listOfDiscoveredDevices = new ObservableCollection<DiscoveredDevice>();
        public ObservableCollection<DiscoveredDevice> ListOfDiscoveredDevices
        {
            get { return _listOfDiscoveredDevices; }
            set { _listOfDiscoveredDevices = value; OnPropertyChanged(nameof(ListOfDiscoveredDevices)); }
        }

        // Microchip PICs are usually set to receive the discovery message on port 30303
        private const int PICListeningPort = 30303;

        private const string BroadcastIPAddress = "255.255.255.255";
        private const string DiscoveryMessage = "Who's there?";
        private readonly byte[] DiscoveryMessageBytes; 

        private UdpState GlobalUDP;

        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = this;

            DiscoveryMessageBytes = Encoding.ASCII.GetBytes(DiscoveryMessage);
            InitializeApp();
            CheckWiFi();
        }

        private void InitializeApp()
        {
            Connectivity.ConnectivityChanged += ConnectivityChanged;

            ButtonFindDevices.IsEnabled = false;

            GlobalUDP.UDPClient = new UdpClient();
            GlobalUDP.EP = new IPEndPoint(IPAddress.Parse(BroadcastIPAddress), PICListeningPort);
            IPEndPoint BindEP = new IPEndPoint(IPAddress.Any, PICListeningPort);

            // UDP listening port
            GlobalUDP.UDPClient.Client.Bind(BindEP);

            GlobalUDP.UDPClient.EnableBroadcast = true;
            GlobalUDP.UDPClient.MulticastLoopback = false;

            // Configures listening for the response
            GlobalUDP.UDPClient.BeginReceive(ReceiveCallback, GlobalUDP);
        }

        private void CheckWiFi()
        {
            if (Connectivity.ConnectionProfiles.Contains(ConnectionProfile.WiFi) &&
                Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
            {
                LabelWifiStatus.Text = $"Wi-Fi is enabled";
                LabelWifiStatus.TextColor = Color.GreenYellow;
                ButtonFindDevices.IsEnabled = true;

                SendDiscoveryMessage();
            }
            else
            {
                LabelWifiStatus.Text = $"Please enable Wi-Fi";
                LabelWifiStatus.TextColor = Color.OrangeRed;

                ButtonFindDevices.IsEnabled = false;
            }
        }

        private void SendDiscoveryMessage()
        {
            ListOfDiscoveredDevices.Clear();

            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));

            GlobalUDP.UDPClient.Send(
                    DiscoveryMessageBytes,
                    DiscoveryMessageBytes.Length,
                    new IPEndPoint(IPAddress.Parse(BroadcastIPAddress), PICListeningPort));
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            UdpState MyUDP = (UdpState)ar.AsyncState;

            // Splits the response message string into Hostname, IP and MAC addresses
            string[] ReceivedLines = Regex.Split(Encoding.ASCII.GetString(MyUDP.UDPClient.EndReceive(ar, ref MyUDP.EP)), "\r\n");

            if (ReceivedLines.Length == 3)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ListOfDiscoveredDevices.Add(new DiscoveredDevice
                    {
                        Hostname = ReceivedLines[0].TrimEnd(),
                        IP = MyUDP.EP.Address.ToString(),
                        MAC = ReceivedLines[1]
                    });
                });
            }

            // Configures the UDP client for receiving further messages
            MyUDP.UDPClient.BeginReceive(ReceiveCallback, MyUDP);
        }

        struct UdpState
        {
            public IPEndPoint EP;
            public UdpClient UDPClient;
        }

        private void ConnectivityChanged(object sender, ConnectivityChangedEventArgs e) => CheckWiFi();

        private void ButtonFindDevices_Clicked(object sender, EventArgs e) => SendDiscoveryMessage();

        private void ImageLogo_Tapped(object sender, EventArgs e) => Launcher.OpenAsync(new Uri("https://github.com/burneech/PIC-Ethernet-Discoverer"));

        private void ListViewDiscoveredDevices_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var tappedItem = e.Item as DiscoveredDevice;
            if (tappedItem == null)
                return;

            Launcher.OpenAsync(new Uri($"http://{tappedItem.IP}"));
        }

        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
