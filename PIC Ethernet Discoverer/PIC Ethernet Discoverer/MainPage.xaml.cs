using System.Text.RegularExpressions;
using PIC_Ethernet_Discoverer.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Essentials;
using System.Net.Sockets;
using Xamarin.Forms;
using System.Linq;
using System.Text;
using System.Net;
using System;

namespace PIC_Ethernet_Discoverer
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private ObservableCollection<DiscoveredDevice> _listOfDiscoveredDevices = new ObservableCollection<DiscoveredDevice>();
        public ObservableCollection<DiscoveredDevice> ListOfDiscoveredDevices
        {
            get { return _listOfDiscoveredDevices; }
            set { _listOfDiscoveredDevices = value; OnPropertyChanged(nameof(ListOfDiscoveredDevices)); }
        }

        private UdpState GlobalUDP;

        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = this;
            InitializeApp();
            CheckWiFi();
        }

        private void InitializeApp()
        {
            Connectivity.ConnectivityChanged += ConnectivityChanged;
            ButtonFindDevices.IsEnabled = false;
        }

        private void CheckWiFi()
        {
            if (Connectivity.ConnectionProfiles.Contains(ConnectionProfile.WiFi) &&
                Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
            {
                LabelWifiStatus.Text = $"Wi-Fi is enabled";
                LabelWifiStatus.TextColor = Color.GreenYellow;
                ButtonFindDevices.IsEnabled = true;

                InitialDiscover();
            }
            else
            {
                LabelWifiStatus.Text = $"Please enable Wi-Fi";
                LabelWifiStatus.TextColor = Color.OrangeRed;

                ButtonFindDevices.IsEnabled = false;
            }
        }

        private void SearchDevices()
        {
            ListOfDiscoveredDevices.Clear();

            Vibration.Vibrate(TimeSpan.FromMilliseconds(50));
            byte[] DiscoverMsg = Encoding.ASCII.GetBytes("Discovery message.");
            GlobalUDP.UDPClient.Send(DiscoverMsg, DiscoverMsg.Length, new IPEndPoint(IPAddress.Parse("255.255.255.255"), 30303));
        }

        #region UDP

        private void InitialDiscover()
        {
            ListOfDiscoveredDevices.Clear();

            GlobalUDP.UDPClient = new UdpClient();
            GlobalUDP.EP = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 30303);
            IPEndPoint BindEP = new IPEndPoint(IPAddress.Any, 30303);
            byte[] DiscoverMsg = Encoding.ASCII.GetBytes("Discovery message.");

            GlobalUDP.UDPClient.Client.Bind(BindEP);
            GlobalUDP.UDPClient.EnableBroadcast = true;
            GlobalUDP.UDPClient.MulticastLoopback = false;
            GlobalUDP.UDPClient.BeginReceive(ReceiveCallback, GlobalUDP);
            GlobalUDP.UDPClient.Send(DiscoverMsg, DiscoverMsg.Length, new IPEndPoint(IPAddress.Parse("255.255.255.255"), 30303));
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            UdpState MyUDP = (UdpState)ar.AsyncState;

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

            MyUDP.UDPClient.BeginReceive(ReceiveCallback, MyUDP);
        }

        struct UdpState
        {
            public IPEndPoint EP;
            public UdpClient UDPClient;
        }

        #endregion

        private void ConnectivityChanged(object sender, ConnectivityChangedEventArgs e) => CheckWiFi();
        private void ButtonFindDevices_Clicked(object sender, EventArgs e) => SearchDevices();
        private void ImageLogo_Tapped(object sender, EventArgs e) => Launcher.OpenAsync(new Uri("https://github.com/burneech/PIC-Ethernet-Discoverer"));
        private void ListViewDiscoveredDevices_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var tappedItem = e.Item as DiscoveredDevice;
            if (tappedItem == null)
                return;
            Launcher.OpenAsync(new Uri($"http://{tappedItem.IP}"));
        }

        #region INotify stuff
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
