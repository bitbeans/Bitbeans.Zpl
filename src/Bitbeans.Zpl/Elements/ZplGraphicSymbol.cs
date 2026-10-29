using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Graphic symbol (<c>^GS</c>): a registered trade mark, copyright, trade mark, UL or CSA symbol.</summary>
    /// <remarks>
    /// Format: <c>^GSo,h,w</c> followed by the symbol character as field data.
    /// </remarks>
    public class ZplGraphicSymbol : ZplPositionedElementBase
    {
        /// <summary>Orientation of the symbol.</summary>
        public FieldOrientation FieldOrientation { get; private set; }
        /// <summary>Character width in dots (<c>w</c> parameter); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Width { get; private set; }
        /// <summary>Character height in dots (<c>h</c> parameter); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Height { get; private set; }

        /// <summary>The symbol, or <see langword="null"/> for empty field data.</summary>
        public GraphicSymbolCharacter? Character { get; private set; }

        /// <summary>Creates a graphic symbol from a character.</summary>
        /// <param name="text">String whose first character (<c>A</c> to <c>E</c>, see <see cref="GraphicSymbolCharacter"/>) selects the symbol; it must be one of these characters. <see langword="null"/> or empty renders no symbol.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="width">Character width in dots.</param>
        /// <param name="height">Character height in dots.</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="fieldJustification">Field justification rendered as the third parameter of <c>^FO</c>/<c>^FT</c>; <see cref="FieldJustification.None"/> omits it.</param>
        public ZplGraphicSymbol(
            string text,
            int positionX,
            int positionY,
            int width,
            int height,
            FieldOrientation fieldOrientation = FieldOrientation.Normal,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            FieldJustification fieldJustification = FieldJustification.None)
            : base(positionX, positionY, bottomToTop, useDefaultPosition, fieldJustification)
        {
            GraphicSymbolCharacter? character = default;
            if (!string.IsNullOrEmpty(text))
            {
                character = (GraphicSymbolCharacter)text[0];
            }

            Character = ValidateCharacter(character, nameof(text));
            FieldOrientation = fieldOrientation;
            Width = width;
            Height = height;
        }

        /// <summary>Creates a graphic symbol.</summary>
        /// <param name="character">The symbol, or <see langword="null"/> for none.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="width">Character width in dots.</param>
        /// <param name="height">Character height in dots.</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="fieldJustification">Field justification rendered as the third parameter of <c>^FO</c>/<c>^FT</c>; <see cref="FieldJustification.None"/> omits it.</param>
        public ZplGraphicSymbol(
            GraphicSymbolCharacter? character,
            int positionX,
            int positionY,
            int width,
            int height,
            FieldOrientation fieldOrientation = FieldOrientation.Normal,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            FieldJustification fieldJustification = FieldJustification.None)
            : base(positionX, positionY, bottomToTop, useDefaultPosition, fieldJustification)
        {
            Character = ValidateCharacter(character, nameof(character));
            FieldOrientation = fieldOrientation;
            Width = width;
            Height = height;
        }

        private static GraphicSymbolCharacter? ValidateCharacter(GraphicSymbolCharacter? character, string paramName)
        {
            if (character is GraphicSymbolCharacter value && !Enum.IsDefined(value))
            {
                throw new ArgumentException("The character is not a defined graphic symbol (A to E).", paramName);
            }

            return character;
        }

        /// <summary>Renders the position and <c>^GSo,h,w^FDc^FS</c>.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the symbol.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^GSo,h,w
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"^GS{RenderFieldOrientation(FieldOrientation)},{context.Scale(Height)},{context.Scale(Width)}^FD{(char?)Character}^FS");

            return result;
        }
    }
}
