
namespace Bitbeans.Zpl.Viewer.Tests
{
    [TestClass]
    public class StringHelperTest
    {
        /// <summary>
        /// Test for label containing extended ascii character
        /// </summary>
        [TestMethod]
        public void ExtendedASCIICharacter()
        {
            string zplString = Common.LoadZPL("extended_ascii");
            Common.DefaultPrint(zplString, "extended_ascii.png");
        }
    }
}
