using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The parameters of a PDF417 barcode (<c>^B7</c>) for the next field.</summary>
    public class PDF417FieldData : FieldDataBase
    {
        /// <summary>Gets or sets the row height in dots (<c>h</c> parameter); <see langword="null"/> if omitted.</summary>
        public int? Height { get; set; }
        /// <summary>Gets or sets the field orientation (<c>o</c> parameter).</summary>
        public FieldOrientation FieldOrientation { get; set; }
        /// <summary>Gets or sets the number of data columns (<c>c</c> parameter); <see langword="null"/> if omitted.</summary>
        public int? Columns { get; set; }
        /// <summary>Gets or sets the number of rows (<c>r</c> parameter); <see langword="null"/> if omitted.</summary>
        public int? Rows { get; set; }
        /// <summary>Gets or sets a value indicating whether the truncated (compact) form is used (<c>t</c> parameter).</summary>
        public bool Compact { get; set; }
        /// <summary>Gets or sets the security level (<c>s</c> parameter).</summary>
        public int SecurityLevel { get; set; }
    }
}
