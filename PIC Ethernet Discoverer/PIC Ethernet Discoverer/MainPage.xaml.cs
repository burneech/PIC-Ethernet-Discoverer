using Xamarin.Essentials;
using Xamarin.Forms;
using System;

namespace PIC_Ethernet_Discoverer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ImageLogo_Tapped(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("https://github.com/burneech/PIC-Ethernet-Discoverer"));
        }
    }
}
