# Changelog

All notable changes to this project are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses
[Semantic Versioning](https://semver.org/).

## [Unreleased]

## [1.1.0] - 2026-10-30

### Changed

- Linux: `Bitbeans.Zpl.SkiaSharp` (and with it `Bitbeans.Zpl.Viewer`) no longer references
  `SkiaSharp.NativeAssets.Linux`. Both SkiaSharp native packages for Linux ship the same `libSkiaSharp.so`, so the
  dependency overrode the `NoDependencies` variant an application chose for a container without fontconfig, and
  rendering failed there. **On Linux, add `SkiaSharp.NativeAssets.Linux` (needs `libfontconfig1`) or
  `SkiaSharp.NativeAssets.Linux.NoDependencies` to the application**; see the
  [Linux notes](docs/viewer.md#linux). `HarfBuzzSharp.NativeAssets.Linux` stays referenced, it has no
  system dependencies. Windows and macOS are unchanged.

### Fixed

- Viewer: `FontManager` holds no SkiaSharp object in static fields, so a missing `libSkiaSharp` no longer fails its
  type initializer; the error comes from the first SkiaSharp call.

## [1.0.0] - 2026-10-30

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

- Viewer: the misspelled analyzer classes `DownloadObjectsZplCommandAnaylzer` and `FieldOriginZplCommandAnalzer` were renamed to
  `DownloadObjectsZplCommandAnalyzer` and `FieldOriginZplCommandAnalyzer` (no aliases).
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

- ZPL injection through names: `ZplRecallFormat`, `ZplDownloadGraphics`, `ZplDownloadObjects`, `ZplRecallGraphic`,
  `ZplImageMove`, `ZplFontIdentifier` and `ZplFont` validate device letters (`R`, `E`, `B`, `A`, `Z`), file names, extensions
  and font identifiers in the constructor (`ArgumentException`; no `^`, `~`, comma, colon, whitespace or control characters).
  `ZplRecallGraphic` and `ZplRecallFormat` throw `ArgumentNullException` for a `null` name. Downloads
  (`ZplDownloadGraphics`, `ZplDownloadObjects`) accept only the writable devices `R`, `E`, `B` and `A`.
- An omitted object name is valid ZPL (the printer uses `UNKNOWN`): `ZplRecallFormat` accepts an empty name, and
  `ZplDownloadGraphics`, `ZplDownloadObjects`, `ZplRecallGraphic` and `ZplImageMove` replace an empty name by `UNKNOWN`
  (a name that is only an extension, `.PNG`, becomes `UNKNOWN.PNG`).
- `ZplRecallGraphic` accepts a name with extension (`SAMPLE.GRF`); the 8 character limit applies to the name before the
  extension, and `.GRF` is only added when there is no extension (new property `FileName`). Before, the viewer could
  not recall `^XGR:SAMPLE.GRF` because the name including the extension was longer than 8 characters.
- `ZplImageMove` with an empty extension no longer renders a trailing dot (`^IMR:LOGO.GRF^FS`, new property `FileName`).
- `ZplQrCode` rejects a model other than 1 or 2 and a mask value outside 0 to 7 (`ArgumentOutOfRangeException`).
- `ZplGraphicField` XML docs: `b` is the binary byte count and `c` the graphic field count (they were swapped); the rendered order `^GFa,b,c,d` was already correct.
- NuGet packages are reproducible: CI and release set `SOURCE_DATE_EPOCH` to the commit time for `dotnet pack`.
- docs: `ZplRecallFormat` renders `^XF`.
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
- Viewer: the EAN-8 interpretation line printed a wrong check digit; EAN-8, EAN-13, UPC-A and UPC-E share one GS1 calculation.
- Viewer: `^FR` and `^LR` were ignored for bar codes; bars, modules and the interpretation line are now reversed. The bar code elements
  have optional `reversePrint` and `fieldJustification` parameters and `ZplFieldDataElementBase.ReversePrint`/`FieldJustification`.
- Viewer: bar codes are drawn from the bitmap without a PNG round trip and no longer leak bitmaps; `BarcodeDrawerBase.DrawBarcode` takes an `SKBitmap`.
- Viewer: a bar code anchored with `^FT` above the label top was shifted down; it is clipped instead.
- Viewer: the justification of `^FO`/`^FT` (`z`) was ignored for bar codes; right justified bar codes end at the origin.
- `ZplDownloadGraphics` always halved the image instead of scaling by the dpi factor
  ([upstream #346](https://github.com/BinaryKits/BinaryKits.Zpl/issues/346)).
- `ZplDownloadGraphics` did not throw for image names longer than 8 characters.
- Ellipse drawer checked for the wrong element type ([upstream #342](https://github.com/BinaryKits/BinaryKits.Zpl/pull/342)).
- Security advisories of ImageSharp (GHSA-jjfr-hcj7-qf5w, GHSA-j9gm-c75j-xc9q, GHSA-j3p4-wp97-rph4,
  [upstream #348](https://github.com/BinaryKits/BinaryKits.Zpl/issues/348)) no longer apply.
- Viewer: native Skia resources (surfaces, snapshots, encoded data, decoded images, fonts) are disposed after drawing;
  `FontManager` owns and caches the typefaces it creates and is `IDisposable`.
- Viewer: one `ZplElementDrawer` can draw on several threads at the same time (the element drawers are created per draw).
- Viewer: `^FT,y` (omitted x with explicit y) continues at the end of the previous field with baseline y for text fields and
  field blocks.
- Viewer: `^FT` graphics and images (`^GB`, `^GC`, `^GE`, `^GD`, `^GF`, `^IM`, `^XG`) above the label are clipped instead of moved down.
- Viewer: the `^FO`/`^FT` justification (right) is applied to graphics, images and field blocks.
- Viewer: `^GE` with white line color is drawn white.
- Viewer: `ZplGraphicField.Data` is the ZPL graphic data (hex, ACS, `:B64:`, `:Z64:`) and is converted when drawing, so
  `new ZplGraphicField(20, 20, 1, 1, 1, "FF")` draws a row; a parsed element renders valid ZPL.
- Viewer: `^FB` hanging indent wraps the second and following lines in the remaining width instead of clipping them.
- Viewer: `^FH` keeps surrogate pairs (emoji) in literal text; only the escapes are decoded.
- Viewer: `^GD` stays within the specified width and height.
- Viewer: reverse print fields use a layer limited to the field instead of a label-sized bitmap.
- Viewer: `Draw`/`DrawSurface` validate the label size and density (`ArgumentOutOfRangeException`); elements without a
  drawer or failing during drawer selection are logged and skipped.
- Viewer: fields of a stored format (`^DF`) that supply their own `^FN`/`^FD` data print that data as default when the
  recalling label gives none; data of the recalling label overrides it.
- Viewer: reverse print `^FB` text is drawn normally into the reverse layer, so overlapping lines no longer cancel each
  other out; the other reverse drawers follow the same rule.
- Viewer: `ZplElementDrawer` is `IDisposable` and disposes the `FontManager` of the default options it creates (not of
  caller-provided options); `FontManager.TypefaceGS` is created lazily and a disposed `FontManager` throws
  `ObjectDisposedException`. Disposing while a draw is running rejects new draws, lets the running draw finish and
  releases the font manager when the last draw ends.
- Linux: the packages reference `SkiaSharp.NativeAssets.Linux` instead of the `NoDependencies` variant, which
  has no fontconfig support. With it the viewer found no system font on Linux and drew outline text (font `0`,
  `^A0`) blank. Linux now needs `libfontconfig1`.
- Viewer: rotated barcodes and field blocks with an odd size were moved one dot to the left or top, because the
  rotation center was computed with integer division.
- Viewer: the `SKFont` of `^GS` graphic symbols is disposed.
- CI: CodeQL runs the `security-extended` queries on the shipped code; third-party actions of the release
  workflow are pinned to commit SHAs.
- Viewer: an omitted format name (`^DF^FS`, `^XF^FS`) is stored and recalled as `UNKNOWN`, like on the printer.
- Viewer: `~DG` stores the graphic as `NAME.GRF` (an omitted name as `UNKNOWN.GRF`), and `^XG` accepts a name
  without extension or without name (`^XGR:LOGO`, `^XGR:`); an invalid magnification keeps 1.
- Viewer: `~DY` stores the object under its name and the extension of the `x` parameter (`~DYR:LOGO,P,P,...` as
  `LOGO.PNG`; codes `B`, `C`, `E`, `F`, `H`, `NRD`, `P`, `PAC`, `T`, `X` and `.GRF` for any other value), so
  `^IMR:LOGO.PNG` finds it; `^XG` and `^IM` elements created in code find parsed downloads by their
  `FileName`.
- Viewer: `^GF` defaults the format to A when omitted; format B (binary) is rejected with an error instead of being
  decoded as hexadecimal.
- Viewer: centered, right aligned and justified `^FB` lines 2+ respect the hanging indent.
- Viewer: `^BQ` with a model other than 1 or 2 or a mask outside 0 - 7 uses the defaults 2 / 7.
- Viewer: `^A@` (font by name) and invalid `^A`/`^CF` font names keep the current font instead of failing the field;
  lowercase font names are accepted; a bare `^A` no longer throws.

### Changed (viewer API)

- `ZplPositionedElementBase.FieldJustification` and `UseDefaultPositionX`; optional `fieldJustification` parameter on
  `ZplGraphicBox/Circle/Ellipse/DiagonalLine/Field`, `ZplImageMove`, `ZplRecallGraphic`, `ZplFieldBlock`; optional
  `useDefaultPositionX` on `ZplTextField`, `ZplFieldBlock`, `ZplFieldTypeset`; `LabelPosition.UseDefaultPositionX`.
- `ZplGraphicField.Data` holds the raw ZPL graphic data instead of a hex encoded PNG.
- `FontManager` implements `IDisposable`; `ZplElementDrawer` implements `IDisposable`.

### Removed

- `BinaryKits.Zpl.Protocol`, `BinaryKits.Zpl.Labelary`, the viewer web API and Docker image.
- `ImageSharpImageConverter`, `DrawerOptions.FontLoader` (use `DrawerOptions.FontManager.FontLoader`).
- .NET Framework / .NET Standard code paths.

[Unreleased]: https://github.com/bitbeans/Bitbeans.Zpl/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/bitbeans/Bitbeans.Zpl/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/bitbeans/Bitbeans.Zpl/releases/tag/v1.0.0
