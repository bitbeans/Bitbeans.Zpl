namespace Bitbeans.Zpl.Viewer
{
    /// <summary>Storage for objects downloaded to the printer (for example by <c>~DG</c> and <c>~DY</c>), by storage device and file name.</summary>
    public interface IPrinterStorage
    {
        /// <summary>Stores a file, replacing an existing file of the same name.</summary>
        /// <param name="storageDevice">The storage device letter (for example <c>R</c> or <c>E</c>).</param>
        /// <param name="fileName">The name of the stored file.</param>
        /// <param name="data">The file content.</param>
        void AddFile(char storageDevice, string fileName, byte[] data);
        /// <summary>Gets a stored file.</summary>
        /// <param name="storageDevice">The storage device letter (for example <c>R</c> or <c>E</c>).</param>
        /// <param name="fileName">The name of the stored file.</param>
        /// <returns>The file content, or an empty array if the file does not exist.</returns>
        byte[] GetFile(char storageDevice, string fileName);
    }
}
