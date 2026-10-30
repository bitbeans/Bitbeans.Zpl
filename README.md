<p align="center">
  <img src="docs/images/icon.png" width="112" alt="Bitbeans.Zpl logo">
</p>

<h1 align="center">Bitbeans.Zpl</h1>

<p align="center">
  Create, convert and render <b>ZPL II</b> labels for Zebra printers with .NET 10, without ImageSharp.
</p>

<p align="center">
  <a href="https://github.com/bitbeans/Bitbeans.Zpl/actions/workflows/ci.yml"><img src="https://github.com/bitbeans/Bitbeans.Zpl/actions/workflows/ci.yml/badge.svg" alt="CI"></a>
  <a href="https://www.nuget.org/packages/Bitbeans.Zpl"><img src="https://img.shields.io/nuget/v/Bitbeans.Zpl.svg?label=Bitbeans.Zpl" alt="Bitbeans.Zpl on NuGet"></a>
  <a href="https://www.nuget.org/packages/Bitbeans.Zpl.SkiaSharp"><img src="https://img.shields.io/nuget/v/Bitbeans.Zpl.SkiaSharp.svg?label=Bitbeans.Zpl.SkiaSharp" alt="Bitbeans.Zpl.SkiaSharp on NuGet"></a>
  <a href="https://www.nuget.org/packages/Bitbeans.Zpl.Viewer"><img src="https://img.shields.io/nuget/v/Bitbeans.Zpl.Viewer.svg?label=Bitbeans.Zpl.Viewer" alt="Bitbeans.Zpl.Viewer on NuGet"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT license"></a>
</p>

---

## Highlights

- **Dependency-free core.** `Bitbeans.Zpl` builds ZPL from typed elements and references no other package.
- **No ImageSharp.** Images are converted with SkiaSharp (MIT), so there are no license fees.
- **Offline preview.** `Bitbeans.Zpl.Viewer` parses ZPL and renders it to PNG, JPEG or WebP locally, without
  calling labelary.com.
