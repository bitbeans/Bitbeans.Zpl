using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The parameters of an Aztec Code (<c>^BO</c>) for the next field.</summary>
    public class AztecBarcodeFieldData : FieldDataBase
    {
        /// <summary>Gets or sets the field orientation (<c>o</c> parameter).</summary>
        public FieldOrientation FieldOrientation { get; set; }
        /// <summary>Gets or sets the magnification factor (<c>b</c> parameter); the default is 2.</summary>
        public int MagnificationFactor { get; set; }
        /// <summary>Gets or sets a value indicating whether extended channel interpretation is enabled (<c>c</c> parameter).</summary>
        public bool ExtendedChannel { get; set; }
        /// <summary>Gets or sets the error control and symbol size/type (<c>d</c> parameter); the default is 0.</summary>
        public int ErrorControl { get; set; }
        /// <summary>Gets or sets a value indicating whether the symbol is a menu symbol (<c>e</c> parameter).</summary>
        public bool MenuSymbol { get; set; }
        /// <summary>Gets or sets the number of symbols for structured append (<c>f</c> parameter); the default is 1.</summary>
        public int SymbolCount { get; set; }
        /// <summary>Gets or sets the ID field for structured append (<c>g</c> parameter).</summary>
        public string IdField { get; set; }

    }
}
