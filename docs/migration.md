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
- `ZplDownloadGraphics` and `ZplDownloadObjects` reject the read-only storage device `Z`.
- Viewer: `~DY` objects are stored under name and extension (`~DYR:LOGO,P,P,...` as `LOGO.PNG`); read them from
  `IPrinterStorage` with the full file name.
- Transparent image pixels are treated as white instead of black.
- Z64 compressed data uses .NET's zlib and can differ byte-wise from older output; the decoded graphic is identical.
- Viewer: fields are finalized at `^FS` instead of `^FD`
  ([upstream #332](https://github.com/BinaryKits/BinaryKits.Zpl/pull/332)), which fixes `^BY` default barcode
  heights and several positioning cases.
- Field data is escaped. `^` and `~` in content no longer inject commands; they are written as hex escapes
  with `^FH`. Content that already contains hex escapes (`_1D`) is escaped again, so pass the literal
  character (`"\u001D"`) instead.
- Backslashes in text fields are kept instead of being replaced with a space.
- `^BY` module widths and QR/Aztec magnification are scaled with the print DPI. `Scale(int)` rounds
  instead of truncating, so converted labels can differ by one dot.
- `ZplImageMove` and `ZplRecallFormat` end with `^FS`, `ZplRecallFormat` no longer throws.
- `ZplRecallGraphic` defaults to `useDefaultPosition: false`.
- Constructors validate more input: hexadecimal indicators, QR orientation (only normal), QR/Aztec
  magnification (1 to 10), corner rounding (0 to 8), Code 128 mode, graphic symbol characters and
  compression schemes. `ZplBarcodeAnsiCodabar` throws `ArgumentException`.
- B64/Z64 decoding checks the CRC and throws `FormatException` for corrupt data.
- Subclasses of `ZplBarcode` call `RenderModuleWidth(context)` instead of `RenderModuleWidth()`.

### Viewer API

- `DrawerOptions.FontLoader` (obsolete) was removed, use `DrawerOptions.FontManager.FontLoader`.
- PDF output is not available.