- **Safe output.** Field data is escaped, so `^` and `~` in user input cannot inject printer commands.
- **Tested.** Over 1,400 tests with 100 % line and branch coverage, run on Linux, Windows and macOS.
- **Open about its origin.** Based on [BinaryKits.Zpl](https://github.com/BinaryKits/BinaryKits.Zpl) and the
  SkiaSharp port by [ivanjx](https://github.com/ivanjx/BinaryKits.Zpl). See
  [where the code comes from](#where-the-code-comes-from).

## Packages

| Package | Use it to | Depends on |
| --- | --- | --- |
| [`Bitbeans.Zpl`](src/Bitbeans.Zpl/README.md) | Create labels with typed elements | nothing |
| [`Bitbeans.Zpl.SkiaSharp`](src/Bitbeans.Zpl.SkiaSharp/README.md) | Put PNG/JPEG images on labels (`~DG`, `~DY`) | SkiaSharp |
| [`Bitbeans.Zpl.Viewer`](src/Bitbeans.Zpl.Viewer/README.md) | Render ZPL to an image | SkiaSharp, HarfBuzz, ZXing.Net |

```shell
dotnet add package Bitbeans.Zpl
dotnet add package Bitbeans.Zpl.SkiaSharp   # only if you embed images
dotnet add package Bitbeans.Zpl.Viewer      # only if you render previews
```

All packages target **.NET 10**. On Linux, the SkiaSharp based packages need a SkiaSharp native package in the
application, see [Linux](docs/viewer.md#linux).

## Quick start

### Create a label

```csharp
using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

var elements = new List<ZplElementBase>
{
    new ZplTextField("Hello ZPL", 50, 50, new ZplFont(fontWidth: 40, fontHeight: 40)),
    new ZplBarcode128("123ABC", 50, 120),
    new ZplGraphicBox(30, 30, 500, 300, borderThickness: 4),
};

string zpl = new ZplEngine(elements).ToZplString(new ZplRenderOptions());
```

### Add an image

```csharp
using Bitbeans.Zpl.SkiaSharp;

var converter = new SkiaImageConverter();
var engine = new ZplEngine
{
    new ZplDownloadGraphics('R', "SAMPLE", File.ReadAllBytes("logo.png"), converter),
    new ZplRecallGraphic(100, 100, 'R', "SAMPLE"),
};
```

### Render a preview

```csharp
using Bitbeans.Zpl.Viewer;
using Bitbeans.Zpl.Viewer.Models;

IPrinterStorage printerStorage = new PrinterStorage();
var analyzer = new ZplAnalyzer(printerStorage);
using var drawer = new ZplElementDrawer(printerStorage);

AnalyzeInfo analyzeInfo = analyzer.Analyze(zpl);
foreach (LabelInfo label in analyzeInfo.LabelInfos)
{
    // 4 x 6 inch label at 203 dpi (8 dots per mm)
    byte[] png = drawer.Draw(label.ZplElements, labelWidth: 101.6, labelHeight: 152.4, printDensityDpmm: 8);
    File.WriteAllBytes("label.png", png);
}
```

## What is supported

| Area | Elements and commands |
| --- | --- |
| Text | Text fields, field blocks (`^FB`), text blocks (`^TB`), fonts (`^A`, `^CF`), encodings (`^CI`), hex escapes (`^FH`) |
| Linear barcodes | Code 11, Code 39, Code 93, Code 128 (incl. GS1), EAN-8, EAN-13, UPC-A, UPC-E, UPC/EAN extensions, Interleaved and Industrial 2 of 5, ANSI Codabar, MSI, LOGMARS |
| 2D codes | QR Code, Data Matrix, PDF417, Aztec, MaxiCode |
| Graphics | Boxes, circles, ellipses, diagonal lines, symbols (`^GS`), reverse print (`^FR`, `^LR`) |
| Images | `~DG` + `^XG`, `~DY` + `^IM`, `^GF`; ACS, B64 and Z64 compression |
| Templates | Stored formats (`^DF`, `^XF`) with field numbers (`^FN`) |
| Output | DPI scaling between printer resolutions, comments, raw ZPL |

The viewer supports the same commands; see the [support matrix](docs/viewer-support.md) for details and
known limitations.

## Documentation

| Topic | |
| --- | --- |
| [Creating labels](docs/labels.md) | Elements, engine, render options, DPI scaling, text and barcodes |
| [Images](docs/images.md) | `~DG` vs. `~DY`, `IImageConverter`, writing your own converter |
| [Viewer](docs/viewer.md) | Rendering, `DrawerOptions`, fonts, bitmap fonts, logging, formats |
| [Supported commands](docs/viewer-support.md) | Command and barcode support of the viewer |
| [Printing](docs/printing.md) | Sending ZPL to a printer |
| [Migrating from BinaryKits.Zpl](docs/migration.md) | Namespace map and breaking changes |
| [Changelog](CHANGELOG.md) | Release notes |

## Contributing

```shell
dotnet build Bitbeans.Zpl.slnx
dotnet test Bitbeans.Zpl.slnx
pwsh ./eng/coverage.ps1     # tests, merged coverage report and the 100 % threshold check
```

Issues and pull requests are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) first, and report security
issues as described in [SECURITY.md](SECURITY.md).

## Where the code comes from

Bitbeans.Zpl is not written from scratch. Most of the code comes from other open source projects, and the
complete Git history of those projects is preserved in this repository.

| Source | What it contributed |
| --- | --- |
| [BinaryKits.Zpl](https://github.com/BinaryKits/BinaryKits.Zpl) by Binary Kits Pte. Ltd. and contributors (MIT), `BinaryKits.Zpl.Label` 3.3.1 and `BinaryKits.Zpl.Viewer` 1.3.1 | The label elements, the ZPL engine, the parser and the original viewer |
| [ivanjx/BinaryKits.Zpl](https://github.com/ivanjx/BinaryKits.Zpl), branch `new` (MIT) | The port of the viewer from ImageSharp to SkiaSharp, bitmap fonts, Code 11, EAN-8, MSI, LOGMARS, Industrial 2 of 5, Aztec, MaxiCode and many viewer fixes |
| Upstream pull request [#332](https://github.com/BinaryKits/BinaryKits.Zpl/pull/332) by MatteoH2O1999 | Fields are finalized at `^FS` instead of `^FD` |

### What Bitbeans.Zpl changes

- **Packaging:** the core has no dependencies. Image conversion moved to the separate `Bitbeans.Zpl.SkiaSharp`
  package, and ImageSharp was removed from all packages.
- **Platform:** only .NET 10 is targeted. Old .NET Framework and .NET Standard code paths were removed, as were
  the PDF output, the web API and the Labelary client.
- **Bug fixes:** a full review of the core, the parser and the renderer found and fixed many bugs. Examples:
  - ZPL injection through field data.
  - Images were always scaled by one half (upstream issue [#346](https://github.com/BinaryKits/BinaryKits.Zpl/issues/346)).
  - Wrong check digits for MSI and EAN-8.
  - Missing reverse print, missing justification and lost `^XF` labels.
  - Culture-dependent number output.
- **Quality:** the test suite grew from about a third of the code covered to 100 % line and branch coverage. The
  documentation samples are executed as tests.

The [changelog](CHANGELOG.md) lists every change, and the [migration guide](docs/migration.md) lists the
breaking changes.

### Contributing back

Pull requests are welcome, both here and upstream. Fixes that also apply to BinaryKits.Zpl are worth sending
there too.

## License

[MIT](LICENSE). The original copyright notice of Binary Kits Pte. Ltd. is retained.
