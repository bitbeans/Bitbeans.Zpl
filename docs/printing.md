# Printing

ZPL is plain text. Most network printers accept it on TCP port 9100 (raw printing):

```csharp
using System.Net.Sockets;
using System.Text;

string zpl = "^XA^FO50,50^A0N,40,40^FDHello printer^FS^XZ";

using var client = new TcpClient();
await client.ConnectAsync("192.168.1.50", 9100);
await using NetworkStream stream = client.GetStream();

// ZPL defaults to code page 850; use ^CI28 in the label for UTF-8 text
byte[] data = Encoding.UTF8.GetBytes(zpl);
await stream.WriteAsync(data);
await stream.FlushAsync();
```

USB printers can be addressed via the operating system spooler in raw mode (for example the Windows
`WritePrinter` API or `lp -o raw` on Linux/macOS).

## Printer resolution

Labels are measured in dots. Design for one resolution and let the engine scale:

```csharp
var options = new ZplRenderOptions { SourcePrintDpi = 203, TargetPrintDpi = 300 };
```

| Print density | dpi | dots per mm (viewer `printDensityDpmm`) |
| --- | --- | --- |
| 6 dpmm | 152 | 6 |
| 8 dpmm | 203 | 8 |
| 12 dpmm | 300 | 12 |
| 24 dpmm | 600 | 24 |

## Printers that understand ZPL

| Manufacturer | Emulation |
| --- | --- |
| [Zebra Technologies](https://www.zebra.com) | native |
| [Honeywell](https://sps.honeywell.com) | ZSIM |
| [Avery Dennison](https://www.averydennison.com) | MLI |
| [SATO](https://www.satoeurope.com) | SZPL |
| [Printronix](https://www.printronix.com) | ZGL |
| [GoDEX](https://www.godexintl.com) | GZPL |
| [cab](https://www.cab.de), [Toshiba Tec](https://www.toshibatec.eu), [AirTrack](https://airtrack.com) | yes |

The [ZPL II programming guide](https://www.zebra.com/content/dam/zebra/manuals/printers/common/programming/zpl-zbi2-pm-en.pdf)
documents every command.
