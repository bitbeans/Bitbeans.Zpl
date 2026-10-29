namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The parameters of a MaxiCode barcode (<c>^BD</c>) for the next field.</summary>
    public class MaxiCodeBarcodeFieldData : FieldDataBase
    {
        /// <summary>Gets or sets the mode (<c>m</c> parameter); the default is 2.</summary>
        public int Mode { get; set; }
        /// <summary>Gets or sets the position of the symbol in a structured append (<c>n</c> parameter); the default is 1.</summary>
        public int Position { get; set; }
        /// <summary>Gets or sets the total number of symbols in a structured append (<c>t</c> parameter); the default is 1.</summary>
        public int Total { get; set; }
    }
}
