using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The parameters of a Code 128 barcode (<c>^BC</c>) for the next field.</summary>
    public class Code128BarcodeFieldData : FieldDataBase
    {
        /// <summary>Gets or sets the field orientation (<c>o</c> parameter).</summary>
        public FieldOrientation FieldOrientation { get; set; }
        /// <summary>Gets or sets the barcode height in dots (<c>h</c> parameter); <see langword="null"/> uses the <c>^BY</c> default.</summary>
        public int? Height { get; set; }
        /// <summary>Gets or sets a value indicating whether the interpretation line is printed (<c>f</c> parameter).</summary>
        public bool PrintInterpretationLine { get; set; }
        /// <summary>Gets or sets a value indicating whether the interpretation line is printed above the code (<c>g</c> parameter).</summary>
        public bool PrintInterpretationLineAboveCode { get; set; }
        /// <summary>Gets or sets a value indicating whether the UCC check digit is calculated and printed (<c>e</c> parameter).</summary>
        public bool UccCheckDigit { get; set; }
        /// <summary>Gets or sets the Code 128 mode (<c>m</c> parameter): <c>N</c> (default), <c>U</c>, <c>A</c> or <c>D</c>.</summary>
        public string Mode { get; set; }
    }
}
