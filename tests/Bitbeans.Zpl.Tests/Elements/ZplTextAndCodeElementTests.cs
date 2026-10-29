using Bitbeans.Zpl.Elements;
using System;

namespace Bitbeans.Zpl.Tests
{
    [TestClass]
    public class ZplTextAndCodeElementTests
    {
        private static readonly ZplRenderOptions Scaled = new() { SourcePrintDpi = 200, TargetPrintDpi = 300 };

        // ---- text field ----

        [TestMethod]
        public void TextField()
        {
            var font = new ZplFont();
            var text = new ZplTextField("Hello", 10, 20, font);

            Assert.AreSame(font, text.Font);
            Assert.AreEqual("Hello", text.Text);
            Assert.IsFalse(text.ReversePrint);
            Assert.IsNull(text.HexadecimalIndicator);
            Assert.AreEqual(NewLineConversionMethod.ToSpace, text.NewLineConversion);
            Assert.AreEqual("^A0N,30,30\n^FO10,20\n^FDHello^FS", text.ToZplString());
            Assert.AreEqual("^A0N,45,45\n^FO15,30\n^FDHello^FS", text.ToZplString(Scaled));
        }

        [TestMethod]
        public void TextFieldOptions()
        {
            var font = new ZplFont();
            Assert.AreEqual(
                "^A0N,30,30\n^FT10,20\n^FR^FDHello^FS",
                new ZplTextField("Hello", 10, 20, font, reversePrint: true, bottomToTop: true).ToZplString());
            Assert.AreEqual(
                "^A0N,30,30\n^FO10,20,1\n^FDHello^FS",
                new ZplTextField("Hello", 10, 20, font, fieldJustification: FieldJustification.Right).ToZplString());
            Assert.AreEqual(
                "^A0N,30,30\n^FT\n^FDHello^FS",
                new ZplTextField("Hello", 10, 20, font, bottomToTop: true, useDefaultPosition: true).ToZplString());
        }

        [TestMethod]
        public void TextFieldWithNullTextRendersEmptyField()
        {
            Assert.AreEqual("^A0N,30,30\n^FO1,2\n^FD^FS", new ZplTextField(null, 1, 2, new ZplFont()).ToZplString());
        }

        [TestMethod]
        public void TextFieldHexadecimalIndicator()
        {
            var font = new ZplFont();
            Assert.AreEqual(
                "^A0N,30,30\n^FO1,2\n^FH^FDa_5Fb_5E_7Ec d^FS",
                new ZplTextField("a_b^~c\\d", 1, 2, font, hexadecimalIndicator: '_').ToZplString());
            Assert.AreEqual(
                "^A0N,30,30\n^FO1,2\n^FH!^FDa!5Fb!5E!7E^FS",
                new ZplTextField("a_b^~", 1, 2, font, hexadecimalIndicator: '!').ToZplString());
            Assert.AreEqual(
                "^A0N,30,30\n^FO1,2\n^FH^FR^FDx^FS",
                new ZplTextField("x", 1, 2, font, hexadecimalIndicator: '_', reversePrint: true).ToZplString());
        }

        [TestMethod]
        public void TextFieldReplacesCommandCharactersWithoutHexadecimalIndicator()
        {
            Assert.AreEqual(
                "^A0N,30,30\n^FO1,2\n^FDa b c d_e^FS",
                new ZplTextField("a^b~c\\d_e", 1, 2, new ZplFont()).ToZplString());
        }

        [TestMethod]
        [DataRow(NewLineConversionMethod.ToSpace, "a b")]
        [DataRow(NewLineConversionMethod.ToEmpty, "ab")]
        [DataRow(NewLineConversionMethod.ToZplNewLine, "a\\&b")]
        [DataRow((NewLineConversionMethod)99, "a\nb")]
        public void TextFieldNewLineConversion(NewLineConversionMethod method, string expected)
        {
            var text = new ZplTextField("a\nb", 1, 2, new ZplFont(), method);
            Assert.AreEqual($"^A0N,30,30\n^FO1,2\n^FD{expected}^FS", text.ToZplString());
        }

        [TestMethod]
        public void TextFieldSetTemplateContent()
        {
            var text = new ZplTextField("old", 1, 2, new ZplFont());
            text.SetTemplateContent("new");

            Assert.AreEqual("new", text.Text);
            Assert.AreEqual("^A0N,30,30\n^FO1,2\n^FDnew^FS", text.ToZplString());
        }

