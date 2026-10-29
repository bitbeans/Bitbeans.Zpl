namespace Bitbeans.Zpl
{
    /// <summary>Orientation of a field, bar code or font, as used by e.g. <c>^A</c>, <c>^BC</c> and <c>^FW</c>.</summary>
    public enum FieldOrientation
    {
        /// <summary>Normal orientation (<c>N</c>).</summary>
        Normal,
        /// <summary>Rotated 90 degrees clockwise (<c>R</c>).</summary>
        Rotated90,
        /// <summary>Inverted, rotated 180 degrees (<c>I</c>).</summary>
        Rotated180,
        /// <summary>Read from bottom up, rotated 270 degrees clockwise (<c>B</c>).</summary>
        Rotated270
    }
}
