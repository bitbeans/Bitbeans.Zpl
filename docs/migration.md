# Migrating from BinaryKits.Zpl

## Packages

| BinaryKits.Zpl | Bitbeans.Zpl |
| --- | --- |
| `BinaryKits.Zpl.Label` | `Bitbeans.Zpl` (+ `Bitbeans.Zpl.SkiaSharp` for images) |
| `BinaryKits.Zpl.Viewer` | `Bitbeans.Zpl.Viewer` |
| `BinaryKits.Zpl.Protocol` | not continued |
| `BinaryKits.Zpl.Labelary` | not continued |

Only **.NET 10** is targeted. ImageSharp is not used anywhere.

## Namespaces

| Old | New |
| --- | --- |
| `BinaryKits.Zpl.Label` | `Bitbeans.Zpl` |
| `BinaryKits.Zpl.Label.Elements` | `Bitbeans.Zpl.Elements` |
| `BinaryKits.Zpl.Label.Helpers` | `Bitbeans.Zpl.Helpers` |
| `BinaryKits.Zpl.Label.ImageConverters` | `Bitbeans.Zpl.ImageConverters` |
| `BinaryKits.Zpl.Viewer.*` | `Bitbeans.Zpl.Viewer.*` |

A search and replace of `BinaryKits.Zpl.Label` with `Bitbeans.Zpl` followed by `BinaryKits.Zpl` with
`Bitbeans.Zpl` converts most code.

## Breaking changes

### Images need an explicit converter

```csharp
// before
new ZplDownloadGraphics('R', "SAMPLE", imageData, ZplCompressionScheme.ACS);
new ZplDownloadObjects('R', "SAMPLE.PNG", imageData);

// after
var converter = new SkiaImageConverter(); // Bitbeans.Zpl.SkiaSharp
new ZplDownloadGraphics('R', "SAMPLE", imageData, converter, ZplCompressionScheme.ACS);
new ZplDownloadObjects('R', "SAMPLE.PNG", imageData, converter);
```

`ImageSharpImageConverter` is replaced by `SkiaImageConverter`. `IImageConverter` has a new member
`ConvertToPng(byte[] imageData, double scaleFactor)`. The GRF to PNG conversion is available as
`SkiaImageConverter.ConvertGrfToPng`.

### Behavior changes

- `ZplDownloadGraphics` scales the image by `TargetPrintDpi / SourcePrintDpi`. BinaryKits always halved the
  image when the factor was not 1 ([upstream #346](https://github.com/BinaryKits/BinaryKits.Zpl/issues/346)).
- Image names longer than 8 characters now throw an `ArgumentException` (the check existed but never threw).
- Transparent image pixels are treated as white instead of black.
- Z64 compressed data uses .NET's zlib and can differ byte-wise from older output; the decoded graphic is identical.
- Viewer: fields are finalized at `^FS` instead of `^FD`
  ([upstream #332](https://github.com/BinaryKits/BinaryKits.Zpl/pull/332)), which fixes `^BY` default barcode
  heights and several positioning cases.

### Viewer API

- `DrawerOptions.FontLoader` (obsolete) was removed, use `DrawerOptions.FontManager.FontLoader`.
- PDF output is not available.
