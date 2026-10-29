using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl
{
    /// <summary>Commonly used constants.</summary>
    public static class ZplConstants
    {
        /// <summary>Font constants.</summary>
        public static class Font
        {
            /// <summary>Default font: scalable font <c>0</c>, 30 x 30 dots, normal orientation.</summary>
            public static readonly ZplFont Default = new ZplFont(30, 30, "0", FieldOrientation.Normal);
        }
    }
}
