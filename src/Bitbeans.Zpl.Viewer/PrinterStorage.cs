using System.Collections.Concurrent;

namespace Bitbeans.Zpl.Viewer
{
    /// <summary>Thread-safe in-memory <see cref="IPrinterStorage"/>.</summary>
    public class PrinterStorage : IPrinterStorage
    {
        private readonly ConcurrentDictionary<char, ConcurrentDictionary<string, byte[]>> cache;

        /// <summary>Initializes a new, empty instance of the <see cref="PrinterStorage"/> class.</summary>
        public PrinterStorage()
        {
            this.cache = new ConcurrentDictionary<char, ConcurrentDictionary<string, byte[]>>();
        }

        ///<inheritdoc/>
        public void AddFile(char storageDevice, string fileName, byte[] data)
        {
            ConcurrentDictionary<string, byte[]> files = this.cache.GetOrAdd(storageDevice, _ => new ConcurrentDictionary<string, byte[]>());
            files[fileName] = data;
        }

        ///<inheritdoc/>
        public byte[] GetFile(char storageDevice, string fileName)
        {
            if (this.cache.TryGetValue(storageDevice, out ConcurrentDictionary<string, byte[]> files)
                && files.TryGetValue(fileName, out byte[] data))
            {
                return data ?? [];
            }

            return [];
        }
    }
}
