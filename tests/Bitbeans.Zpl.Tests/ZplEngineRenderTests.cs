using Bitbeans.Zpl.Elements;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Tests
{
    [TestClass]
    public class ZplEngineRenderTests
    {
        [TestMethod]
        public void RenderOptionsDefaults()
        {
            var options = new ZplRenderOptions();

            Assert.IsTrue(options.AddStartEndFormat);
            Assert.IsTrue(options.AddDefaultLabelHome);
            Assert.IsFalse(options.DisplayComments);
            Assert.IsFalse(options.AddEmptyLineBeforeElementStart);
            Assert.IsFalse(options.CompressedRendering);
            Assert.AreEqual(203, options.SourcePrintDpi);
            Assert.AreEqual(203, options.TargetPrintDpi);
            Assert.AreEqual(1.0, options.ScaleFactor);
        }

        [TestMethod]
        public void RenderOptionsScale()
        {
            var options = new ZplRenderOptions { SourcePrintDpi = 200, TargetPrintDpi = 300 };

            Assert.AreEqual(1.5, options.ScaleFactor);
            Assert.AreEqual(150, options.Scale(100));
            Assert.AreEqual(7, options.Scale(5)); // truncated, not rounded
            Assert.AreEqual(7.5, options.Scale(5.0));
        }

        [TestMethod]
        public void EmptyEngineRendersFormatOnly()
        {
            Assert.AreEqual("^XA\n^LH0,0\n^XZ", new ZplEngine().ToZplString(new ZplRenderOptions()));
        }

        [TestMethod]
        public void StartEndAndLabelHomeCanBeDisabled()
        {
            var engine = new ZplEngine { new ZplRaw("^FDX^FS") };
            var options = new ZplRenderOptions { AddStartEndFormat = false, AddDefaultLabelHome = false };

            Assert.AreEqual("^FDX^FS", engine.ToZplString(options));
        }

        [TestMethod]
        public void RawZplCodeIsAdded()
        {
            var engine = new ZplEngine();
            engine.AddRawZplCode("^FO1,1^FDX^FS");

            Assert.HasCount(1, engine);
            Assert.AreEqual("^XA\n^LH0,0\n^FO1,1^FDX^FS\n^XZ", engine.ToZplString(new ZplRenderOptions()));
        }

        [TestMethod]
        public void DisabledElementsAreSkipped()
        {
            var disabled = new ZplRaw("^DISABLED") { IsEnabled = false };
            var engine = new ZplEngine(new ZplElementBase[] { new ZplRaw("^A"), disabled, new ZplRaw("^B") });

            Assert.AreEqual("^XA\n^LH0,0\n^A\n^B\n^XZ", engine.ToZplString(new ZplRenderOptions()));
        }

        [TestMethod]
        public void EmptyLineBeforeEachElement()
        {
            var engine = new ZplEngine { new ZplRaw("^A"), new ZplRaw("^B") };
            var options = new ZplRenderOptions { AddEmptyLineBeforeElementStart = true };

            Assert.AreEqual("^XA\n^LH0,0\n\n^A\n\n^B\n^XZ", engine.ToZplString(options));
        }

        [TestMethod]
        public void CommentsAreOnlyRenderedWhenRequested()
        {
            var element = new ZplRaw("^A");
            element.Comments.Add("a^b~c");
            element.Comments.Add("second");
            var plain = new ZplRaw("^B");
            var engine = new ZplEngine { element, plain };

            Assert.AreEqual("^XA\n^LH0,0\n^A\n^B\n^XZ", engine.ToZplString(new ZplRenderOptions()));
            Assert.AreEqual(
                "^XA\n^LH0,0\n^FX\n//a[caret]b[tilde]c\n//second\n^A\n^B\n^XZ",
                engine.ToZplString(new ZplRenderOptions { DisplayComments = true }));
        }

        [TestMethod]
        public void CompressedRenderingJoinsElementLines()
        {
            var engine = new ZplEngine { new ZplGraphicBox(10, 20, 30, 40), new ZplTextField("X", 1, 2, new ZplFont()) };
            var options = new ZplRenderOptions { CompressedRendering = true };

            Assert.AreEqual(
                "^XA\n^LH0,0\n^FO10,20^GB30,40,1,B,0^FS\n^A0N,30,30^FO1,2^FDX^FS\n^XZ",
                engine.ToZplString(options));
        }

        [TestMethod]
        public void RenderReturnsLinesWithoutJoining()
        {
            var engine = new ZplEngine { new ZplGraphicBox(10, 20, 30, 40) };
            List<string> lines = engine.Render(new ZplRenderOptions());

            CollectionAssert.AreEqual(new[] { "^XA", "^LH0,0", "^FO10,20", "^GB30,40,1,B,0^FS", "^XZ" }, lines);
        }

        [TestMethod]
        public void MapToHexadecimalValue()
        {
            Assert.AreEqual("41", ZplEngine.MapToHexadecimalValue('A'));
            Assert.AreEqual("0A", ZplEngine.MapToHexadecimalValue('\n'));
            Assert.AreEqual("5F", ZplEngine.MapToHexadecimalValue('_'));
            Assert.AreEqual("FF", ZplEngine.MapToHexadecimalValue('ÿ'));
        }

        [TestMethod]
        public void MapToHexadecimalValueRejectsNonByteCharacters()
        {
            Assert.ThrowsExactly<System.OverflowException>(() => ZplEngine.MapToHexadecimalValue('Ā'));
        }

        [TestMethod]
        public void ConstantsProvideDefaultFont()
        {
            Assert.AreEqual("^A0N,30,30", ZplConstants.Font.Default.ToZplString());
        }
    }
}