        // ---- field block ----

        [TestMethod]
        public void FieldBlock()
        {
            var block = new ZplFieldBlock("text", 10, 20, 300, new ZplFont());

            Assert.AreEqual(300, block.Width);
            Assert.AreEqual(1, block.MaxLineCount);
            Assert.AreEqual(0, block.LineSpace);
            Assert.AreEqual(0, block.HangingIndent);
            Assert.AreEqual(TextJustification.Left, block.TextJustification);
            Assert.AreEqual("^A0N,30,30\n^FO10,20\n^FB300,1,0,L,0\n^FDtext^FS", block.ToZplString());
            Assert.AreEqual("^A0N,45,45\n^FO15,30\n^FB450,1,0,L,0\n^FDtext^FS", block.ToZplString(Scaled));
        }

        [TestMethod]
        [DataRow(TextJustification.Left, "L")]
        [DataRow(TextJustification.Center, "C")]
        [DataRow(TextJustification.Right, "R")]
        [DataRow(TextJustification.Justified, "J")]
        public void FieldBlockJustification(TextJustification justification, string expected)
        {
            var block = new ZplFieldBlock("t", 10, 20, 300, new ZplFont(), 4, 6, justification, 10);
            Assert.AreEqual($"^A0N,30,30\n^FO10,20\n^FB300,4,6,{expected},10\n^FDt^FS", block.ToZplString());
            Assert.AreEqual(4, block.MaxLineCount);
            Assert.AreEqual(6, block.LineSpace);
            Assert.AreEqual(10, block.HangingIndent);
        }

        [TestMethod]
        public void FieldBlockUnknownJustificationIsRejected()
        {
            var block = new ZplFieldBlock("t", 10, 20, 300, new ZplFont(), textJustification: (TextJustification)99);
            Assert.ThrowsExactly<NotImplementedException>(() => block.ToZplString());
        }

        [TestMethod]
        public void FieldBlockNewLineAndOptions()
        {
            var block = new ZplFieldBlock("a\nb", 10, 20, 300, new ZplFont(), hexadecimalIndicator: '_', reversePrint: true, bottomToTop: true);
            Assert.AreEqual("^A0N,30,30\n^FT10,20\n^FB300,1,0,L,0\n^FH^FR^FDa\\&b^FS", block.ToZplString());

            var defaultPosition = new ZplFieldBlock("a", 10, 20, 300, new ZplFont(), bottomToTop: true, useDefaultPosition: true);
            Assert.AreEqual("^A0N,30,30\n^FT\n^FB300,1,0,L,0\n^FDa^FS", defaultPosition.ToZplString());
        }

        [TestMethod]
        public void SingleLineFieldBlock()
        {
            var block = new ZplSingleLineFieldBlock("a\nb", 10, 20, 300, new ZplFont());

            Assert.AreEqual(9999, block.MaxLineCount);
            Assert.AreEqual(9999, block.LineSpace);
            Assert.AreEqual("^A0N,30,30\n^FO10,20\n^FB300,9999,9999,L,0\n^FDa b^FS", block.ToZplString());

            var custom = new ZplSingleLineFieldBlock("a\nb", 10, 20, 300, new ZplFont(), TextJustification.Center, NewLineConversionMethod.ToEmpty, '_', true);
            Assert.AreEqual("^A0N,30,30\n^FO10,20\n^FB300,9999,9999,C,0\n^FH^FR^FDab^FS", custom.ToZplString());
        }

        // ---- text block ----

        [TestMethod]
        public void TextBlock()
        {
            var block = new ZplTextBlock("text", 10, 20, 300, 100, new ZplFont(fieldOrientation: FieldOrientation.Rotated90));

            Assert.AreEqual(300, block.Width);
            Assert.AreEqual(100, block.Height);
            Assert.AreEqual("^A0R,30,30\n^FO10,20\n^TBR,300,100\n^FDtext^FS", block.ToZplString());
            Assert.AreEqual("^A0R,45,45\n^FO15,30\n^TBR,450,150\n^FDtext^FS", block.ToZplString(Scaled));
        }

