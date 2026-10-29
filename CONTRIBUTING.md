# Contributing

Thanks for helping to improve Bitbeans.Zpl! Issues and pull requests are welcome.

## Prerequisites

- .NET SDK 10.0.400 or a later 10.0 feature band ([`global.json`](global.json) rolls forward with `latestFeature`)
- PowerShell 7 for the coverage script

## Build and test

```shell
dotnet restore
dotnet build Bitbeans.Zpl.slnx -c Release
dotnet test Bitbeans.Zpl.slnx -c Release
```

Coverage report and threshold check (the CI runs the same script):

```shell
dotnet tool restore
pwsh ./eng/coverage.ps1
```

Package audit: vulnerable or deprecated packages and licenses other than MIT, Apache-2.0, BSD or MS-PL fail
the check (the CI runs it on every push and once a week):

```shell
pwsh ./eng/check-packages.ps1
```

The HTML report is written to `artifacts/coverage/index.html`.

## Repository layout

| Path | Content |
| --- | --- |
| `src/Bitbeans.Zpl` | Core package, must stay free of dependencies |
| `src/Bitbeans.Zpl.SkiaSharp` | `IImageConverter` implementation |
| `src/Bitbeans.Zpl.Viewer` | ZPL parser (`CommandAnalyzers`) and renderer (`ElementDrawers`) |
| `tests/*` | MSTest projects, one per package |
| `docs/` | User documentation |
| `eng/` | Build scripts |

Package versions are managed centrally in [`Directory.Packages.props`](Directory.Packages.props),
shared build settings live in [`Directory.Build.props`](Directory.Build.props).

## Guidelines

- **Tests are required.** Every change needs tests; the coverage gate requires 100 % line and branch
  coverage. Assert behavior (ZPL output, decoded barcodes, pixel regions), not just execution.
- Warnings are errors. Keep public APIs documented with XML comments.
- Follow the existing code style ([`.editorconfig`](.editorconfig)).
- Viewer behavior should match a real ZPL printer and the
  [ZPL II programming guide](https://www.zebra.com/content/dam/zebra/manuals/printers/common/programming/zpl-zbi2-pm-en.pdf),
  not a particular online viewer. Reference the relevant command in your pull request.
- Add an entry to [`CHANGELOG.md`](CHANGELOG.md) under *Unreleased*.

## Adding a ZPL command to the viewer

1. Add or extend the element in `src/Bitbeans.Zpl/Elements` (with rendering back to ZPL).
2. Add a command analyzer in `src/Bitbeans.Zpl.Viewer/CommandAnalyzers` and register it in `ZplAnalyzer`.
3. Add a drawer in `src/Bitbeans.Zpl.Viewer/ElementDrawers` and register it in `ZplElementDrawer`.
4. Cover parsing, ZPL output and rendering with tests and update [`docs/viewer-support.md`](docs/viewer-support.md).

## Releases

Maintainers push a tag `vX.Y.Z` (or `vX.Y.Z-preview.N`). The release workflow builds and tests the code, then
publishes the packages to NuGet and creates a GitHub release. Publishing uses
[NuGet trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing), so no API key is
stored in the repository. The `nuget` environment can require a manual approval.
