namespace Bitbeans.Zpl
{
    /// <summary>Quality level (error correction) of a Data Matrix symbol (<see cref="Elements.ZplDataMatrix"/>, <c>^BX</c>). The member name is the rendered value.</summary>
    public enum QualityLevel
    {
        /// <summary>ECC 0 (<c>0</c>), the legacy convolutional error correction level.</summary>
        ECC0,
        /// <summary>ECC 50 (<c>50</c>).</summary>
        ECC50,
        /// <summary>ECC 80 (<c>80</c>).</summary>
        ECC80,
        /// <summary>ECC 100 (<c>100</c>).</summary>
        ECC100,
        /// <summary>ECC 140 (<c>140</c>).</summary>
        ECC140,
        /// <summary>ECC 200 (<c>200</c>), Reed-Solomon error correction; the recommended level.</summary>
        ECC200
    }
}