        [TestMethod]
        public void TextBlockNewLineConversion()
        {
            var block = new ZplTextBlock("a\nb", 10, 20, 300, 100, new ZplFont(), NewLineConversionMethod.ToEmpty, '_');
            Assert.AreEqual("^A0N,30,30\n^FO10,20\n^TBN,300,100\n^FH^FDab^FS", block.ToZplString());
        }

        [TestMethod]
        public void TextBlockDoesNotSupportZplNewLine()
        {
            Assert.ThrowsExactly<NotSupportedException>(() =>
                new ZplTextBlock("a", 10, 20, 300, 100, new ZplFont(), NewLineConversionMethod.ToZplNewLine));
        }

        // ---- 2D codes ----

        [TestMethod]
        public void FieldDataElement()
        {
            var element = new ZplFieldDataElementBase("data", 10, 20, FieldOrientation.Normal, null, false, false);
            Assert.AreEqual("^FO10,20\n^FDdata^FS", element.ToZplString());

            element.SetTemplateContent("other");
            Assert.AreEqual("other", element.Content);
            Assert.AreEqual("^FO10,20\n^FDother^FS", element.ToZplString());

            var hex = new ZplFieldDataElementBase("a_b", 10, 20, FieldOrientation.Normal, '_', true, false);
            Assert.AreEqual("^FT10,20\n^FH^FDa_b^FS", hex.ToZplString());
            var hexCustom = new ZplFieldDataElementBase("a", 10, 20, FieldOrientation.Normal, '!', false, false);
            Assert.AreEqual("^FO10,20\n^FH!^FDa^FS", hexCustom.ToZplString());
        }

        [TestMethod]
        public void AztecBarcode()
        {
            var aztec = new ZplAztecBarcode("X", 10, 20);
            Assert.AreEqual("^FO10,20\n^BON,2,N,0,N,1,\n^FDX^FS", aztec.ToZplString());
            Assert.AreEqual(2, aztec.MagnificationFactor);
            Assert.IsFalse(aztec.ExtendedChannel);
            Assert.IsFalse(aztec.MenuSymbol);
            Assert.AreEqual(1, aztec.SymbolCount);
            Assert.IsNull(aztec.IdField);
        }

        [TestMethod]
        public void AztecBarcodeWithOptions()
        {
            var aztec = new ZplAztecBarcode("X", 10, 20, 3, true, 10, true, 2, "ID", FieldOrientation.Rotated90);
            Assert.AreEqual("^FO10,20\n^BOR,3,Y,10,Y,2,ID\n^FDX^FS", aztec.ToZplString());
            Assert.IsTrue(aztec.MenuSymbol);
            Assert.AreEqual(10, aztec.ErrorControl);
            Assert.AreEqual("ID", aztec.IdField);
        }

        [TestMethod]
        public void DataMatrixDefaults()
        {
            var matrix = new ZplDataMatrix("X", 10, 20);
            Assert.AreEqual("^FO10,20\n^BXN,100,0\n^FDX^FS", matrix.ToZplString());
            Assert.AreEqual("^FO15,30\n^BXN,150,0\n^FDX^FS", matrix.ToZplString(Scaled));
            Assert.AreEqual(100, matrix.Height);
            Assert.AreEqual(QualityLevel.ECC0, matrix.QualityLevel);
            Assert.IsNull(matrix.Columns);
            Assert.IsNull(matrix.Rows);
            Assert.AreEqual(6, matrix.Format);
            Assert.AreEqual('~', matrix.EscapeSequence);
            Assert.IsNull(matrix.AspectRatio);
        }

        [TestMethod]
        public void DataMatrixExtendedParameters()
        {
            Assert.AreEqual(
                "^FO10,20\n^BXR,50,200\n^FDX^FS",
                new ZplDataMatrix("X", 10, 20, 50, QualityLevel.ECC200, FieldOrientation.Rotated90).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BXN,100,200,10,\n^FDX^FS",
                new ZplDataMatrix("X", 10, 20, qualityLevel: QualityLevel.ECC200, columns: 10).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BXN,100,200,,12\n^FDX^FS",
                new ZplDataMatrix("X", 10, 20, qualityLevel: QualityLevel.ECC200, rows: 12).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BXN,100,200,,,1\n^FDX^FS",
                new ZplDataMatrix("X", 10, 20, qualityLevel: QualityLevel.ECC200, format: 1).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BXN,100,200,,,,\\\n^FDX^FS",
                new ZplDataMatrix("X", 10, 20, qualityLevel: QualityLevel.ECC200, escapeSequence: '\\').ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BXN,100,200,8,18,2,\\,2\n^FDX^FS",
                new ZplDataMatrix("X", 10, 20, qualityLevel: QualityLevel.ECC200, columns: 8, rows: 18, format: 2, escapeSequence: '\\', aspectRatio: 2).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BXN,100,200,,,,~,1\n^FDX^FS",
                new ZplDataMatrix("X", 10, 20, qualityLevel: QualityLevel.ECC200, aspectRatio: 1).ToZplString());
        }

