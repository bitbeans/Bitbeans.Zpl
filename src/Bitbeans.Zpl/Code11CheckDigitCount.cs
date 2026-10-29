namespace Bitbeans.Zpl;

/// <summary>Number of check digits of a Code 11 bar code (<see cref="Elements.ZplBarcode11"/>).</summary>
public enum Code11CheckDigitCount
{
    /// <summary>One check digit (rendered as <c>Y</c>).</summary>
    One,
    /// <summary>Two check digits (rendered as <c>N</c>), the ZPL default.</summary>
    Two
}
