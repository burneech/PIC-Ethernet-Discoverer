# PIC Ethernet Discoverer

## Android/iOS app for discovering Microchip PIC MCUs on the local network

<img src="https://i.imgur.com/jy6YFmQ.jpg" alt="Android App" width="30%" height="30%">

In order for the PIC MCU to be discoverable by this application, it needs to implement the [Announce module](https://documentation.help/Microchip-TCP.IP-Stack/Announce.html) from the Microchip TCP/IP stack.

### Application info
- Xamarin.Forms (Android & iOS)
- .NET Standard 2.0
- Application icon by [Finee Icons](https://icon-icons.com/users/OwQZcKa644lKTrLZuZCkc/icon-sets/) from [icon-icons.com](https://icon-icons.com/icon/search-locate-find-phone-mobile/80465).

### Good to know
- Xamarin does not like long project paths. If you encounter weird build errors just move the project closer to your drive's root
  - (e.g. `C:/PIC-Ethernet-Discoverer/`)
