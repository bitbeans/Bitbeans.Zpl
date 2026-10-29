using Bitbeans.Zpl;
using Bitbeans.Zpl.Viewer.Models;

using System.Collections.Generic;

namespace Bitbeans.Zpl.Viewer
{
    /// <summary>The state of a virtual Zebra printer while ZPL is analyzed: positions, fonts, barcode defaults and the pending data of the next field. The command analyzers read and update it.</summary>
    public class VirtualPrinter
    {
        /// <summary>Gets the label home set with <c>^LH</c>, or <see langword="null"/> if none was set.</summary>
        public LabelHome LabelHomePosition { get; private set; }
        /// <summary>Gets the position of the next field set with <c>^FO</c> or <c>^FT</c>.</summary>
        public LabelPosition NextElementPosition { get; private set; }

        /// <summary>
        /// <see langword="true"/> if <c>^FO</c> or <c>^FT</c> was processed for the pending field.
        /// </summary>
        public bool HasNextElementPosition { get; private set; }

        /// <summary>
        /// Y position (including the label home) of the last field that was positioned with explicit
        /// coordinates. Used when the y coordinate of <c>^FT</c> is omitted.
        /// </summary>
        public int LastElementPositionY { get; private set; }
        /// <summary>Gets the barcode or symbol parameters and content of the next field, or <see langword="null"/> if none is pending.</summary>
        public FieldDataBase NextElementFieldData { get; private set; }
        /// <summary>Gets the field block (<c>^FB</c>) of the next field, or <see langword="null"/> if none is pending.</summary>
        public FieldBlock NextElementFieldBlock { get; private set; }
        /// <summary>Gets the default field orientation set with <c>^FW</c>; the default is <see cref="Bitbeans.Zpl.FieldOrientation.Normal"/>.</summary>
        public FieldOrientation FieldOrientation { get; private set; } = FieldOrientation.Normal;
        /// <summary>Gets the default field justification set with <c>^FW</c>; the default is <see cref="Bitbeans.Zpl.FieldJustification.None"/>.</summary>
        public FieldJustification FieldJustification { get; private set; } = FieldJustification.None;
        /// <summary>Gets the default font width in dots set with <c>^CF</c>; 0 if not set.</summary>
        public int FontWidth { get; private set; } = 0;
        /// <summary>Gets the default font height in dots set with <c>^CF</c>; 0 if not set.</summary>
        public int FontHeight { get; private set; } = 0;
        /// <summary>Gets the default font name set with <c>^CF</c>; the default is <c>A</c>.</summary>
        public string FontName { get; private set; } = "A";
        /// <summary>Gets the comments (<c>^FX</c>) collected for the pending field.</summary>
        public List<string> Comments { get; private set; }

        /// <summary>
        /// Override the default font, only for the next element
        /// </summary>
        public FontInfo NextFont { get; private set; }

        /// <summary>Gets a value indicating whether the next field is printed in reverse (<c>^FR</c>).</summary>
        public bool NextElementFieldReverse { get; private set; }
        /// <summary>Gets the hexadecimal escape character (<c>^FH</c>) of the next field, or <see langword="null"/> if none was set.</summary>
        public char? NextElementFieldHexadecimalIndicator { get; private set; }
        /// <summary>Gets the justification of the next field set with <c>^FO</c> or <c>^FT</c>; <see cref="Bitbeans.Zpl.FieldJustification.None"/> if none was set.</summary>
        public FieldJustification NextElementFieldJustification { get; private set; } = FieldJustification.None;
        /// <summary>Gets a value indicating whether all fields are printed in reverse (<c>^LR</c>).</summary>
        public bool LabelReverse { get; private set; }
        /// <summary>Gets the barcode defaults set with <c>^BY</c>.</summary>
        public BarcodeInfo BarcodeInfo { get; private set; }

        /// <summary>Gets the name under which the following label is stored as a format (<c>^DF</c>), or <see langword="null"/> if none was set.</summary>
        public string NextDownloadFormatName { get; private set; }
        /// <summary>Gets the field number (<c>^FN</c>) of the next field, or <see langword="null"/> if none was set.</summary>
        public int? NextFieldNumber { get; private set; }

        private bool nextFontOrientationIsExplicit;

        /// <summary>Initializes a new instance of the <see cref="VirtualPrinter"/> class with the default state.</summary>
        public VirtualPrinter()
        {
            this.BarcodeInfo = new BarcodeInfo();
            this.Comments = [];
        }

