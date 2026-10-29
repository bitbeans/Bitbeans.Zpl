using Bitbeans.Zpl.Elements;
using System;
using System.Linq;

namespace Bitbeans.Zpl.Tests
{
    /// <summary>Omitted object names (the printer uses UNKNOWN), graphic names with extension and download devices.</summary>
    [TestClass]
    public class OmittedNameTests
    {
        private static readonly FakeImageConverter Converter = new FakeImageConverter();

        [TestMethod]
        public void DownloadGraphicsWithoutNameUsesUnknown()
        {
            string zpl = new ZplDownloadGraphics('R', string.Empty, [1], Converter).ToZplString();

            StringAssert.StartsWith(zpl, "~DGR:UNKNOWN.GRF,");
        }

        [TestMethod]
        public void DownloadObjectsWithoutNameUsesUnknown()
        {
            ZplDownloadObjects download = new('E', string.Empty, [1], Converter);

            Assert.AreEqual("UNKNOWN", download.ObjectName);
            StringAssert.StartsWith(download.ToZplString(), "~DYE:UNKNOWN,P,P,");
        }

        [TestMethod]
        [DataRow("LOGO", "LOGO", "LOGO.GRF")]
        [DataRow("SAMPLE.GRF", "SAMPLE.GRF", "SAMPLE.GRF")]
        [DataRow("ABCDEFGH.GRF", "ABCDEFGH.GRF", "ABCDEFGH.GRF")]
        [DataRow("", "UNKNOWN", "UNKNOWN.GRF")]
        [DataRow(".GRF", "UNKNOWN.GRF", "UNKNOWN.GRF")]
        public void RecallGraphicAcceptsNamesWithAndWithoutExtension(string name, string imageName, string fileName)
        {
            ZplRecallGraphic recall = new(10, 20, 'R', name, 2, 3);

            Assert.AreEqual(imageName, recall.ImageName);
            Assert.AreEqual(fileName, recall.FileName);
            Assert.AreEqual($"^XGR:{fileName},2,3^FS", recall.Render(new ZplRenderOptions()).Last());
        }

        [TestMethod]
        [DataRow("ABCDEFGHI")]
        [DataRow("ABCDEFGHI.GRF")]
        [DataRow("LOGO.")]
        [DataRow("A.B.GRF")]
        public void RecallGraphicRejectsInvalidGraphicNames(string name)
        {
            var ex = Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallGraphic(0, 0, 'R', name));

            Assert.AreEqual("imageName", ex.ParamName);
        }

        [TestMethod]
        [DataRow('R')]
        [DataRow('E')]
        [DataRow('B')]
        [DataRow('A')]
        public void DownloadsAcceptWritableDevices(char device)
        {
            Assert.AreEqual(device, new ZplDownloadGraphics(device, "A", [1], Converter).StorageDevice);
            Assert.AreEqual(device, new ZplDownloadObjects(device, "A.PNG", [1], Converter).StorageDevice);
        }

        [TestMethod]
        public void FontIdentifierStillRequiresAFileName()
        {
            var ex = Assert.ThrowsExactly<ArgumentException>(() => new ZplFontIdentifier('E', "R", string.Empty));

            Assert.AreEqual("fontFileName", ex.ParamName);
        }

        [TestMethod]
        public void RecallsStillAcceptTheFirmwareDevice()
        {
            Assert.AreEqual('Z', new ZplRecallGraphic(0, 0, 'Z', "A").StorageDevice);
            Assert.AreEqual('Z', new ZplImageMove(0, 0, 'Z', "A", "GRF").StorageDevice);
        }
    }
}
