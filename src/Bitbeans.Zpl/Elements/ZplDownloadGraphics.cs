using Bitbeans.Zpl.Helpers;
using Bitbeans.Zpl.ImageConverters;
using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>
    /// Download Graphics<br/>
    /// The ~DG command downloads an ASCII Hex representation of a graphic image.
    /// If .GRF is not the specified file extension, .GRF is automatically appended.
    /// </summary>
    /// <remarks>
    /// Format:~DGd:o.x,t,w,data
    /// d = device to store image
    /// o = image name
    /// x = extension
    /// t = total number of bytes in graphic
    /// w = number of bytes per row
    /// data = ASCII hexadecimal string defining image
    /// </remarks>
    public class ZplDownloadGraphics : ZplDownload
    {
        /// <summary>Name of the image on the printer, at most 8 characters; the extension is always <c>.GRF</c>.</summary>
        public string ImageName { get; private set; }
        private string _extension { get; set; }
        /// <summary>The encoded source image (PNG, JPEG, BMP, ...).</summary>
        public byte[] ImageData { get; private set; }

        private readonly IImageConverter _imageConverter;
        readonly ZplCompressionScheme _compressionScheme;

        /// <summary>Creates a graphic download.</summary>
        /// <param name="storageDevice">Storage device (<c>R</c>, <c>E</c>, <c>B</c>, <c>A</c>).</param>
        /// <param name="imageName">Image name, maximum 8 characters.</param>
        /// <param name="imageData">Encoded image data (PNG, JPEG, BMP, ...).</param>
        /// <param name="imageConverter">Converter used to create the bitonal image, e.g. <c>Bitbeans.Zpl.SkiaSharp.SkiaImageConverter</c>.</param>
        /// <param name="compressionScheme">Compression of the graphic data; default <see cref="ZplCompressionScheme.ACS"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="imageName"/>, <paramref name="imageData"/> or <paramref name="imageConverter"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="imageName"/> is longer than 8 characters.</exception>
        public ZplDownloadGraphics(
            char storageDevice,
            string imageName,
            byte[] imageData,
            IImageConverter imageConverter,
            ZplCompressionScheme compressionScheme = ZplCompressionScheme.ACS)
            : base(storageDevice)
        {
            ArgumentNullException.ThrowIfNull(imageName);
            ArgumentNullException.ThrowIfNull(imageData);
            ArgumentNullException.ThrowIfNull(imageConverter);

            if (imageName.Length > 8)
            {
                throw new ArgumentException("maximum length of 8 characters exceeded", nameof(imageName));
            }

            _extension = "GRF"; //Fixed

            ImageName = imageName;
            ImageData = imageData;
            _imageConverter = imageConverter;
            _compressionScheme = compressionScheme;
        }

        /// <summary>Converts the image to a bitonal image (scaled by <see cref="ZplRenderOptions.ScaleFactor"/> first if it is not 1), compresses it and renders <c>~DGd:o.GRF,t,w,</c> and the data as two lines.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The <c>~DG</c> command line and the data line.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            byte[] objectData = context.ScaleFactor != 1
                ? _imageConverter.ConvertToPng(ImageData, context.ScaleFactor)
                : ImageData;

            var imageResult = _imageConverter.ConvertImage(objectData);
            string zplData = string.Empty;

            switch (_compressionScheme)
            {
                case ZplCompressionScheme.None:
                    zplData = imageResult.RawData.ToHexFromBytes();
                    break;
                case ZplCompressionScheme.ACS:
                    zplData = ZebraACSCompressionHelper.Compress(imageResult.RawData.ToHexFromBytes(), imageResult.BytesPerRow);
                    break;
                case ZplCompressionScheme.Z64:
                    //TODO: Reduce multiple conversions of byte array to string. 
                    zplData = ZebraZ64CompressionHelper.Compress(imageResult.RawData);
                    break;
                case ZplCompressionScheme.B64:
                    //TODO: Implement this compression scheme.
                    zplData = ZebraB64CompressionHelper.Compress(imageResult.RawData);
                    break;
                    //throw new NotSupportedException();
            }

            return new List<string>
            {
                $"~DG{StorageDevice}:{ImageName}.{_extension},{imageResult.BinaryByteCount},{imageResult.BytesPerRow},",
                zplData
            };
        }
    }
}
