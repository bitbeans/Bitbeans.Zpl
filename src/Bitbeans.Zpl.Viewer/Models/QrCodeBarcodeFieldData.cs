using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The parameters of a QR Code (<c>^BQ</c>) for the next field.</summary>
    public class QrCodeBarcodeFieldData : FieldDataBase
    {
        /// <summary>Gets or sets the model (<c>b</c> parameter); the default is 2.</summary>
        public int Model { get; set; }
        /// <summary>Gets or sets the field orientation (<c>o</c> parameter).</summary>
        public FieldOrientation FieldOrientation { get; set; }
        /// <summary>Gets or sets the magnification factor (<c>c</c> parameter, 1 to 10); the default is 2.</summary>
        public int MagnificationFactor { get; set; }
        /// <summary>Gets or sets the error correction level (<c>d</c> parameter); the default is <see cref="ErrorCorrectionLevel.HighReliability"/>.</summary>
        public ErrorCorrectionLevel ErrorCorrection { get; set; }
        /// <summary>Gets or sets the mask value (<c>e</c> parameter); the default is 7.</summary>
        public int MaskValue { get; set; }
    }
}