        /// <summary>Sets the position of the next field (<c>^FO</c> or <c>^FT</c>). Unless the default position is used, the Y position is remembered as <see cref="LastElementPositionY"/>.</summary>
        /// <param name="x">The X position in dots, including the label home.</param>
        /// <param name="y">The Y position in dots, including the label home.</param>
        /// <param name="calculateFromBottom"><see langword="true"/> for <c>^FT</c> (bottom-left origin).</param>
        /// <param name="useDefaultPosition"><see langword="true"/> to continue at the default position.</param>
        public void SetNextElementPosition(int x, int y, bool calculateFromBottom = false, bool useDefaultPosition = false)
        {
            this.NextElementPosition = new LabelPosition(x, y, calculateFromBottom, useDefaultPosition);
            this.HasNextElementPosition = true;
            if (!useDefaultPosition)
            {
                this.LastElementPositionY = y;
            }
        }

        /// <summary>Resets the position of the next field.</summary>
        public void ClearNextElementPosition()
        {
            this.NextElementPosition = new LabelPosition(0, 0, false, false);
            this.HasNextElementPosition = false;
        }

        /// <summary>Sets the barcode or symbol parameters of the next field; content that was already set is kept.</summary>
        /// <param name="fieldData">The field data.</param>
        public void SetNextElementFieldData(FieldDataBase fieldData)
        {
            if (this.NextElementFieldData != null)
            {
                fieldData.Content = this.NextElementFieldData.Content;
            }
            this.NextElementFieldData = fieldData;
        }

        /// <summary>Sets the content of the next field, creating plain field data if none is pending.</summary>
        /// <param name="content">The field content.</param>
        public void SetNextElementFieldDataContent(string content)
        {
            this.NextElementFieldData ??= new FieldDataBase();
            this.NextElementFieldData.Content = content;
        }

        /// <summary>Clears the field data of the next field.</summary>
        public void ClearNextElementFieldData()
        {
            this.NextElementFieldData = null;
        }

        /// <summary>Sets the field block of the next field.</summary>
        /// <param name="fieldBlock">The field block.</param>
        public void SetNextElementFieldBlock(FieldBlock fieldBlock)
        {
            this.NextElementFieldBlock = fieldBlock;
        }

        /// <summary>Clears the field block of the next field.</summary>
        public void ClearNextElementFieldBlock()
        {
            this.NextElementFieldBlock = null;
        }

        /// <summary>
        /// Overrides the font for the next element only.
        /// </summary>
        /// <param name="fontName">Font name</param>
        /// <param name="fieldOrientation">Field orientation</param>
        /// <param name="fontWidth">Font width</param>
        /// <param name="fontHeight">Font height</param>
        /// <param name="orientationIsExplicit"><see langword="true"/> if <c>^A</c> specified the orientation, so a later
        /// <c>^FW</c> in the same field does not change it</param>
        public void SetNextFont(
            string fontName,
            FieldOrientation fieldOrientation,
            int fontWidth,
            int fontHeight,
            bool orientationIsExplicit = false)
        {
            this.NextFont = new FontInfo(fontName, fieldOrientation, fontWidth, fontHeight);
            this.nextFontOrientationIsExplicit = orientationIsExplicit;
        }

        /// <summary>Clears the font override of the next field.</summary>
        public void ClearNextFont()
        {
            this.NextFont = null;
            this.nextFontOrientationIsExplicit = false;
        }

        /// <summary>Marks the next field to be printed in reverse.</summary>
        public void SetNextElementFieldReverse()
        {
            this.NextElementFieldReverse = true;
        }

        /// <summary>Clears the reverse print flag of the next field.</summary>
        public void ClearNextElementFieldReverse()
        {
            this.NextElementFieldReverse = false;
        }

        /// <summary>Sets the hexadecimal escape character of the next field.</summary>
        /// <param name="replaceChar">The escape character.</param>
        public void SetNextElementFieldHexadecimalIndicator(char replaceChar)
        {
            this.NextElementFieldHexadecimalIndicator = replaceChar;
        }

        /// <summary>Clears the hexadecimal escape character of the next field.</summary>
        public void ClearNextElementFieldHexadecimalIndicator()
        {
            this.NextElementFieldHexadecimalIndicator = null;
        }

        /// <summary>Sets the justification of the next field.</summary>
        /// <param name="fieldJustification">The justification.</param>
        public void SetNextElementFieldJustification(FieldJustification fieldJustification)
        {
            this.NextElementFieldJustification = fieldJustification;
        }

