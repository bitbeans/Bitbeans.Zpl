namespace Bitbeans.Zpl
{
    /// <summary>Symbol characters available in the <c>^GS</c> graphic symbol command (<see cref="Elements.ZplGraphicSymbol"/>). The value is the character sent as field data.</summary>
    public enum GraphicSymbolCharacter
    {
        /// <summary>Registered trade mark symbol (<c>A</c>).</summary>
        RegisteredTradeMark = 'A',
        /// <summary>Copyright symbol (<c>B</c>).</summary>
        Copyright = 'B',
        /// <summary>Trade mark symbol (<c>C</c>).</summary>
        TradeMark = 'C',
        /// <summary>Underwriters Laboratories approval symbol (<c>D</c>).</summary>
        UnderwritersLaboratoriesApproval = 'D', 
        /// <summary>Canadian Standards Association approval symbol (<c>E</c>).</summary>
        CanadianStandardsAssociationApproval = 'E'
    }
}
