namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    [TestClass]
    public class PrinterStorageTest
    {
        [TestMethod]
        public void StoredFileCanBeRead()
        {
            PrinterStorage storage = new();

            storage.AddFile('R', "LOGO.GRF", [1, 2, 3]);

            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, storage.GetFile('R', "LOGO.GRF"));
        }

        [TestMethod]
        public void UnknownDeviceHasNoFiles()
        {
            PrinterStorage storage = new();
            storage.AddFile('R', "LOGO.GRF", [1]);

            Assert.IsEmpty(storage.GetFile('E', "LOGO.GRF"));
        }

        [TestMethod]
        public void UnknownFileOnKnownDeviceIsEmpty()
        {
            PrinterStorage storage = new();
            storage.AddFile('R', "LOGO.GRF", [1]);

            Assert.IsEmpty(storage.GetFile('R', "OTHER.GRF"));
        }

        [TestMethod]
        public void FilesOfDifferentDevicesAreSeparate()
        {
            PrinterStorage storage = new();

            storage.AddFile('R', "A.GRF", [1]);
            storage.AddFile('E', "A.GRF", [2]);

            CollectionAssert.AreEqual(new byte[] { 1 }, storage.GetFile('R', "A.GRF"));
            CollectionAssert.AreEqual(new byte[] { 2 }, storage.GetFile('E', "A.GRF"));
        }

        [TestMethod]
        public void SecondFileWithTheSameNameReplacesTheFirst()
        {
            PrinterStorage storage = new();

            storage.AddFile('R', "A.GRF", [1]);
            storage.AddFile('R', "A.GRF", [2]);

            CollectionAssert.AreEqual(new byte[] { 2 }, storage.GetFile('R', "A.GRF"));
        }

        [TestMethod]
        public void NullDataIsReadAsEmpty()
        {
            PrinterStorage storage = new();

            storage.AddFile('R', "A.GRF", null);

            Assert.IsEmpty(storage.GetFile('R', "A.GRF"));
        }
    }
}
