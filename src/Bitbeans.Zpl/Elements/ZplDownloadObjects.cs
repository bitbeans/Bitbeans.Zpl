using Bitbeans.Zpl.Helpers;
using Bitbeans.Zpl.ImageConverters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>
    /// Download Graphics / Native TrueType or OpenType Font
    /// The ~DY command downloads to the printer graphic objects or fonts in any supported format.
    /// This command can be used in place of ~DG for more saving and loading options.
    /// ~DY is the preferred command to download TrueType fonts on printers with firmware greater than X.13.
    /// It is faster than ~DU.
    /// </summary>
    /// <remarks>
    /// Format:~DYd:f,b,x,t,w,data
    /// d = file location
    /// f = file name
    /// b = format downloaded in data field
    /// x = extension of stored file
    /// t = total number of bytes in file
    /// w = total number of bytes per row
    /// data = data
    /// </remarks>
    public class ZplDownloadObjects : ZplDownload
    {
        public string ObjectName { get; private set; }
        public byte[] ImageData { get; private set; }

        private readonly IImageConverter _imageConverter;

        /// <summary>
        /// Zpl Download Objects
        /// </summary>
        /// <param name="storageDevice">Storage device (R, E, B, A)</param>
        /// <param name="imageName">Object name</param>
        /// <param name="imageData">Encoded image data (PNG, JPEG, BMP, ...)</param>
        /// <param name="imageConverter">Converter used to create the PNG, e.g. Bitbeans.Zpl.SkiaSharp.SkiaImageConverter</param>
        public ZplDownloadObjects(char storageDevice, string imageName, byte[] imageData, IImageConverter imageConverter)
            : base(storageDevice)
        {
            ArgumentNullException.ThrowIfNull(imageName);
            ArgumentNullException.ThrowIfNull(imageData);
            ArgumentNullException.ThrowIfNull(imageConverter);

            ObjectName = imageName;
            ImageData = imageData;
            _imageConverter = imageConverter;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            byte[] objectData = _imageConverter.ConvertToPng(ImageData, context.ScaleFactor);

            var hexString = ByteHelper.BytesToHex(objectData);

            var formatDownloadedInDataField = 'P'; //portable network graphic (.PNG) - ZB64 encoded 
            var extensionOfStoredFile = 'P'; //store as compressed (.PNG)

            var result = new List<string>
            {
                $"~DY{StorageDevice}:{ObjectName},{formatDownloadedInDataField},{extensionOfStoredFile},{objectData.Length},,{hexString}"
            };

            return result;
        }
    }
}
