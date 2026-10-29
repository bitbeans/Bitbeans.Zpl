namespace Bitbeans.Zpl;

/// <summary>Check digit selection of an MSI bar code (<see cref="Elements.ZplBarcodeMsi"/>, <c>^BM</c>).</summary>
public enum MsiBarcodeCheckDigitMode
{
    /// <summary>No check digit (rendered as <c>A</c>).</summary>
    None,
    /// <summary>One Modulus 10 check digit (rendered as <c>B</c>, the ZPL default).</summary>
    Mod1_10,
    /// <summary>Two Modulus 10 check digits (rendered as <c>C</c>).</summary>
    Mod2_10,
    /// <summary>One Modulus 11 check digit followed by one Modulus 10 check digit (rendered as <c>D</c>).</summary>
    Mod_1_11_1_10
}
