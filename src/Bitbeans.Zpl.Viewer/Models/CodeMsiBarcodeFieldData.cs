using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models;

/// <summary>The parameters of an MSI barcode (<c>^BM</c>) for the next field.</summary>
public class CodeMsiBarcodeFieldData : FieldDataBase
{
    /// <summary>Gets or sets the field orientation (<c>o</c> parameter).</summary>
    public FieldOrientation FieldOrientation { get; set; }
    /// <summary>Gets or sets the check digit selection (<c>e</c> parameter, <c>A</c> to <c>D</c>).</summary>
    public MsiBarcodeCheckDigitMode CheckDigitSelection { get; set; }
    /// <summary>Gets or sets the barcode height in dots (<c>h</c> parameter); <see langword="null"/> uses the <c>^BY</c> default.</summary>
    public int? Height { get; set; }
    /// <summary>Gets or sets a value indicating whether the interpretation line is printed (<c>f</c> parameter).</summary>
    public bool PrintInterpretationLine { get; set; }
    /// <summary>Gets or sets a value indicating whether the interpretation line is printed above the code (<c>g</c> parameter).</summary>
    public bool PrintInterpretationLineAboveCode { get; set; }
    /// <summary>Gets or sets a value indicating whether the check digit is inserted into the interpretation line (sixth parameter of <c>^BM</c>).</summary>
    public bool PrintInterpretationLineWithCheckDigit { get; set; }
}
