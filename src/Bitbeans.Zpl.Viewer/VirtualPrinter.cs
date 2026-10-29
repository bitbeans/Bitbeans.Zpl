using Bitbeans.Zpl;
using Bitbeans.Zpl.Viewer.Models;

using System.Collections.Generic;

namespace Bitbeans.Zpl.Viewer
{
    public class VirtualPrinter
    {
        public LabelHome LabelHomePosition { get; private set; }
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
        public FieldDataBase NextElementFieldData { get; private set; }
        public FieldBlock NextElementFieldBlock { get; private set; }
        public FieldOrientation FieldOrientation { get; private set; } = FieldOrientation.Normal;
        public FieldJustification FieldJustification { get; private set; } = FieldJustification.None;
        public int FontWidth { get; private set; } = 0;
        public int FontHeight { get; private set; } = 0;
        public string FontName { get; private set; } = "A";
        public List<string> Comments { get; private set; }

        /// <summary>
        /// Override the default font, only for the next element
        /// </summary>
        public FontInfo NextFont { get; private set; }

        public bool NextElementFieldReverse { get; private set; }
        public char? NextElementFieldHexadecimalIndicator { get; private set; }
        public FieldJustification NextElementFieldJustification { get; private set; } = FieldJustification.None;
        public bool LabelReverse { get; private set; }
        public BarcodeInfo BarcodeInfo { get; private set; }

        public string NextDownloadFormatName { get; private set; }
        public int? NextFieldNumber { get; private set; }

        private bool nextFontOrientationIsExplicit;

        public VirtualPrinter()
        {
            this.BarcodeInfo = new BarcodeInfo();
            this.Comments = [];
        }

        public void SetNextElementPosition(int x, int y, bool calculateFromBottom = false, bool useDefaultPosition = false)
        {
            this.NextElementPosition = new LabelPosition(x, y, calculateFromBottom, useDefaultPosition);
            this.HasNextElementPosition = true;
            if (!useDefaultPosition)
            {
                this.LastElementPositionY = y;
            }
        }

        public void ClearNextElementPosition()
        {
            this.NextElementPosition = new LabelPosition(0, 0, false, false);
            this.HasNextElementPosition = false;
        }

        public void SetNextElementFieldData(FieldDataBase fieldData)
        {
            if (this.NextElementFieldData != null)
            {
                fieldData.Content = this.NextElementFieldData.Content;
            }
            this.NextElementFieldData = fieldData;
        }

        public void SetNextElementFieldDataContent(string content)
        {
            this.NextElementFieldData ??= new FieldDataBase();
            this.NextElementFieldData.Content = content;
        }

        public void ClearNextElementFieldData()
        {
            this.NextElementFieldData = null;
        }

        public void SetNextElementFieldBlock(FieldBlock fieldBlock)
        {
            this.NextElementFieldBlock = fieldBlock;
        }

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

        public void ClearNextFont()
        {
            this.NextFont = null;
            this.nextFontOrientationIsExplicit = false;
        }

        public void SetNextElementFieldReverse()
        {
            this.NextElementFieldReverse = true;
        }

        public void ClearNextElementFieldReverse()
        {
            this.NextElementFieldReverse = false;
        }

        public void SetNextElementFieldHexadecimalIndicator(char replaceChar)
        {
            this.NextElementFieldHexadecimalIndicator = replaceChar;
        }

        public void ClearNextElementFieldHexadecimalIndicator()
        {
            this.NextElementFieldHexadecimalIndicator = null;
        }

        public void SetNextElementFieldJustification(FieldJustification fieldJustification)
        {
            this.NextElementFieldJustification = fieldJustification;
        }

        public void ClearNextElementFieldJustification()
        {
            this.NextElementFieldJustification = FieldJustification.None;
        }

        public void SetLabelReverse(bool reverse)
        {
            this.LabelReverse = reverse;
        }

        public void SetFieldOrientation(FieldOrientation fieldOrientation)
        {
            this.FieldOrientation = fieldOrientation;
            if (this.NextFont != null && !this.nextFontOrientationIsExplicit)
            {
                this.SetNextFont(this.NextFont.FontName, fieldOrientation, this.NextFont.FontWidth, this.NextFont.FontHeight);
            }
        }

        public void SetFieldJustification(FieldJustification fieldJustification)
        {
            this.FieldJustification = fieldJustification;
        }

        public void SetFontWidth(int fontWidth)
        {
            this.FontWidth = fontWidth;
        }

        public void SetFontHeight(int fontHeight)
        {
            this.FontHeight = fontHeight;
        }

        public void SetFontName(string fontName)
        {
            this.FontName = fontName;
        }

        public void SetBarcodeModuleWidth(int moduleWidth)
        {
            this.BarcodeInfo.ModuleWidth = moduleWidth;
        }

        public void SetBarcodeWideBarToNarrowBarWidthRatio(double wideBarToNarrowBarWidthRatio)
        {
            this.BarcodeInfo.WideBarToNarrowBarWidthRatio = wideBarToNarrowBarWidthRatio;
        }

        public void SetBarcodeHeight(int height)
        {
            this.BarcodeInfo.Height = height;
        }

        public void SetLabelHome(int x, int y)
        {
            this.LabelHomePosition = new LabelHome(x, y);
        }

        public void AddComment(string comment)
        {
            this.Comments.Add(comment);
        }

        public void ClearComments()
        {
            this.Comments.Clear();
        }

        public void SetNextDownloadFormatName(string name)
        {
            this.NextDownloadFormatName = name;
        }

        public void ClearNextDownloadFormatName()
        {
            this.NextDownloadFormatName = null;
        }

        public void SetNextFieldNumber(int number)
        {
            this.NextFieldNumber = number;
        }

        public void ClearNextFieldNumber()
        {
            this.NextFieldNumber = null;
        }
    }
}
