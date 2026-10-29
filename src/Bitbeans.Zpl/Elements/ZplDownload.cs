namespace Bitbeans.Zpl.Elements
{
    /// <summary>Base class of the commands that download objects (graphics, fonts) to the printer.</summary>
    public abstract class ZplDownload : ZplElementBase
    {
        /// <summary>Storage device: <c>R</c> (DRAM), <c>E</c> (flash), <c>B</c> (memory card) or <c>A</c> (auto).</summary>
        public char StorageDevice { get; private set; }

        /// <summary>Initializes the download command.</summary>
        /// <param name="storageDevice">Storage device: <c>R</c>, <c>E</c>, <c>B</c> or <c>A</c>; <c>Z</c> (firmware) is read-only.</param>
        /// <exception cref="System.ArgumentException"><paramref name="storageDevice"/> is not one of <c>R</c>, <c>E</c>, <c>B</c>, <c>A</c>.</exception>
        public ZplDownload(char storageDevice)
        {
            StorageDevice = Helpers.ZplIdentifier.DownloadDevice(storageDevice, nameof(storageDevice));
        }
    }
}
