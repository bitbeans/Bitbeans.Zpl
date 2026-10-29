namespace Bitbeans.Zpl
{
    /// <summary>Error correction level of a QR Code (<see cref="Elements.ZplQrCode"/>, <c>^BQ</c>).</summary>
    public enum ErrorCorrectionLevel
    {
        /// <summary>Ultra-high reliability, level H (<c>H</c>), recovers about 30 % of the symbol.</summary>
        UltraHighReliability,
        /// <summary>High reliability, level Q (<c>Q</c>), recovers about 25 % of the symbol.</summary>
        HighReliability,
        /// <summary>Standard reliability, level M (<c>M</c>), recovers about 15 % of the symbol.</summary>
        Standard,
        /// <summary>High density, level L (<c>L</c>), recovers about 7 % of the symbol.</summary>
        HighDensity
    }
}
