# Bitbeans.Zpl

Create **Zebra ZPL II** labels in .NET 10 with typed elements - text, 1D/2D barcodes, graphics,
images and stored formats. **No dependencies.**

```csharp
using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

var engine = new ZplEngine
{
    new ZplTextField("Hello ZPL", 50, 50, new ZplFont(fontWidth: 40, fontHeight: 40)),
    new ZplBarcode128("123ABC", 50, 120),
    new ZplQrCode("https://example.com", 400, 50, magnificationFactor: 4),
    new ZplGraphicBox(30, 30, 700, 400, borderThickness: 4),
};

string zpl = engine.ToZplString(new ZplRenderOptions { SourcePrintDpi = 203, TargetPrintDpi = 300 });
```

Embedding images (`~DG`, `~DY`) needs an `IImageConverter`, for example from
[Bitbeans.Zpl.SkiaSharp](https://www.nuget.org/packages/Bitbeans.Zpl.SkiaSharp).
Render previews with [Bitbeans.Zpl.Viewer](https://www.nuget.org/packages/Bitbeans.Zpl.Viewer).

Documentation: https://github.com/bitbeans/Bitbeans.Zpl/blob/main/docs/labels.md

Fork of [BinaryKits.Zpl](https://github.com/BinaryKits/BinaryKits.Zpl) (MIT).
