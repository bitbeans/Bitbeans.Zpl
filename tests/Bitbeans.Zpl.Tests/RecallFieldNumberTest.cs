using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Tests
{
    [TestClass]
    public class RecallFieldNumberTest
    {
        [TestMethod]
        public void SingleElement()
        {
            var output = new ZplRecallFieldNumber(100, "value").ToZplString();

            Assert.IsNotNull(output);
            Assert.AreEqual("^FN100\n^FDvalue^FS", output);
        }
    }
}