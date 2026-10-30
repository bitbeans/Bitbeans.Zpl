# Bitbeans.Zpl.Viewer

Render **ZPL II** labels to PNG, JPEG or WebP in .NET 10 - an offline alternative to labelary.com,
based on SkiaSharp, HarfBuzz and ZXing.Net. **No ImageSharp.**

```csharp
using Bitbeans.Zpl.Viewer;
using Bitbeans.Zpl.Viewer.Models;

IPrinterStorage printerStorage = new PrinterStorage();
var analyzer = new ZplAnalyzer(printerStorage);
using var drawer = new ZplElementDrawer(printerStorage);

AnalyzeInfo analyzeInfo = analyzer.Analyze("^XA^FO50,50^A0N,50,50^FDHello^FS^FO50,120^BCN,100^FD12345^FS^XZ");
foreach (LabelInfo label in analyzeInfo.LabelInfos)
{
    byte[] png = drawer.Draw(label.ZplElements, labelWidth: 101.6, labelHeight: 152.4, printDensityDpmm: 8);
    File.WriteAllBytes("label.png", png);
}
```

Supports text (outline and bitmap fonts, field blocks, rotation, reverse print), graphics, downloaded
images, stored formats (`^DF`/`^XF`) and the common 1D and 2D barcodes (Code 11/39/93/128, EAN, UPC,
2 of 5, Codabar, MSI, LOGMARS, QR, Data Matrix, PDF417, Aztec, MaxiCode).

On Linux, add a SkiaSharp native package to the application: `SkiaSharp.NativeAssets.Linux` (finds the system
fonts, needs `libfontconfig1`) or `SkiaSharp.NativeAssets.Linux.NoDependencies` (minimal container images; register
the fonts with `FontManager.RegisterTypeface`). Ubuntu/Docker examples:
https://github.com/bitbeans/Bitbeans.Zpl/blob/main/docs/viewer.md#linux

Documentation: https://github.com/bitbeans/Bitbeans.Zpl/blob/main/docs/viewer.md
