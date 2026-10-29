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
  check digit, EAN-8, MSI, LOGMARS, Industrial 2 of 5, Aztec (`^B0`), MaxiCode, all Data Matrix parameters,
  diagonal lines, ellipses, field block justification, background color option and more.

### Changed

- `Bitbeans.Zpl` has **no dependencies**; ImageSharp was removed from all packages.
- Targets .NET 10 only.
- `ZplDownloadGraphics` and `ZplDownloadObjects` require an `IImageConverter`.
- Viewer: fields are finalized at `^FS` instead of `^FD`
  ([upstream #332](https://github.com/BinaryKits/BinaryKits.Zpl/pull/332), extended to all symbologies).
- Transparent image pixels are converted to white instead of black.
- Z64 compression uses `ZLibStream`.

### Fixed

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
