# Bitbeans.Zpl.SkiaSharp

SkiaSharp based `IImageConverter` for [Bitbeans.Zpl](https://www.nuget.org/packages/Bitbeans.Zpl):
convert PNG, JPEG, BMP, GIF or WebP images into Zebra graphics - **without ImageSharp**.

```csharp
using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.SkiaSharp;

var converter = new SkiaImageConverter(threshold: 128);

var engine = new ZplEngine
{
    new ZplDownloadGraphics('R', "LOGO", File.ReadAllBytes("logo.png"), converter, ZplCompressionScheme.Z64),
    new ZplRecallGraphic(50, 50, 'R', "LOGO"),
};

string zpl = engine.ToZplString(new ZplRenderOptions());
```

- Transparent pixels are composed onto white
- Images are scaled with a Mitchell cubic filter when source and target dpi differ
- Linux native assets (`SkiaSharp.NativeAssets.Linux`) are included; they need `libfontconfig1`, which is
  installed on most distributions

Documentation: https://github.com/bitbeans/Bitbeans.Zpl/blob/main/docs/images.md