        [TestMethod]
        public void DataMatrixPositionAndHex()
        {
            Assert.AreEqual(
                "^FT10,20\n^BXN,100,0\n^FH^FDX^FS",
                new ZplDataMatrix("X", 10, 20, hexadecimalIndicator: '_', bottomToTop: true).ToZplString());
        }

        [TestMethod]
        public void MaxiCode()
        {
            var maxi = new ZplMaxiCode("X", 10, 20);
            Assert.AreEqual(2, maxi.Mode);
            Assert.AreEqual(1, maxi.Position);
            Assert.AreEqual(1, maxi.Total);
            Assert.AreEqual("^FO10,20\n^BD2,1,1\n^FDX^FS", maxi.ToZplString());
            Assert.AreEqual("^FT10,20\n^BD4,2,3\n^FH^FDX^FS", new ZplMaxiCode("X", 10, 20, 4, 2, 3, '_', true).ToZplString());
        }

        [TestMethod]
        public void Pdf417()
        {
            var pdf = new ZplPDF417("X", 10, 20);
            Assert.AreEqual(8, pdf.Height);
            Assert.AreEqual(2, pdf.ModuleWidth);
            Assert.IsNull(pdf.Columns);
            Assert.IsNull(pdf.Rows);
            Assert.IsFalse(pdf.Compact);
            Assert.AreEqual(0, pdf.SecurityLevel);
            Assert.AreEqual("^FO10,20\n^BY2\n^B7N,8,0,,,N\n^FDX^FS", pdf.ToZplString());
            Assert.AreEqual("^FO15,30\n^BY2\n^B7N,12,0,,,N\n^FDX^FS", pdf.ToZplString(Scaled));
        }

        [TestMethod]
        public void Pdf417WithOptions()
        {
            var pdf = new ZplPDF417("X", 10, 20, 10, 3, 5, 20, true, 4, FieldOrientation.Rotated180, '_', true);
            Assert.AreEqual("^FT10,20\n^BY3\n^B7I,10,4,5,20,Y\n^FH^FDX^FS", pdf.ToZplString());
        }

        [TestMethod]
        public void QrCode()
        {
            var qr = new ZplQrCode("MM,AAC-42", 10, 20);
            Assert.AreEqual(2, qr.Model);
            Assert.AreEqual(2, qr.MagnificationFactor);
            Assert.AreEqual(ErrorCorrectionLevel.HighReliability, qr.ErrorCorrectionLevel);
            Assert.AreEqual(7, qr.MaskValue);
            Assert.AreEqual(10, qr.VerticalQuietZone);
            Assert.AreEqual("^FO10,20\n^BQN,2,2,Q,7\n^FDQA,MM,AAC-42^FS", qr.ToZplString());
            Assert.AreEqual("^FO15,30\n^BQN,2,3,Q,7\n^FDQA,MM,AAC-42^FS", qr.ToZplString(Scaled));
        }

        [TestMethod]
        public void QrCodeWithOptions()
        {
            Assert.AreEqual(
                "^FT10,20\n^BQR,1,4,H,3\n^FH^FDHA,X^FS",
                new ZplQrCode("X", 10, 20, 1, 4, ErrorCorrectionLevel.UltraHighReliability, 3, FieldOrientation.Rotated90, '_', true).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BQN,2,2,L,7\n^FH!^FDLA,X^FS",
                new ZplQrCode("X", 10, 20, errorCorrectionLevel: ErrorCorrectionLevel.HighDensity, hexadecimalIndicator: '!').ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BQN,2,2,M,7\n^FDMA,X^FS",
                new ZplQrCode("X", 10, 20, errorCorrectionLevel: ErrorCorrectionLevel.Standard).ToZplString());
        }
    }
}
