using System.Collections.Generic;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    [TestClass]
    public class FormatMergerTest
    {
        private static ZplTextField Text(string text, int y)
        {
            return new ZplTextField(text, 10, y, ZplConstants.Font.Default);
        }

        private static LabelInfo Format(string name, params ZplElementBase[] elements)
        {
            return new LabelInfo { DownloadFormatName = name, ZplElements = elements };
        }

        private static LabelInfo Label(params ZplElementBase[] elements)
        {
            return new LabelInfo { ZplElements = elements };
        }

        [TestMethod]
        public void LabelsWithoutFormatsAreReturnedAsIs()
        {
            List<LabelInfo> merged = new FormatMerger().MergeFormats([Label(Text("a", 10)), Label(Text("b", 20), Text("c", 30))]);

            Assert.HasCount(2, merged);
            Assert.HasCount(1, merged[0].ZplElements);
            Assert.HasCount(2, merged[1].ZplElements);
        }

        [TestMethod]
        public void FormatsAreNotReturnedAsLabels()
        {
            List<LabelInfo> merged = new FormatMerger().MergeFormats([Format("FMT", Text("a", 10))]);

            Assert.IsEmpty(merged);
        }

        [TestMethod]
        public void FixedElementsOfTheFormatAreKept()
        {
            LabelInfo format = Format("FMT", Text("fixed", 10), new ZplFieldNumber(1, Text(null, 20)));
            LabelInfo data = Label(new ZplRecallFormat("FMT.ZPL"), new ZplRecallFieldNumber(1, "value"));

            List<LabelInfo> merged = new FormatMerger().MergeFormats([format, data]);

            Assert.HasCount(1, merged);
            Assert.HasCount(2, merged[0].ZplElements);
            Assert.AreEqual("fixed", ((ZplTextField)merged[0].ZplElements[0]).Text);
            Assert.AreEqual("value", ((ZplTextField)merged[0].ZplElements[1]).Text);
        }

        [TestMethod]
        public void FieldsWithoutDataAreOmitted()
        {
            LabelInfo format = Format("FMT", new ZplFieldNumber(1, Text(null, 10)), new ZplFieldNumber(2, Text(null, 20)));
            LabelInfo data = Label(new ZplRecallFormat("FMT.ZPL"), new ZplRecallFieldNumber(2, "two"), new ZplRecallFieldNumber(3, "unused"));

            List<LabelInfo> merged = new FormatMerger().MergeFormats([format, data]);

            Assert.HasCount(1, merged[0].ZplElements);
            Assert.AreEqual("two", ((ZplTextField)merged[0].ZplElements[0]).Text);
        }

        [TestMethod]
        public void FormatNameWithStorageDeviceAndExtension()
        {
            LabelInfo format = Format("FMT", Text("fixed", 10));

            List<LabelInfo> merged = new FormatMerger().MergeFormats([format, Label(new ZplRecallFormat("R:FMT.ZPL"))]);

            Assert.HasCount(1, merged[0].ZplElements);
        }

        [TestMethod]
        public void FormatNameWithoutStorageDeviceOrExtension()
        {
            LabelInfo format = Format("FMT", Text("fixed", 10));

            List<LabelInfo> merged = new FormatMerger().MergeFormats([format, Label(new ZplRecallFormat("FMT"))]);

            Assert.HasCount(1, merged[0].ZplElements);
        }

        [TestMethod]
        public void SingleCharacterFormatNameIsNotADevice()
        {
            LabelInfo format = Format("F", Text("fixed", 10));

            List<LabelInfo> merged = new FormatMerger().MergeFormats([format, Label(new ZplRecallFormat("F"))]);

            Assert.HasCount(1, merged[0].ZplElements);
        }

        [TestMethod]
        public void FormatCanBeRecalledBySeveralLabels()
        {
            LabelInfo format = Format("FMT", new ZplFieldNumber(1, Text(null, 10)));
            LabelInfo first = Label(new ZplRecallFormat("FMT.ZPL"), new ZplRecallFieldNumber(1, "one"));
            LabelInfo second = Label(new ZplRecallFormat("FMT.ZPL"), new ZplRecallFieldNumber(1, "two"));

            List<LabelInfo> merged = new FormatMerger().MergeFormats([format, first, second]);

            Assert.HasCount(2, merged);
            Assert.AreEqual("one", ((ZplTextField)merged[0].ZplElements[0]).Text);
            Assert.AreEqual("two", ((ZplTextField)merged[1].ZplElements[0]).Text);
        }

        [TestMethod]
        public void RecallOfAFormatThatIsDownloadedLaterFails()
        {
            Assert.ThrowsExactly<System.InvalidOperationException>(() =>
                new FormatMerger().MergeFormats([Label(new ZplRecallFormat("FMT.ZPL")), Format("FMT", Text("a", 10))]));
        }

        [TestMethod]
        public void FieldNumberDataInTheSameLabelFillsTheMatchingFieldOnly()
        {
            // A label that declares its own ^FN fields: data for a field number replaces that field
            LabelInfo label = Label(
                Text("before", 5),
                new ZplFieldNumber(1, Text(null, 10)),
                new ZplFieldNumber(2, Text(null, 20)),
                new ZplRecallFieldNumber(2, "two"));

            List<LabelInfo> merged = new FormatMerger().MergeFormats([label]);

            ZplElementBase[] elements = merged[0].ZplElements;
            Assert.HasCount(3, elements);
            Assert.AreEqual("before", ((ZplTextField)elements[0]).Text);
            Assert.IsInstanceOfType<ZplFieldNumber>(elements[1]);
            Assert.AreEqual("two", ((ZplTextField)elements[2]).Text);
        }
    }
}