        /// <summary>Resets the justification of the next field to <see cref="Bitbeans.Zpl.FieldJustification.None"/>.</summary>
        public void ClearNextElementFieldJustification()
        {
            this.NextElementFieldJustification = FieldJustification.None;
        }

        /// <summary>Sets whether all fields are printed in reverse (<c>^LR</c>).</summary>
        /// <param name="reverse"><see langword="true"/> to print in reverse.</param>
        public void SetLabelReverse(bool reverse)
        {
            this.LabelReverse = reverse;
        }

        /// <summary>Sets the default field orientation (<c>^FW</c>). A pending font override (<c>^A</c>) without an explicit orientation follows the new orientation.</summary>
        /// <param name="fieldOrientation">The orientation.</param>
        public void SetFieldOrientation(FieldOrientation fieldOrientation)
        {
            this.FieldOrientation = fieldOrientation;
            if (this.NextFont != null && !this.nextFontOrientationIsExplicit)
            {
                this.SetNextFont(this.NextFont.FontName, fieldOrientation, this.NextFont.FontWidth, this.NextFont.FontHeight);
            }
        }

        /// <summary>Sets the default field justification (<c>^FW</c>).</summary>
        /// <param name="fieldJustification">The justification.</param>
        public void SetFieldJustification(FieldJustification fieldJustification)
        {
            this.FieldJustification = fieldJustification;
        }

        /// <summary>Sets the default font width (<c>^CF</c>).</summary>
        /// <param name="fontWidth">The width in dots.</param>
        public void SetFontWidth(int fontWidth)
        {
            this.FontWidth = fontWidth;
        }

        /// <summary>Sets the default font height (<c>^CF</c>).</summary>
        /// <param name="fontHeight">The height in dots.</param>
        public void SetFontHeight(int fontHeight)
        {
            this.FontHeight = fontHeight;
        }

        /// <summary>Sets the default font name (<c>^CF</c>).</summary>
        /// <param name="fontName">The font name.</param>
        public void SetFontName(string fontName)
        {
            this.FontName = fontName;
        }

        /// <summary>Sets the default barcode module width (<c>^BY</c>).</summary>
        /// <param name="moduleWidth">The module width in dots.</param>
        public void SetBarcodeModuleWidth(int moduleWidth)
        {
            this.BarcodeInfo.ModuleWidth = moduleWidth;
        }

        /// <summary>Sets the default wide bar to narrow bar width ratio (<c>^BY</c>).</summary>
        /// <param name="wideBarToNarrowBarWidthRatio">The ratio.</param>
        public void SetBarcodeWideBarToNarrowBarWidthRatio(double wideBarToNarrowBarWidthRatio)
        {
            this.BarcodeInfo.WideBarToNarrowBarWidthRatio = wideBarToNarrowBarWidthRatio;
        }

        /// <summary>Sets the default barcode height (<c>^BY</c>).</summary>
        /// <param name="height">The height in dots.</param>
        public void SetBarcodeHeight(int height)
        {
            this.BarcodeInfo.Height = height;
        }

        /// <summary>Sets the label home (<c>^LH</c>).</summary>
        /// <param name="x">The X offset in dots.</param>
        /// <param name="y">The Y offset in dots.</param>
        public void SetLabelHome(int x, int y)
        {
            this.LabelHomePosition = new LabelHome(x, y);
        }

        /// <summary>Adds a comment (<c>^FX</c>) to the pending field.</summary>
        /// <param name="comment">The comment text.</param>
        public void AddComment(string comment)
        {
            this.Comments.Add(comment);
        }

        /// <summary>Removes all collected comments.</summary>
        public void ClearComments()
        {
            this.Comments.Clear();
        }

        /// <summary>Sets the name under which the following label is stored as a format (<c>^DF</c>).</summary>
        /// <param name="name">The format name including the storage device.</param>
        public void SetNextDownloadFormatName(string name)
        {
            this.NextDownloadFormatName = name;
        }

        /// <summary>Clears the format name set by <see cref="SetNextDownloadFormatName"/>.</summary>
        public void ClearNextDownloadFormatName()
        {
            this.NextDownloadFormatName = null;
        }

        /// <summary>Sets the field number (<c>^FN</c>) of the next field.</summary>
        /// <param name="number">The field number.</param>
        public void SetNextFieldNumber(int number)
        {
            this.NextFieldNumber = number;
        }

        /// <summary>Clears the field number of the next field.</summary>
        public void ClearNextFieldNumber()
        {
            this.NextFieldNumber = null;
        }
    }
}
