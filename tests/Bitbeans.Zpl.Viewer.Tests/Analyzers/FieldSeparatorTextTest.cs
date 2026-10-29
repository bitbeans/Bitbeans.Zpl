using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    /// <summary>
    /// Text fields, field blocks, QR codes and field numbers created when a field is finalized at ^FS.
    /// </summary>
    [TestClass]
    public class FieldSeparatorTextTest
    {
        [TestMethod]
        public void TextField_DefaultFont()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FO10,20^FDhello^FS");

            Assert.AreEqual("hello", text.Text);
            Assert.AreEqual(10, text.PositionX);
            Assert.AreEqual(20, text.PositionY);
            Assert.AreEqual("A", text.Font.FontName);
            Assert.AreEqual(0, text.Font.FontHeight);
            Assert.AreEqual(0, text.Font.FontWidth);
            Assert.AreEqual(FieldOrientation.Normal, text.Font.FieldOrientation);
            Assert.IsFalse(text.ReversePrint);
            Assert.IsNull(text.HexadecimalIndicator);
            Assert.IsNotNull(text.FieldOrigin);
            Assert.IsNull(text.FieldTypeset);
        }

        [TestMethod]
        public void TextField_UsesChangeAlphanumericDefaultFont()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^CF0,40,30^FO10,20^FDhello^FS");

            Assert.AreEqual("0", text.Font.FontName);
            Assert.AreEqual(40, text.Font.FontHeight);
            Assert.AreEqual(30, text.Font.FontWidth);
        }

        [TestMethod]
        public void TextField_FieldOrientationOfFw()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FWR^FO10,20^FDhello^FS");

            Assert.AreEqual(FieldOrientation.Rotated90, text.Font.FieldOrientation);
        }

        [TestMethod]
        public void TextField_ScalableFontOverridesDefaultFontOnlyForNextField()
        {
            ZplElementBase[] elements = AnalyzerTestHelper.Elements("^CF0,40,30^FO10,20^ABR,50,45^FDone^FS^FO10,80^FDtwo^FS");

            ZplFont first = ((ZplTextField)elements[0]).Font;
            Assert.AreEqual("B", first.FontName);
            Assert.AreEqual(FieldOrientation.Rotated90, first.FieldOrientation);
            Assert.AreEqual(50, first.FontHeight);
            Assert.AreEqual(45, first.FontWidth);

            ZplFont second = ((ZplTextField)elements[1]).Font;
            Assert.AreEqual("0", second.FontName);
            Assert.AreEqual(40, second.FontHeight);
            Assert.AreEqual(FieldOrientation.Normal, second.FieldOrientation);
        }

        [TestMethod]
        public void TextField_ScalableFontOrientationFollowsLaterFw()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FO10,20^A0,20,20^FWI^FDhello^FS");

            Assert.AreEqual(FieldOrientation.Rotated180, text.Font.FieldOrientation);
        }

        [TestMethod]
        public void TextField_FieldTypeset()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FT10,20^FDhello^FS");

            Assert.IsNotNull(text.FieldTypeset);
            Assert.IsNull(text.FieldOrigin);
            Assert.AreEqual(10, text.PositionX);
            Assert.AreEqual(20, text.PositionY);
            Assert.IsFalse(text.UseDefaultPosition);
        }

        [TestMethod]
        public void TextField_FieldTypesetWithoutCoordinates()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FT^FDhello^FS");

            Assert.IsTrue(text.UseDefaultPosition);
        }

        [TestMethod]
        public void TextField_WithoutPositionIsAtZero()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FDhello^FS");

            Assert.AreEqual(0, text.PositionX);
            Assert.AreEqual(0, text.PositionY);
        }

        [TestMethod]
        public void TextField_HexadecimalIndicator()
        {
            Assert.AreEqual('_', AnalyzerTestHelper.Single<ZplTextField>("^FO0,0^FH^FDa_5Fb^FS").HexadecimalIndicator);
            Assert.AreEqual('\\', AnalyzerTestHelper.Single<ZplTextField>("^FO0,0^FH\\^FDa\\5Fb^FS").HexadecimalIndicator);
        }

        [TestMethod]
        public void TextField_PerFieldSettingsAreResetAtFieldSeparator()
        {
            ZplElementBase[] elements = AnalyzerTestHelper.Elements("^FO10,20^FH\\^FR^FDone^FS^FDtwo^FS");

            ZplTextField first = (ZplTextField)elements[0];
            Assert.AreEqual('\\', first.HexadecimalIndicator);
            Assert.IsTrue(first.ReversePrint);
            Assert.AreEqual(10, first.PositionX);

            ZplTextField second = (ZplTextField)elements[1];
            Assert.IsNull(second.HexadecimalIndicator);
            Assert.IsFalse(second.ReversePrint);
            Assert.AreEqual(0, second.PositionX);
        }

        [TestMethod]
        public void TextField_FieldReverse()
        {
            Assert.IsTrue(AnalyzerTestHelper.Single<ZplTextField>("^FO0,0^FR^FDx^FS").ReversePrint);
            Assert.IsTrue(AnalyzerTestHelper.Single<ZplTextField>("^LRY^FO0,0^FDx^FS").ReversePrint);
        }

        [TestMethod]
        public void TextField_LabelHomeOffset()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^LH100,50^FO10,20^FDx^FS");

            Assert.AreEqual(110, text.PositionX);
            Assert.AreEqual(70, text.PositionY);
        }

        [TestMethod]
        public void TextField_JustificationOfFieldOriginWinsOverFw()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FWN,2^FO10,20,1^FDx^FS");

            Assert.AreEqual(FieldJustification.Right, text.FieldOrigin.FieldJustification);
        }

        [TestMethod]
        public void TextField_JustificationFallsBackToFw()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FWN,2^FO10,20^FDx^FS");

            Assert.AreEqual(FieldJustification.Auto, text.FieldOrigin.FieldJustification);
        }

        [TestMethod]
        public void TextField_JustificationIsResetAfterTheField()
        {
            ZplElementBase[] elements = AnalyzerTestHelper.Elements("^FO10,20,1^FDone^FS^FO10,40^FDtwo^FS");

            Assert.AreEqual(FieldJustification.Right, ((ZplTextField)elements[0]).FieldOrigin.FieldJustification);
            Assert.AreEqual(FieldJustification.None, ((ZplTextField)elements[1]).FieldOrigin.FieldJustification);
        }

        [TestMethod]
        public void FieldWithoutDataProducesNoElement()
        {
            Assert.IsEmpty(AnalyzerTestHelper.Elements("^FO10,20^FS"));
        }

        [TestMethod]
        public void FieldBlock()
        {
            ZplFieldBlock block = AnalyzerTestHelper.Single<ZplFieldBlock>("^FO10,20^FB400,5,2,R,15^FH\\^FR^FDhello^FS");

            Assert.AreEqual("hello", block.Text);
            Assert.AreEqual(10, block.PositionX);
            Assert.AreEqual(20, block.PositionY);
            Assert.AreEqual(400, block.Width);
            Assert.AreEqual(5, block.MaxLineCount);
            Assert.AreEqual(2, block.LineSpace);
            Assert.AreEqual(TextJustification.Right, block.TextJustification);
            Assert.AreEqual(15, block.HangingIndent);
            Assert.AreEqual('\\', block.HexadecimalIndicator);
        }

        [TestMethod]
        public void FieldBlock_AppliesOnlyToTheNextField()
        {
            ZplElementBase[] elements = AnalyzerTestHelper.Elements("^FO0,0^FB400,2^FDone^FS^FO0,50^FDtwo^FS");

            Assert.IsInstanceOfType<ZplFieldBlock>(elements[0]);
            Assert.IsInstanceOfType<ZplTextField>(elements[1]);
        }

        [TestMethod]
        public void FieldBlock_FieldTypeset()
        {
            ZplFieldBlock block = AnalyzerTestHelper.Single<ZplFieldBlock>("^FT^FB100,1^FDx^FS");

            Assert.IsNotNull(block.FieldTypeset);
            Assert.IsTrue(block.UseDefaultPosition);
        }

        // QR code field data: ^FD<ECC><input>,<data> (normal) / ^FDD<count><mask>,<ECC><input>,<data> (mixed)
        private static ZplQrCode Qr(string commands)
        {
            return AnalyzerTestHelper.Single<ZplQrCode>(commands);
        }

        [TestMethod]
        public void QrCode_PlainFieldData()
        {
            ZplQrCode qr = Qr("^BY3,3.0,77^FO10,20^BQR,1,6,H,3^FDhello^FS");

            Assert.AreEqual("hello", qr.Content);
            Assert.AreEqual(10, qr.PositionX);
            Assert.AreEqual(20, qr.PositionY);
            Assert.AreEqual(1, qr.Model);
            Assert.AreEqual(6, qr.MagnificationFactor);
            Assert.AreEqual(ErrorCorrectionLevel.UltraHighReliability, qr.ErrorCorrectionLevel);
            Assert.AreEqual(3, qr.MaskValue);
            // the ZPL II standard does not allow rotation of QR codes
            Assert.AreEqual(FieldOrientation.Normal, qr.FieldOrientation);
            Assert.AreEqual(77, qr.VerticalQuietZone);
        }

        [TestMethod]
        public void QrCode_FieldTypeset()
        {
            Assert.IsNotNull(Qr("^FT10,20^BQN^FDhello^FS").FieldTypeset);
        }

        [TestMethod]
        public void QrCode_NormalFormatWithAutomaticInput()
        {
            ZplQrCode qr = Qr("^FO0,0^BQN^FDQA,Hello, World^FS");

            Assert.AreEqual("Hello, World", qr.Content);
            Assert.AreEqual(ErrorCorrectionLevel.HighReliability, qr.ErrorCorrectionLevel);
        }

        [TestMethod]
        [DataRow("HA,x", ErrorCorrectionLevel.UltraHighReliability)]
        [DataRow("QA,x", ErrorCorrectionLevel.HighReliability)]
        [DataRow("MA,x", ErrorCorrectionLevel.Standard)]
        [DataRow("LA,x", ErrorCorrectionLevel.HighDensity)]
        public void QrCode_ErrorCorrectionInFieldDataOverridesCommand(string fieldData, ErrorCorrectionLevel expected)
        {
            ZplQrCode qr = Qr("^FO0,0^BQN,2,2,H^FD" + fieldData + "^FS");

            Assert.AreEqual(expected, qr.ErrorCorrectionLevel);
            Assert.AreEqual("x", qr.Content);
        }

        [TestMethod]
        [DataRow("MM,N12345", "12345")]
        [DataRow("MM,A12abc", "12abc")]
        [DataRow("MM,K1234", "1234")]
        [DataRow("MM,B0005Hello World", "Hello")]
        [DataRow("MM,B0100abc", "abc")]
        [DataRow("MM,Xdata", "Xdata")]
        public void QrCode_NormalFormatWithManualInput(string fieldData, string expected)
        {
            Assert.AreEqual(expected, Qr("^FO0,0^BQN^FD" + fieldData + "^FS").Content);
        }

        [TestMethod]
        public void QrCode_FieldDataNotInNormalOrMixedFormatIsKept()
        {
            ZplQrCode qr = Qr("^FO0,0^BQN,2,2,L^FDplain data, with comma^FS");

            Assert.AreEqual("plain data, with comma", qr.Content);
            Assert.AreEqual(ErrorCorrectionLevel.HighDensity, qr.ErrorCorrectionLevel);
        }

        [TestMethod]
        public void QrCode_MixedFormatWithAutomaticInputJoinsTheParts()
        {
            ZplQrCode qr = Qr("^FO0,0^BQN^FDD0123AB,LA,abc,def,ghi^FS");

            Assert.AreEqual("abcdefghi", qr.Content);
            Assert.AreEqual(ErrorCorrectionLevel.HighDensity, qr.ErrorCorrectionLevel);
        }

        [TestMethod]
        public void QrCode_MixedFormatWithManualInputParsesEachMode()
        {
            // numeric, alphanumeric, kanji without count (ends at the next comma); binary with count (4 digits)
            ZplQrCode qr = Qr("^FO0,0^BQN^FDD0123AB,HM,N123,Aabc,B0003xyzK456^FS");

            Assert.AreEqual("123abcxyz456", qr.Content);
            Assert.AreEqual(ErrorCorrectionLevel.UltraHighReliability, qr.ErrorCorrectionLevel);
        }

        [TestMethod]
        public void QrCode_MixedFormatWithManualInputKeepsPartsWithoutModeAndStopsAtTheEnd()
        {
            ZplQrCode qr = Qr("^FO0,0^BQN^FDD0123AB,MM,plain,N12,x^FS");

            Assert.AreEqual("plain12x", qr.Content);
        }

        [TestMethod]
        public void QrCode_MixedFormatWithBinaryCountLongerThanTheData()
        {
            ZplQrCode qr = Qr("^FO0,0^BQN^FDD0123AB,MM,B0099abc^FS");

            Assert.AreEqual("abc", qr.Content);
        }

        [TestMethod]
        public void QrCode_MixedFormatWithManualInputAndSingleModeCharacter()
        {
            // "N" alone has no data, so it is not a valid mode part and is kept as is
            ZplQrCode qr = Qr("^FO0,0^BQN^FDD0123AB,MM,N^FS");

            Assert.AreEqual("N", qr.Content);
        }

        // Field numbers (templates, ^FN)
        [TestMethod]
        public void FieldNumber_WithoutDataIsStoredAsTemplateField()
        {
            ZplFieldNumber number = AnalyzerTestHelper.Single<ZplFieldNumber>("^FO10,20^A0N,30,20^FN5^FS");

            Assert.AreEqual(5, number.Number);
            ZplTextField text = (ZplTextField)number.FormatElement;
            Assert.AreEqual(10, text.PositionX);
            Assert.AreEqual(20, text.PositionY);
            Assert.AreEqual("0", text.Font.FontName);
            Assert.AreEqual(string.Empty, text.Text);
        }

        [TestMethod]
        public void FieldNumber_ForBarcodeKeepsTheBarcodeAsFormatElement()
        {
            ZplFieldNumber number = AnalyzerTestHelper.Single<ZplFieldNumber>("^FO10,20^BCN,50^FN2^FS");

            Assert.AreEqual(2, number.Number);
            Assert.IsInstanceOfType<ZplBarcode128>(number.FormatElement);
            Assert.AreEqual(50, ((ZplBarcode128)number.FormatElement).Height);
        }

        [TestMethod]
        public void FieldNumber_WithDataWithoutTemplateProducesNoElement()
        {
            // The recall element is resolved by the format merger; without a matching template it is dropped
            Assert.IsEmpty(AnalyzerTestHelper.Elements("^FN5^FDvalue^FS"));
        }

        [TestMethod]
        public void FieldNumber_IsConsumedByTheField()
        {
            ZplElementBase[] elements = AnalyzerTestHelper.Elements("^FO10,20^FN5^FS^FO10,50^FDplain^FS");

            Assert.IsInstanceOfType<ZplFieldNumber>(elements[0]);
            Assert.IsInstanceOfType<ZplTextField>(elements[1]);
        }
    }
}
