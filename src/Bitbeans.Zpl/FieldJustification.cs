namespace Bitbeans.Zpl
{
    /// <summary>Field justification, the optional third parameter of the field position commands <c>^FO</c>, <c>^FT</c> and <c>^FW</c>. Not to be confused with <see cref="TextJustification"/> of <c>^FB</c>.</summary>
    public enum FieldJustification
    {
        /// <summary>Justification is not specified; the parameter is omitted and the printer default applies.</summary>
        None,
        /// <summary>Left-justified field (<c>0</c>).</summary>
        Left,
        /// <summary>Right-justified field (<c>1</c>).</summary>
        Right,
        /// <summary>Auto justification, depending on the script direction of the text (<c>2</c>).</summary>
        Auto
    }
}
