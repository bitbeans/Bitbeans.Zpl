# Images

ZPL printers only print black and white dots. `Bitbeans.Zpl` offers two ways to put an image on a label.
Both need an `IImageConverter`; the core package deliberately contains no imaging code.

```shell
dotnet add package Bitbeans.Zpl.SkiaSharp
```

```csharp
using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.SkiaSharp;

var converter = new SkiaImageConverter();
```

> For the best result convert the graphic to black and white before. Images are scaled
> automatically when `SourcePrintDpi` and `TargetPrintDpi` differ.

## 1. `ZplDownloadGraphics` (`~DG` + `^XG`)

The library converts the image to a bitonal graphic (GRF) and the printer receives ready-to-print data.
Works on every ZPL printer.

```csharp
var engine = new ZplEngine
{
    new ZplDownloadGraphics('R', "SAMPLE", File.ReadAllBytes("logo.png"), converter),
    new ZplRecallGraphic(100, 100, 'R', "SAMPLE"),
};

string zpl = engine.ToZplString(new ZplRenderOptions { SourcePrintDpi = 203, TargetPrintDpi = 300 });
```

The graphic data can be compressed:

| `ZplCompressionScheme` | Encoding |
| --- | --- |
| `ACS` (default) | Zebra ASCII compression (run length) |
| `Z64` | zlib deflate + Base64 + CRC |
| `B64` | Base64 + CRC |
| `None` | Plain hexadecimal |

```csharp
new ZplDownloadGraphics('R', "SAMPLE", imageData, converter, ZplCompressionScheme.Z64);
```

The image name is limited to 8 characters.

## 2. `ZplDownloadObjects` (`~DY` + `^IM`)

The image is sent as PNG and the printer converts it. Requires a printer firmware that supports `~DY`
with PNG.

```csharp
var engine = new ZplEngine
{
    new ZplDownloadObjects('R', "SAMPLE.PNG", File.ReadAllBytes("logo.png"), converter),
    new ZplImageMove(100, 100, 'R', "SAMPLE", "PNG"),
};
```

## `SkiaImageConverter`

| Member | Description |
| --- | --- |
| `SkiaImageConverter(int threshold = 128)` | Pixels with an average luminance below `threshold` become black |
| `ConvertImage(byte[])` | Encoded image (PNG, JPEG, BMP, GIF, WebP, ...) to GRF |
| `ConvertToPng(byte[], double scaleFactor)` | Decode, scale (Mitchell cubic) and encode as PNG |
| `ConvertGrfToPng(byte[], int bytesPerRow)` | GRF back to PNG with transparent background |

Transparent pixels are composed onto white, so a PNG with a transparent background prints as expected.
TIFF input is not supported by SkiaSharp.

## Writing your own converter

Implement `IImageConverter` to use another imaging library (System.Drawing, Magick.NET, ...)
or a custom dithering:

```csharp
using Bitbeans.Zpl.ImageConverters;

public sealed class MyConverter : IImageConverter
{
    public ImageResult ConvertImage(byte[] imageData)
    {
        // RawData: one bit per pixel, most significant bit first, 1 = black,
        // every row padded to whole bytes
        return new ImageResult { RawData = data, BytesPerRow = bytesPerRow, BinaryByteCount = data.Length };
    }

    public byte[] ConvertToPng(byte[] imageData, double scaleFactor) => /* scaled PNG */;
}
```
