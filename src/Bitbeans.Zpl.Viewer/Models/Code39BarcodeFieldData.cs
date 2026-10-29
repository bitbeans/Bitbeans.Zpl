using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The parameters of a Code 39 barcode (<c>^B3</c>) for the next field.</summary>
    public class Code39BarcodeFieldData : FieldDataBase
    {
        /// <summary>Gets or sets the field orientation (<c>o</c> parameter).</summary>
        public FieldOrientation FieldOrientation { get; set; }
        /// <summary>Gets or sets the barcode height in dots (<c>h</c> parameter); <see langword="null"/> uses the <c>^BY</c> default.</summary>
        public int? Height { get; set; }
        /// <summary>Gets or sets a value indicating whether the interpretation line is printed (<c>f</c> parameter).</summary>
        public bool PrintInterpretationLine { get; set; }
        /// <summary>Gets or sets a value indicating whether the interpretation line is printed above the code (<c>g</c> parameter).</summary>
        public bool PrintInterpretationLineAboveCode { get; set; }
        /// <summary>Gets or sets a value indicating whether a Mod 43 check digit is added (<c>e</c> parameter).</summary>
        public bool Mod43CheckDigit { get; set; }
    }
}
