using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The parameters of an EAN-13 barcode (<c>^BE</c>) for the next field.</summary>
    public class CodeEAN13BarcodeFieldData : FieldDataBase
    {
        /// <summary>Gets or sets the field orientation (<c>o</c> parameter).</summary>
        public FieldOrientation FieldOrientation { get; set; }
        /// <summary>Gets or sets the barcode height in dots (<c>h</c> parameter); <see langword="null"/> uses the <c>^BY</c> default.</summary>
        public int? Height { get; set; }
        /// <summary>Gets or sets a value indicating whether the interpretation line is printed (<c>f</c> parameter).</summary>
        public bool PrintInterpretationLine { get; set; }
        /// <summary>Gets or sets a value indicating whether the interpretation line is printed above the code (<c>g</c> parameter).</summary>
        public bool PrintInterpretationLineAboveCode { get; set; }

    }
}
