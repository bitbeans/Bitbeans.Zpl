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
        public string ImageName { get; private set; }
        private string _extension { get; set; }
        public byte[] ImageData { get; private set; }

        private readonly IImageConverter _imageConverter;
        readonly ZplCompressionScheme _compressionScheme;

        /// <summary>
        /// Zpl Download Graphics
        /// </summary>
        /// <param name="storageDevice">Storage device (R, E, B, A)</param>
        /// <param name="imageName">Image name, maximum 8 characters</param>
        /// <param name="imageData">Encoded image data (PNG, JPEG, BMP, ...)</param>
        /// <param name="imageConverter">Converter used to create the bitonal image, e.g. Bitbeans.Zpl.SkiaSharp.SkiaImageConverter</param>
        /// <param name="compressionScheme">Compression of the graphic data</param>
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

        ///<inheritdoc/>
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
