# Changelog

All notable changes to this project are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses
[Semantic Versioning](https://semver.org/).

## [Unreleased]

## [1.0.0] - unreleased

First release as **Bitbeans.Zpl**, a fork of [BinaryKits.Zpl](https://github.com/BinaryKits/BinaryKits.Zpl)
3.3.1 / Viewer 1.3.1. See the [migration guide](docs/migration.md).

### Added

- `Bitbeans.Zpl.SkiaSharp` package with `SkiaImageConverter` (`IImageConverter` based on SkiaSharp).
- `IImageConverter.ConvertToPng` so the core package needs no imaging library.
- `SkiaImageConverter` threshold parameter.
- Viewer improvements from [ivanjx/BinaryKits.Zpl](https://github.com/ivanjx/BinaryKits.Zpl):
  bitmap font rendering (`ZplTextRenderingMode.BitmapStrict`, `IZplBitmapFontProvider`), Code 11, Code 93
  check digit, EAN-8, MSI, LOGMARS, Industrial 2 of 5, Aztec (`^B0`), MaxiCode, all Data Matrix parameters parsed,
  diagonal lines, ellipses, field block justification, background color option and more.

### Changed

- `Bitbeans.Zpl` has **no dependencies**; ImageSharp was removed from all packages.
- Targets .NET 10 only.
- `ZplDownloadGraphics` and `ZplDownloadObjects` require an `IImageConverter`.
- Viewer: fields are finalized at `^FS` instead of `^FD`
  ([upstream #332](https://github.com/BinaryKits/BinaryKits.Zpl/pull/332), extended to all symbologies).
- Transparent image pixels are converted to white instead of black.
- Z64 compression uses `ZLibStream`.
- Field data of all barcodes, 2D codes and text fields is escaped: `^`, `~` and the active hexadecimal
  indicator are written as hex escapes. Content with `^` or `~` and no indicator gets `^FH` automatically.
  Content that already contains escapes such as `_1D` is escaped again; write the literal character instead.
- Text fields keep backslashes (`ZplFieldBlock` writes `\\`) instead of replacing them with a space.
  `ZplRecallFieldNumber` escapes `^`, `~` and `_` instead of replacing them with a space.
- Module widths (`^BY`) and QR/Aztec magnification follow the DPI scale and are clamped to 1 to 10.
  `ZplRenderOptions.Scale(int)` rounds instead of truncating and never turns a non-zero value into 0.
  Box corner rounding is not scaled.
- `ZplImageMove` and `ZplRecallFormat` output ends with `^FS`; `ZplRecallFormat` renders `^XF`
  instead of throwing.
- `ZplRecallGraphic` uses `useDefaultPosition: false` by default.
- Stricter validation (`ArgumentException`/`ArgumentOutOfRangeException`): hexadecimal indicators
  (`^`, `~`, control and non-ASCII characters; a space is allowed), QR orientation other than normal, QR/Aztec
  magnification outside 1 to 10, corner rounding outside 0 to 8, Code 128 modes other than `N`/`U`/`A`/`D`,
  undefined graphic symbol characters, unknown compression schemes, incomplete ACS rows and print DPI of 0
  or less. `ZplBarcodeAnsiCodabar` throws `ArgumentException` instead of `InvalidOperationException`.
- B64/Z64 `Uncompress` verifies the CRC and the complete format and throws `FormatException` on mismatch.
- The protected `ZplBarcode.RenderModuleWidth()` takes the `ZplRenderOptions`.
- Viewer: QR magnification is clamped to 1 to 10 and invalid `^FH` indicators fall back to `_`.
- Viewer: `IFormatMerger.MergeFormats(List<LabelInfo>, ICollection<string>)` reports errors instead of throwing
  (default interface method, so existing implementations keep compiling; `ZplAnalyzer` uses it).
  `FormatMerger.MergeFormats(List<LabelInfo>)` still throws `InvalidOperationException` for unknown formats.
  `LabelInfo.DownloadFormatName` now contains the storage device (`R:NAME`) when `^DF` has one.
  `VirtualPrinter.SetNextFont` has an optional `orientationIsExplicit` parameter; new `VirtualPrinter` members
  `HasNextElementPosition` and `LastElementPositionY`. `PrinterStorage.AddFile` replaces existing files.

### Fixed

- MSI Mod 11 check digit is `(11 - sum mod 11) mod 11` (IBM weights 2 to 7); a value of 10 is written as `10`.
- `ZplGraphicSymbol` accepts a field justification; the viewer passes the `^FO`/`^FT`/`^FW` justification to `^GS`.
- A space can be used as `^FH` indicator.
- ZPL injection through field data containing `^` or `~`.
- `^FR` was not written for `^GB`, `^GC`, `^GE` and `^GD` with reverse print.
- Code 128 UCC check digit and mode were not written.
- PDF417 rendered `^BX` parameters; Aztec menu symbol, MaxiCode, Codabar, LOGMARS, MSI and Industrial
  2 of 5 parameter and `^FH` handling.
- `^BY`, `^FO` and `^FT` used the current culture for numbers.
- `^GF` output missed `^FS`.
- Viewer: `^CI`, empty `^XG` magnification, QR binary byte counts, format merging on Linux and templates
  that were changed by merging.
- Viewer: recalling an unknown format (`^XF`) aborted the analysis; it is now an entry in `AnalyzeInfo.Errors`.
- Viewer: formats on different storage devices with the same name collided; formats are keyed by device and name
  and a later `^DF` replaces the earlier one.
- Viewer: the Data Matrix escape character in `^FD` was split into commands.
- Viewer: a field with `^FN` and `^FD` was dropped; it is now printed and its data applies to all fields with this number.
- Viewer: `^FN1"Name"` (field number with prompt) was read as field number 0.
- Viewer: a second `~DG`/`~DY` with the same name kept the old object.
- Viewer: `^CF` without height or width reset the previous dimensions.
- Viewer: `^FW` overwrote an explicit `^A` orientation of the pending field.
- Viewer: `^FT` with an omitted y coordinate used y 0; it keeps the y of the last positioned field.
- Viewer: non-finite `^BY` ratios are ignored and `^FB` values are clamped to the documented ranges.
- `ZplDownloadGraphics` always halved the image instead of scaling by the dpi factor
  ([upstream #346](https://github.com/BinaryKits/BinaryKits.Zpl/issues/346)).
- `ZplDownloadGraphics` did not throw for image names longer than 8 characters.
- Ellipse drawer checked for the wrong element type ([upstream #342](https://github.com/BinaryKits/BinaryKits.Zpl/pull/342)).
- Security advisories of ImageSharp (GHSA-jjfr-hcj7-qf5w, GHSA-j9gm-c75j-xc9q, GHSA-j3p4-wp97-rph4,
  [upstream #348](https://github.com/BinaryKits/BinaryKits.Zpl/issues/348)) no longer apply.

### Removed

- `BinaryKits.Zpl.Protocol`, `BinaryKits.Zpl.Labelary`, the viewer web API and Docker image.
- `ImageSharpImageConverter`, `DrawerOptions.FontLoader` (use `DrawerOptions.FontManager.FontLoader`).
- .NET Framework / .NET Standard code paths.

[Unreleased]: https://github.com/bitbeans/Bitbeans.Zpl/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/bitbeans/Bitbeans.Zpl/releases/tag/v1.0.0
