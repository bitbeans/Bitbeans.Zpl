
namespace Bitbeans.Zpl.Viewer.Tests
{
    [TestClass]
    public class CustomTest
    {
        [TestMethod]
        public void Custom()
        {
            string zplString = Common.LoadZPL("custom");
            Common.DefaultPrint(zplString, "custom.png");
        }
    }
}
