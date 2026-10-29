using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The parameters of a Data Matrix barcode (<c>^BX</c>) for the next field.</summary>
    public class DataMatrixFieldData : FieldDataBase
    {
        /// <summary>Gets or sets the field orientation (<c>o</c> parameter).</summary>
        public FieldOrientation FieldOrientation { get; set; }
        /// <summary>Gets or sets the module height in dots (<c>h</c> parameter); <see langword="null"/> if omitted.</summary>
        public int? Height { get; set; }
        /// <summary>Gets or sets the quality level (<c>s</c> parameter).</summary>
        public QualityLevel QualityLevel { get; set; }
        /// <summary>Gets or sets the number of columns to encode (<c>c</c> parameter); <see langword="null"/> if omitted.</summary>
        public int? Columns { get; set; }
        /// <summary>Gets or sets the number of rows to encode (<c>r</c> parameter); <see langword="null"/> if omitted.</summary>
        public int? Rows { get; set; }
        /// <summary>Gets or sets the format ID (<c>f</c> parameter); the default is 6.</summary>
        public int Format { get; set; } = 6;
        /// <summary>Gets or sets the escape character (<c>g</c> parameter); the default is <c>~</c>.</summary>
        public char EscapeSequence { get; set; } = '~';
        /// <summary>Gets or sets the aspect ratio (<c>a</c> parameter); <see langword="null"/> if omitted.</summary>
        public int? AspectRatio { get; set; }
    }
}
