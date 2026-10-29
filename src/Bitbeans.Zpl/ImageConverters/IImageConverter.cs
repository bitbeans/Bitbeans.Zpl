namespace Bitbeans.Zpl.ImageConverters
{
    /// <summary>
    /// Converts raster images for the download commands (~DG, ~DY).
    /// Bitbeans.Zpl itself has no imaging dependency; use an implementation such as
    /// <c>Bitbeans.Zpl.SkiaSharp.SkiaImageConverter</c> or provide your own.
    /// </summary>
    public interface IImageConverter
    {
        /// <summary>
        /// Convert an encoded image (PNG, JPEG, BMP, ...) to a bitonal image (GRF).
        /// </summary>
        /// <param name="imageData">Encoded image data</param>
        /// <returns>The GRF data with its row layout</returns>
        ImageResult ConvertImage(byte[] imageData);

        /// <summary>
        /// Decode an image, scale it by <paramref name="scaleFactor"/> and encode it as PNG.
        /// </summary>
        /// <param name="imageData">Encoded image data</param>
        /// <param name="scaleFactor">Scale factor, 1 keeps the original size</param>
        /// <returns>PNG encoded image data</returns>
        byte[] ConvertToPng(byte[] imageData, double scaleFactor);
    }
}
