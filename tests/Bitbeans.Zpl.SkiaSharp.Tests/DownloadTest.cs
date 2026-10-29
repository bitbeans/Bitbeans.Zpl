using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bitbeans.Zpl.SkiaSharp.Tests
{
    [TestClass]
    public class DownloadTest
    {
        [TestMethod]
        [DeploymentItem(@"ZplData/Zpl.png")]
        [DeploymentItem(@"ZplData/DownloadGraphicsACS.txt")]
        public void DownloadGraphicsACS()
        {
            var imageData = File.ReadAllBytes("Zpl.png");

            var elements = new List<ZplElementBase>
            {
                new ZplGraphicBox(0, 0, 100, 100, 4),
                new ZplDownloadGraphics('R', "SAMPLE", imageData, new SkiaImageConverter(), ZplCompressionScheme.ACS),
                new ZplRecallGraphic(100, 100, 'R', "SAMPLE")
            };

            var renderEngine = new ZplEngine(elements);
            var output = renderEngine.ToZplString(new ZplRenderOptions
            {
                AddEmptyLineBeforeElementStart = true,
                TargetPrintDpi = 200,
                SourcePrintDpi = 200
            });

            Debug.WriteLine(output);
            Assert.IsNotNull(output);

            var zplData = File.ReadAllText("DownloadGraphicsACS.txt");
            Assert.AreEqual(zplData, output);
        }
        [TestMethod]
        [DeploymentItem(@"ZplData/Zpl.png")]
        [DeploymentItem(@"ZplData/DownloadGraphicsZ64.txt")]
        public void DownloadGraphicsZ64()
        {
            var imageData = File.ReadAllBytes("Zpl.png");

            var elements = new List<ZplElementBase>
            {
                new ZplGraphicBox(0, 0, 100, 100, 4),
                new ZplDownloadGraphics('R', "SAMPLE", imageData, new SkiaImageConverter(), ZplCompressionScheme.Z64),
                new ZplRecallGraphic(100, 100, 'R', "SAMPLE")
            };

            var renderEngine = new ZplEngine(elements);
            var output = renderEngine.ToZplString(new ZplRenderOptions
            {
                AddEmptyLineBeforeElementStart = true,
                TargetPrintDpi = 200,
                SourcePrintDpi = 200
            });

            Debug.WriteLine(output);
            Assert.IsNotNull(output);

            // Deflate output depends on the zlib implementation, so compare the uncompressed graphic
            var zplData = File.ReadAllText("DownloadGraphicsZ64.txt");
            Assert.AreEqual(ExtractHeader(zplData), ExtractHeader(output));
            CollectionAssert.AreEqual(ExtractZ64Payload(zplData), ExtractZ64Payload(output));
        }
        [TestMethod]
        [DeploymentItem(@"ZplData/Zpl.png")]
        [DeploymentItem(@"ZplData/DownloadGraphicsB64.txt")]
        public void DownloadGraphicsB64()
        {
            var imageData = File.ReadAllBytes("Zpl.png");

            var elements = new List<ZplElementBase>
            {
                new ZplGraphicBox(0, 0, 100, 100, 4),
                new ZplDownloadGraphics('R', "SAMPLE", imageData, new SkiaImageConverter(), ZplCompressionScheme.B64),
                new ZplRecallGraphic(100, 100, 'R', "SAMPLE")
            };

            var renderEngine = new ZplEngine(elements);
            var output = renderEngine.ToZplString(new ZplRenderOptions
            {
                AddEmptyLineBeforeElementStart = true,
                TargetPrintDpi = 200,
                SourcePrintDpi = 200
            });

            Debug.WriteLine(output);
            Assert.IsNotNull(output);

            var zplData = File.ReadAllText("DownloadGraphicsB64.txt");
            Assert.AreEqual(zplData, output);
        }

        [TestMethod]
        [DeploymentItem(@"ZplData/Zpl.png")]
        [DeploymentItem(@"ZplData/DownloadObject.txt")]
        public void DownloadObjects()
        {
            var imageData = File.ReadAllBytes("Zpl.png");

            var elements = new List<ZplElementBase>
            {
                new ZplGraphicBox(0, 0, 100, 100, 4),
                new ZplDownloadObjects('R', "SAMPLE.PNG", imageData, new SkiaImageConverter()),
                new ZplImageMove(100, 100, 'R', "SAMPLE", "PNG")
            };

            var renderEngine = new ZplEngine(elements);
            var output = renderEngine.ToZplString(new ZplRenderOptions
            {
                AddEmptyLineBeforeElementStart = true,
                TargetPrintDpi = 300,
                SourcePrintDpi = 200
            });

            Debug.WriteLine(output);
            Assert.IsNotNull(output);

            // PNG encoder output differs between imaging libraries, so compare the decoded image
            var expected = ParseDownloadObject(File.ReadAllText("DownloadObject.txt"));
            var actual = ParseDownloadObject(output);

            Assert.AreEqual(expected.Prefix, actual.Prefix);
            Assert.AreEqual(expected.Suffix, actual.Suffix);

            using var expectedImage = SKBitmap.Decode(expected.Png);
            using var actualImage = SKBitmap.Decode(actual.Png);
            Assert.AreEqual(expectedImage.Width, actualImage.Width);
            Assert.AreEqual(expectedImage.Height, actualImage.Height);

            var converter = new SkiaImageConverter();
            var expectedGrf = converter.ConvertImage(expected.Png).RawData;
            var actualGrf = converter.ConvertImage(actual.Png).RawData;
            int differentBytes = expectedGrf.Where((b, i) => b != actualGrf[i]).Count();
            Assert.IsLessThan(expectedGrf.Length / 20, differentBytes, "Scaled image deviates too much from the reference");
        }

        private static string ExtractHeader(string zpl)
        {
            return zpl.Substring(0, zpl.IndexOf(":Z64:", StringComparison.Ordinal));
        }

        private static byte[] ExtractZ64Payload(string zpl)
        {
            var line = zpl.Split('\n').Single(o => o.StartsWith(":Z64:", StringComparison.Ordinal));
            return ZebraZ64CompressionHelper.Uncompress(line.Trim());
        }

        private static (string Prefix, byte[] Png, string Suffix) ParseDownloadObject(string zpl)
        {
            var match = Regex.Match(zpl, @"^(?<prefix>.*~DYR:SAMPLE\.PNG,P,P,)(?<length>\d+),,(?<data>[0-9A-F]+)(?<suffix>.*)$", RegexOptions.Singleline);
            Assert.IsTrue(match.Success, "~DY command not found");

            var png = match.Groups["data"].Value.ToBytesFromHex();
            Assert.AreEqual(int.Parse(match.Groups["length"].Value), png.Length);
            return (match.Groups["prefix"].Value, png, match.Groups["suffix"].Value);
        }
    }
}
