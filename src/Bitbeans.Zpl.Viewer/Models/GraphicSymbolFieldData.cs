using Bitbeans.Zpl;

using System;
namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The parameters of a graphic symbol (<c>^GS</c>) for the next field.</summary>
    public class GraphicSymbolFieldData : FieldDataBase
    {
        /// <summary>Gets or sets the field orientation (<c>o</c> parameter).</summary>
        public FieldOrientation FieldOrientation { get; set; }
        /// <summary>Gets or sets the character height in dots (<c>h</c> parameter).</summary>
        public int Height { get; set; }
        /// <summary>Gets or sets the character width in dots (<c>w</c> parameter).</summary>
        public int Width { get; set; }
    }
}
