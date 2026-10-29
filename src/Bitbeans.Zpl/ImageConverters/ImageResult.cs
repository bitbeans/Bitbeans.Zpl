namespace Bitbeans.Zpl.ImageConverters
{
    /// <summary>
    /// Bitonal image (GRF) produced by an <see cref="IImageConverter"/>
    /// </summary>
    public class ImageResult
    {
        /// <summary>
        /// Packed pixel data, one bit per pixel, most significant bit first, 1 = black
        /// </summary>
        public byte[] RawData { get; set; }

        /// <summary>
        /// Total number of bytes of <see cref="RawData"/>
        /// </summary>
        public int BinaryByteCount { get; set; }

        /// <summary>
        /// Number of bytes per image row
        /// </summary>
        public int BytesPerRow { get; set; }
    }
}
