# Viewer

`Bitbeans.Zpl.Viewer` parses ZPL II and renders it to an image with SkiaSharp - an offline alternative
to labelary.com. It has no ImageSharp dependency.

```shell
dotnet add package Bitbeans.Zpl.Viewer
```

```csharp
using Bitbeans.Zpl.Viewer;
using Bitbeans.Zpl.Viewer.ElementDrawers;
using Bitbeans.Zpl.Viewer.Models;
```

## Rendering

Rendering is a two step process: `ZplAnalyzer` turns ZPL into elements (one `LabelInfo` per `^XA` ... `^XZ`),
`ZplElementDrawer` draws the elements of a label.

```csharp
IPrinterStorage printerStorage = new PrinterStorage();
var analyzer = new ZplAnalyzer(printerStorage);
var drawer = new ZplElementDrawer(printerStorage);

AnalyzeInfo analyzeInfo = analyzer.Analyze("^XA^FT100,100^A0N,67,0^FDTest label^FS^XZ");

foreach (LabelInfo label in analyzeInfo.LabelInfos)
{
    byte[] png = drawer.Draw(label.ZplElements, labelWidth: 101.6, labelHeight: 152.4, printDensityDpmm: 8);
    File.WriteAllBytes("label.png", png);
}
```

| `Draw` parameter | Default | Description |
| --- | --- | --- |
| `labelWidth` / `labelHeight` | `101.6` / `152.4` | Label size in millimeters (4 x 6 inch) |
| `printDensityDpmm` | `8` | Dots per millimeter: 6 = 152 dpi, 8 = 203 dpi, 12 = 300 dpi, 24 = 600 dpi |
| `logger` | `null` | `Microsoft.Extensions.Logging.ILogger` for drawing problems (invalid barcode content, ...) |

`DrawSurface(SKSurface, ...)` draws onto an existing Skia surface instead of encoding an image.

### Analyzer results

| `AnalyzeInfo` member | Content |
| --- | --- |
| `LabelInfos` | Parsed labels with their elements; stored formats (`^DF`) are merged with `^XF` data |
| `UnknownCommands` | Commands the viewer does not support |
| `Errors` | Commands that could not be parsed, including the exception message |

Elements in `LabelInfos` keep the field data as it appears in the ZPL, including `^FH` hex escapes, so
that the drawer can decode them. They are meant for drawing and inspection. Calling `ToZplString()` on
them escapes the indicator again (`_5F` becomes `_5F5F`); build new elements with plain text to produce ZPL.

The `IPrinterStorage` shares downloaded graphics (`~DG`, `~DY`) and formats between the analyzer and
the drawer. Reuse the same instance for both, and across calls if labels reference earlier downloads.

## `DrawerOptions`

```csharp
var options = new DrawerOptions
{
    RenderFormat = SKEncodedImageFormat.Png,   // Png, Jpeg or Webp
    RenderQuality = 80,                        // for lossy formats
    OpaqueBackground = true,                   // default is a transparent background
    BackgroundColor = SKColors.White,
    Antialias = true,
    ReplaceDashWithEnDash = true,
    ReplaceUnderscoreWithEnSpace = false,
    TextRenderingMode = ZplTextRenderingMode.Outline,
};

var drawer = new ZplElementDrawer(printerStorage, options);
```

## Fonts

ZPL font `0` is mapped to a condensed bold sans serif, all other fonts to a monospace font. The viewer
picks the first installed font of a font stack:

| Font | Default stack (`FontManager`) |
| --- | --- |
| `0` (`FontStack0`) | Swis721 Cn BT, TeX Gyre Heros Cn, Nimbus Sans Narrow, Roboto Condensed, Helvetica, Helvetica Neue, Arial |
| other (`FontStackA`) | DejaVu Sans Mono, Lucida Console, Andale Mono, Droid Sans Mono |

For identical results on every machine ship your own fonts:

```csharp
var options = new DrawerOptions();
options.FontManager.RegisterTypeface(SKTypeface.FromFile("fonts/RobotoCondensed-Bold.ttf"));
options.FontManager.FontStack0 = ["Roboto Condensed"];
```

Or map ZPL font names yourself (for example for downloaded or CJK fonts):

```csharp
options.FontManager.FontLoader = fontName => fontName switch
{
    "0" => SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
    "1" => SKTypeface.FromFamilyName("SimSun"),
    _ => SKTypeface.Default,
};
```

ZPL font download commands are not supported.

### Bitmap fonts

`ZplTextRenderingMode.BitmapStrict` renders text with bitmap glyphs instead of outlines, which matches
the printer output dot by dot. Glyph data is not shipped (Zebra fonts are proprietary); provide it with an
`IZplBitmapFontProvider`:

```csharp
public sealed class MyBitmapFonts : IZplBitmapFontProvider
{
    public bool TryGet(string fontName, int printDensityDpmm, InternationalFont internationalFont, out ZplBitmapFontData font)
    {
        // build ZplBitmapFontData from ZplBitmapFontMetrics and ZplBitmapGlyph instances
        font = null;
        return false;
    }
}

var options = new DrawerOptions
{
    TextRenderingMode = ZplTextRenderingMode.BitmapStrict,
    BitmapFontProvider = new MyBitmapFonts(),
};
```

Font `0` is a scalable font and is always rendered as outline. For every other font the provider is
asked for that font, then for font `A`; if neither is available the text is rendered as outline.

## Platform notes

- Windows, Linux (x64/arm64, no system dependencies) and macOS are supported. The Linux native assets
  of SkiaSharp and HarfBuzzSharp are referenced by the packages.
- Text output depends on the installed fonts; register fonts explicitly for reproducible images.
- PDF output is not supported.

See [supported commands](viewer-support.md) for the barcode and command matrix.
