using System;
using System.Linq;
using Bitbeans.Zpl.Viewer.Symologies;

namespace Bitbeans.Zpl.Viewer.Tests.Symbologies
{
    [TestClass]
    public class ZplCode128SymbologyTest
    {
        // Symbol values (ISO/IEC 15417): START_A 103, START_B 104, START_C 105, CODE_C 99,
        // CODE_B 100 (from A/C), CODE_A 101 (from B/C), FNC1 102. In code sets A and B a printable
        // character has the value (ASCII - 32).
        private const int StartA = 103;
        private const int StartB = 104;
        private const int StartC = 105;
        private const int CodeC = 99;
        private const int CodeB = 100;
        private const int CodeA = 101;
        private const int Fnc1 = 102;

        private static (int[] Data, string Interpretation) Encode(string content, Code128CodeSet set, bool gs1 = false)
        {
            (bool[] modules, string interpretation) = ZplCode128Symbology.Encode(content, set, gs1);
            return (Code128Decoder.Data(modules), interpretation);
        }

        private static void AssertData(int[] expected, int[] actual)
        {
            CollectionAssert.AreEqual(expected, actual, "expected [" + string.Join(",", expected) + "] but was [" + string.Join(",", actual) + "]");
        }

        [TestMethod]
        public void StartAndStopPatterns()
        {
            (bool[] modules, _) = ZplCode128Symbology.Encode("A", Code128CodeSet.Code128B, false);
            string text = BarPattern.ToText(modules);
            Assert.AreEqual("11010010000", text.Substring(0, 11), "START B");
            Assert.AreEqual("1100011101011", text.Substring(text.Length - 13), "STOP");
            Assert.AreEqual(11 * 3 + 13, text.Length);

            (modules, _) = ZplCode128Symbology.Encode("A", Code128CodeSet.Code128A, false);
            Assert.AreEqual("11010000100", BarPattern.ToText(modules).Substring(0, 11), "START A");

            (modules, _) = ZplCode128Symbology.Encode("12", Code128CodeSet.Code128C, false);
            Assert.AreEqual("11010011100", BarPattern.ToText(modules).Substring(0, 11), "START C");
        }

        [TestMethod]
        public void ChecksumOfWellKnownExample()
        {
            // Wikipedia: "PJJ123C" in code set A, checksum 54
            (bool[] modules, _) = ZplCode128Symbology.Encode("PJJ123C", Code128CodeSet.Code128A, false);
            int[] all = Code128Decoder.Symbols(modules);
            CollectionAssert.AreEqual(new[] { 103, 48, 42, 42, 17, 18, 19, 35, 54 }, all);
        }

        // ---- automatic code set selection ----

        [TestMethod]
        public void AutoEmptyContent()
        {
            (int[] data, string interpretation) = Encode(string.Empty, Code128CodeSet.Code128);
            AssertData([StartB], data);
            Assert.AreEqual(string.Empty, interpretation);
        }

        [TestMethod]
        public void AutoSingleCharacter()
        {
            AssertData([StartB, 33], Encode("A", Code128CodeSet.Code128).Data);
        }

        [TestMethod]
        public void AutoStartsWithCodeCForFourDigits()
        {
            (int[] data, string interpretation) = Encode("1234", Code128CodeSet.Code128);
            AssertData([StartC, 12, 34], data);
            Assert.AreEqual("1234", interpretation);
        }

        [TestMethod]
        public void AutoOddDigitCountSwitchesBackToB()
        {
            AssertData([StartC, 12, 34, CodeB, 21], Encode("12345", Code128CodeSet.Code128).Data);
        }

        [TestMethod]
        public void AutoSwitchesToCodeCLateAndKeepsOddDigitInB()
        {
            // ABC12345 -> START_B A B C 1 CODE_C 23 45
            AssertData([StartB, 33, 34, 35, 17, CodeC, 23, 45], Encode("ABC12345", Code128CodeSet.Code128).Data);
        }

        [TestMethod]
        public void AutoSwitchesToCodeCForEvenDigitRun()
        {
            AssertData([StartB, 33, 34, CodeC, 12, 34], Encode("AB1234", Code128CodeSet.Code128).Data);
        }

        [TestMethod]
        public void AutoDoesNotSwitchForThreeDigits()
        {
            AssertData([StartB, 33, 17, 18, 19], Encode("A123", Code128CodeSet.Code128).Data);
        }

        [TestMethod]
        public void AutoDoesNotStartWithCodeCForThreeDigitsFollowedByLetter()
        {
            AssertData([StartB, 17, 18, 19, 33], Encode("123A", Code128CodeSet.Code128).Data);
        }

        [TestMethod]
        public void AutoSwitchesFromCodeCToBForLetter()
        {
            AssertData([StartC, 12, 34, CodeB, 33], Encode("1234A", Code128CodeSet.Code128).Data);
        }

        [TestMethod]
        public void AutoSkipsCharactersWithoutEncoding()
        {
            (int[] data, string interpretation) = Encode("Aé\tB", Code128CodeSet.Code128);
            AssertData([StartB, 33, 34], data);
            Assert.AreEqual("AB", interpretation);
        }

        [TestMethod]
        [DataRow("Hello World")]
        [DataRow("ABC12345")]
        [DataRow("1234")]
        [DataRow("12345")]
        [DataRow("A1234")]
        [DataRow("1234A")]
        [DataRow("0123456789")]
        [DataRow("012345678")]
        [DataRow("~{|}`^_@[\\]")]
        [DataRow(" !\"#$%&'()*+,-./:;<=>?")]
        [DataRow("abcdefghijklmnopqrstuvwxyz")]
        [DataRow("Code128 test, 2024-05-17 12:30:45")]
        public void AutoRoundTripsThroughZXing(string content)
        {
            (bool[] modules, string interpretation) = ZplCode128Symbology.Encode(content, Code128CodeSet.Code128, false);
            Assert.AreEqual(content, Code128Decoder.ZXingText(modules));
            Assert.AreEqual(content, interpretation);
            Code128Decoder.Symbols(modules);
        }

        [TestMethod]
        public void EveryCodeBSymbolRoundTripsThroughZXing()
        {
            for (int ascii = 32; ascii < 128; ascii++)
            {
                if (ascii == '>')
                {
                    // starts an invocation like ">y"
                    continue;
                }

                string content = "x" + (char)ascii + "y";
                (bool[] modules, _) = ZplCode128Symbology.Encode(content, Code128CodeSet.Code128B, false);
                Assert.AreEqual(content, Code128Decoder.ZXingText(modules), "ASCII " + ascii);
            }
        }

        [TestMethod]
        public void EveryCodeCPairRoundTripsThroughZXing()
        {
            for (int value = 0; value < 100; value++)
            {
                string content = value.ToString("00") + "00";
                (bool[] modules, _) = ZplCode128Symbology.Encode(content, Code128CodeSet.Code128C, false);
                Assert.AreEqual(content, Code128Decoder.ZXingText(modules), "pair " + value);
            }
        }

        [TestMethod]
        public void EveryCodeASymbolEncodesToItsValue()
        {
            for (int ascii = 0; ascii < 96; ascii++)
            {
                string content = ((char)ascii).ToString();
                if (ascii == '>')
                {
                    continue;
                }

                (int[] data, _) = Encode(content, Code128CodeSet.Code128A);
                int expected = ascii < 32 ? ascii + 64 : ascii - 32;
                AssertData([StartA, expected], data);
            }
        }

        // ---- explicit code sets ----

        [TestMethod]
        public void ExplicitCodeA()
        {
            (int[] data, string interpretation) = Encode("AB\u0001", Code128CodeSet.Code128A);
            AssertData([StartA, 33, 34, 65], data);
            Assert.AreEqual("AB\u0001", interpretation);
        }

        [TestMethod]
        public void ExplicitCodeASkipsLowerCase()
        {
            AssertData([StartA, 33], Encode("Aa", Code128CodeSet.Code128A).Data);
        }

        [TestMethod]
        public void ExplicitCodeB()
        {
            AssertData([StartB, 33, 66], Encode("Ab", Code128CodeSet.Code128B).Data);
        }

        [TestMethod]
        public void ExplicitCodeCPairs()
        {
            (int[] data, string interpretation) = Encode("1234", Code128CodeSet.Code128C);
            AssertData([StartC, 12, 34], data);
            Assert.AreEqual("1234", interpretation);
        }

        [TestMethod]
        public void ExplicitCodeCOddDigitSwitchesToB()
        {
            AssertData([StartC, 12, CodeB, 19], Encode("123", Code128CodeSet.Code128C).Data);
        }

        [TestMethod]
        public void ExplicitCodeCSkipsNonDigitPairs()
        {
            // "A4" is no pair and is skipped, the remaining single digit switches to code set B
            AssertData([StartC, 12, CodeB, 20], Encode("12A4", Code128CodeSet.Code128C).Data);
        }

        [TestMethod]
        public void ExplicitCodeCSingleCharacter()
        {
            AssertData([StartC, CodeB, 19], Encode("3", Code128CodeSet.Code128C).Data);
        }

        [TestMethod]
        public void ExplicitEmptyContent()
        {
            AssertData([StartB], Encode(string.Empty, Code128CodeSet.Code128B).Data);
        }

        // ---- start code invocations ----

        [TestMethod]
        [DataRow(">9AB", Code128CodeSet.Code128B, StartA, 33, 34)]
        [DataRow(">:AB", Code128CodeSet.Code128A, StartB, 33, 34)]
        [DataRow(">;1234", Code128CodeSet.Code128A, StartC, 12, 34)]
        [DataRow(">:>9AB", Code128CodeSet.Code128C, StartA, 33, 34)]
        public void StartCodeInvocationSelectsCodeSet(string content, Code128CodeSet initial, int start, int first, int second)
        {
            AssertData([start, first, second], Encode(content, initial).Data);
        }

        [TestMethod]
        public void StartCodeInvocationWithoutContentIsHandledAsInvocation()
        {
            // no content after >9: not a start code, so it switches code sets like a mid-stream invocation
            AssertData([StartB, CodeA], Encode(">9", Code128CodeSet.Code128B).Data);
        }

        // ---- mid-stream invocations ----

        [TestMethod]
        public void InvocationSwitchToCodeCFromA()
        {
            AssertData([StartA, 33, CodeC, 12, 34], Encode("A>51234", Code128CodeSet.Code128A).Data);
        }

        [TestMethod]
        public void InvocationSwitchToCodeBFromA()
        {
            AssertData([StartA, 33, CodeB, 65], Encode("A>6a", Code128CodeSet.Code128A).Data);
        }

        [TestMethod]
        public void InvocationSwitchToCodeAFromB()
        {
            AssertData([StartB, 65, CodeA, 33], Encode("a>7A", Code128CodeSet.Code128B).Data);
        }

        [TestMethod]
        public void InvocationFnc4InCodeADoesNotSwitch()
        {
            AssertData([StartA, 33, 101, 34], Encode("A>7B", Code128CodeSet.Code128A).Data);
        }

        [TestMethod]
        public void InvocationFnc4InCodeBDoesNotSwitch()
        {
            AssertData([StartB, 33, 100, 34], Encode("A>6B", Code128CodeSet.Code128B).Data);
        }

        [TestMethod]
        public void InvocationFromCodeCToB()
        {
            AssertData([StartC, 12, CodeB, 65, 66], Encode("12>6ab", Code128CodeSet.Code128C).Data);
        }

        [TestMethod]
        public void InvocationFromCodeCToA()
        {
            AssertData([StartC, 12, CodeA, 33], Encode("12>7A", Code128CodeSet.Code128C).Data);
        }

        [TestMethod]
        public void InvocationCodeCWhileInCodeCOnlyEmitsTheSymbol()
        {
            AssertData([StartC, 12, 99, 34], Encode("12>534", Code128CodeSet.Code128C).Data);
        }

        [TestMethod]
        [DataRow("><", 62)]
        [DataRow(">0", 30)]
        [DataRow(">=", 94)]
        [DataRow(">1", 95)]
        [DataRow(">2", 96)]
        [DataRow(">3", 97)]
        [DataRow(">4", 98)]
        [DataRow(">8", 102)]
        public void InvocationCodesMapToSymbolValues(string invocation, int value)
        {
            AssertData([StartB, 33, value, 34], Encode("A" + invocation + "B", Code128CodeSet.Code128B).Data);
        }

        [TestMethod]
        [DataRow("A>9B", Code128CodeSet.Code128B, StartB, 33, CodeA, 34)]
        [DataRow("A>;12", Code128CodeSet.Code128B, StartB, 33, CodeC, 12)]
        [DataRow("A>:b", Code128CodeSet.Code128A, StartA, 33, CodeB, 66)]
        [DataRow("A>;12", Code128CodeSet.Code128A, StartA, 33, CodeC, 12)]
        public void StartCodeInvocationInTheMiddleSwitchesCodeSet(string content, Code128CodeSet initial, int start, int a, int sw, int b)
        {
            AssertData([start, a, sw, b], Encode(content, initial).Data);
        }

        [TestMethod]
        public void StartCodeInvocationInCodeC()
        {
            AssertData([StartC, 12, CodeA, 33], Encode("12>9A", Code128CodeSet.Code128C).Data);
            AssertData([StartC, 12, CodeB, 65], Encode("12>:a", Code128CodeSet.Code128C).Data);
        }

        [TestMethod]
        public void StartCodeInvocationForCurrentCodeSetIsIgnored()
        {
            AssertData([StartB, 33, 34], Encode("A>:B", Code128CodeSet.Code128B).Data);
        }

        [TestMethod]
        public void UnknownInvocationDropsTheGreaterThanSign()
        {
            (int[] data, string interpretation) = Encode("A>xB", Code128CodeSet.Code128B);
            AssertData([StartB, 33, 88, 34], data);
            Assert.AreEqual("AxB", interpretation);
        }

        [TestMethod]
        public void TrailingGreaterThanSignIsEncodedLiterally()
        {
            AssertData([StartB, 33, 30], Encode("A>", Code128CodeSet.Code128B).Data);
        }

        [TestMethod]
        public void TrailingGreaterThanSignInCodeCSwitchesToB()
        {
            AssertData([StartC, 12, CodeB, 30], Encode("12>", Code128CodeSet.Code128C).Data);
        }

        // ---- GS1 ----

        [TestMethod]
        public void Gs1EmptyContentInsertsFnc1()
        {
            AssertData([StartB, Fnc1], Encode(string.Empty, Code128CodeSet.Code128, true).Data);
        }

        [TestMethod]
        public void Gs1StartsWithCodeCAndInsertsFnc1()
        {
            (int[] data, string interpretation) = Encode("0112", Code128CodeSet.Code128B, true);
            AssertData([StartC, Fnc1, 1, 12], data);
            Assert.AreEqual("0112", interpretation);
        }

        [TestMethod]
        public void Gs1StartsWithCodeBForShortDigitRun()
        {
            AssertData([StartB, Fnc1, 16, 17, 17], Encode("011", Code128CodeSet.Code128C, true).Data);
        }

        [TestMethod]
        public void Gs1FieldSeparatorInCodeB()
        {
            (int[] data, string interpretation) = Encode("A>8B", Code128CodeSet.Code128, true);
            AssertData([StartB, Fnc1, 33, Fnc1, 34], data);
            Assert.AreEqual("AB", interpretation);
        }

        [TestMethod]
        public void Gs1LeadingFnc1IsNotDuplicated()
        {
            AssertData([StartB, Fnc1, 33], Encode(">8A", Code128CodeSet.Code128, true).Data);
        }

        [TestMethod]
        public void Gs1LeadingFnc1BeforeDigitPairsStartsCodeC()
        {
            AssertData([StartC, Fnc1, 1, 12], Encode(">80112", Code128CodeSet.Code128, true).Data);
        }

        [TestMethod]
        public void Gs1SwitchesToCodeCLate()
        {
            AssertData([StartB, Fnc1, 33, 34, CodeC, 1, 23, 45], Encode("AB012345", Code128CodeSet.Code128, true).Data);
        }

        [TestMethod]
        public void Gs1FieldSeparatorInsideCodeCRun()
        {
            AssertData([StartB, Fnc1, 33, 34, CodeC, 1, 23, Fnc1], Encode("AB0123>8", Code128CodeSet.Code128, true).Data);
        }

        [TestMethod]
        public void Gs1SwitchesFromCodeCToB()
        {
            AssertData([StartC, Fnc1, 1, 12, CodeB, 33], Encode("0112A", Code128CodeSet.Code128, true).Data);
        }

        [TestMethod]
        public void Gs1SkipsCharactersWithoutEncoding()
        {
            AssertData([StartB, Fnc1, 33], Encode("Aé", Code128CodeSet.Code128, true).Data);
        }

        [TestMethod]
        public void Gs1RoundTripsThroughZXing()
        {
            (bool[] modules, _) = ZplCode128Symbology.Encode("0109506000134352" + "10ABC123>821XYZ", Code128CodeSet.Code128, true);
            Assert.AreEqual("]C1" + "0109506000134352" + "10ABC123" + "\u001d" + "21XYZ", Code128Decoder.ZXingText(modules, gs1: true));
            AssertData(GtinWithBatchAndSerial, Code128Decoder.Data(modules));
        }

        // (01)09506000134352 (10)ABC123 (21)XYZ
        private static readonly int[] GtinWithBatchAndSerial =
            [StartC, Fnc1, 1, 9, 50, 60, 0, 13, 43, 52, 10, CodeB, 33, 34, 35, 17, CodeC, 23, Fnc1, 21, CodeB, 56, 57, 58];

        // ---- extended Encode: modes and UCC check digit ----

        private static (int[] Data, string Interpretation) EncodeMode(string content, string mode, bool ucc, Code128CodeSet set = Code128CodeSet.Code128, bool gs1 = false)
        {
            (bool[] modules, string interpretation) = ZplCode128Symbology.Encode(content, set, gs1, mode, ucc);
            return (Code128Decoder.Data(modules), interpretation);
        }

        // SSCC example "376104250021234569": extension digit 3, company prefix 7610425, serial 002123456, check digit 9.
        private static readonly int[] SsccData = [StartC, Fnc1, 0, 37, 61, 4, 25, 0, 21, 23, 45, 69];

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("N")]
        [DataRow("n")]
        [DataRow("A")]
        [DataRow("X")]
        public void NormalModeLeavesContentUntouched(string mode)
        {
            (int[] data, string interpretation) = EncodeMode("AB12", mode, false, Code128CodeSet.Code128B);
            AssertData([StartB, 33, 34, 17, 18], data);
            Assert.AreEqual("AB12", interpretation);
        }

        [TestMethod]
        public void NormalModeWithGs1Flag()
        {
            AssertData([StartB, Fnc1, 33], EncodeMode("A", "N", false, Code128CodeSet.Code128, true).Data);
        }

        [TestMethod]
        [DataRow(">;>80037610425002123456", ">;>8")]
        [DataRow(">;>800376104250021234569", ">;>8")]
        [DataRow(">;>800376104250021234560", ">;>8")]
        [DataRow(">80037610425002123456", ">8")]
        public void ExplicitUccWithCheckDigit(string content, string prefix)
        {
            Assert.IsTrue(content.StartsWith(prefix, StringComparison.Ordinal));
            (int[] data, string interpretation) = EncodeMode(content, "N", true);
            AssertData(SsccData, data);
            Assert.AreEqual("00376104250021234569", interpretation);
        }

        [TestMethod]
        public void ExplicitUccIsIgnoredWithoutUccFlag()
        {
            // handled as start code C, FNC1 invocation and plain digit pairs; the last digit is left over
            (int[] data, string interpretation) = EncodeMode(">;>80037610425002123456", "N", false, Code128CodeSet.Code128C);
            AssertData([StartC, Fnc1, 0, 37, 61, 4, 25, 0, 21, 23, 45, CodeB, 22], data);
            Assert.AreEqual("0037610425002123456", interpretation);
        }

        [TestMethod]
        public void UccFlagWithoutExplicitPatternLeavesContentUntouched()
        {
            (int[] data, string interpretation) = EncodeMode("ABC", "N", true, Code128CodeSet.Code128B);
            AssertData([StartB, 33, 34, 35], data);
            Assert.AreEqual("ABC", interpretation);
        }

        [TestMethod]
        [DataRow("00376104250021234569")]
        [DataRow("00376104250021234560")]
        [DataRow("0037610425002123456")]
        [DataRow(" 00 3761 0425 0021 2345 6 ")]
        public void ModeUNormalizesSscc(string content)
        {
            (int[] data, string interpretation) = EncodeMode(content, "U", false);
            AssertData(SsccData, data);
            Assert.AreEqual("00376104250021234569", interpretation);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("00ABC")]
        [DataRow("0037610425002123456789")]
        [DataRow("003761042500212345")]
        [DataRow("01376104250021234569")]
        public void ModeUInvalidContentThrows(string content)
        {
            Assert.ThrowsExactly<Exception>(() => ZplCode128Symbology.Encode(content, Code128CodeSet.Code128, false, "U", false));
        }

        [TestMethod]
        public void ModeIsCaseInsensitiveAndTrimmed()
        {
            AssertData(SsccData, EncodeMode("0037610425002123456", " u ", false).Data);
        }

        [TestMethod]
        public void ExplicitUccWithWrongDigitCountIsEncodedAsPlainData()
        {
            // 16 digits after >8 do not match the explicit UCC pattern
            (int[] data, string interpretation) = EncodeMode(">80037610425002123", "N", true, Code128CodeSet.Code128C);
            AssertData([StartC, Fnc1, 0, 37, 61, 4, 25, 0, 21, 23], data);
            Assert.AreEqual("0037610425002123", interpretation);
        }

        // ---- application identifier mode ----

        private static void AssertModeD(string content, string expectedPrepared, string expectedInterpretation)
        {
            (bool[] modules, string interpretation) = ZplCode128Symbology.Encode(content, Code128CodeSet.Code128, false, "D", false);
            (bool[] expectedModules, _) = ZplCode128Symbology.Encode(expectedPrepared, Code128CodeSet.Code128, true);
            Assert.AreEqual(BarPattern.ToText(expectedModules), BarPattern.ToText(modules), "prepared content");
            Assert.AreEqual(expectedInterpretation, interpretation);
        }

        [TestMethod]
        public void ModeDEncodesApplicationIdentifiers()
        {
            // (01)09506000134352 (10)ABC123 (21)XYZ: the variable length batch number is followed by FNC1
            (int[] data, string interpretation) = EncodeMode("(01)09506000134352(10)ABC123>821XYZ".Replace(">8", string.Empty).Replace("21XYZ", "(21)XYZ"), "D", false);
            AssertData(GtinWithBatchAndSerial, data);
            Assert.AreEqual("(01)09506000134352(10)ABC123(21)XYZ", interpretation);
        }

        [TestMethod]
        public void ModeDRoundTripsThroughZXing()
        {
            (bool[] modules, _) = ZplCode128Symbology.Encode("(01)09506000134352(10)ABC123(21)XYZ", Code128CodeSet.Code128, false, "d", false);
            Assert.AreEqual("]C1" + "0109506000134352" + "10ABC123" + "" + "21XYZ", Code128Decoder.ZXingText(modules, gs1: true));
        }

        [TestMethod]
        [DataRow("(00)376104250021234560", "00376104250021234569", "(00)376104250021234569")]
        [DataRow("(00)376104250021234569", "00376104250021234569", "(00)376104250021234569")]
        [DataRow("(00) 3761 0425 0021 2345 60 ", "00376104250021234569", "(00) 3761 0425 0021 2345 69 ")]
        [DataRow("(01)09506000134352", "0109506000134352", "(01)09506000134352")]
        [DataRow("(10)ABC", "10ABC", "(10)ABC")]
        [DataRow("(11)240101", "11240101", "(11)240101")]
        [DataRow("(13)240102", "13240102", "(13)240102")]
        [DataRow("(15)240103", "15240103", "(15)240103")]
        [DataRow("(17)240104", "17240104", "(17)240104")]
        [DataRow("(20)12", "2012", "(20)12")]
        [DataRow("(21)SN1", "21SN1", "(21)SN1")]
        [DataRow("(22)ABC", "22ABC", "(22)ABC")]
        [DataRow("(23)ABC", "23ABC", "(23)ABC")]
        [DataRow("(30)12", "3012", "(30)12")]
        [DataRow("(400)ORDER1", "400ORDER1", "(400)ORDER1")]
        [DataRow("(410)1234567890123", "4101234567890123", "(410)1234567890123")]
        [DataRow("(411)1234567890123", "4111234567890123", "(411)1234567890123")]
        [DataRow("(412)1234567890123", "4121234567890123", "(412)1234567890123")]
        [DataRow("(420)12345", "42012345", "(420)12345")]
        [DataRow("(421)123", "421123", "(421)123")]
        [DataRow("(8001)12345678901234", "800112345678901234", "(8001)12345678901234")]
        [DataRow("(8002)ABC", "8002ABC", "(8002)ABC")]
        [DataRow("(3100)123456", "3100123456", "(3100)123456")]
        [DataRow("(3105)123456", "3105123456", "(3105)123456")]
        [DataRow("(3166)123456", "3166123456", "(3166)123456")]
        [DataRow("(3200)123456", "3200123456", "(3200)123456")]
        [DataRow("(3209)123456", "3209123456", "(3209)123456")]
        public void ModeDApplicationIdentifiers(string content, string prepared, string interpretation)
        {
            AssertModeD(content, prepared, interpretation);
        }

        [TestMethod]
        [DataRow("(10)A(21)B", "10A>821B", "(10)A(21)B")]
        [DataRow("(10)A(11)240101", "10A>811240101", "(10)A(11)240101")]
        [DataRow("(11)240101(10)A", "1124010110A", "(11)240101(10)A")]
        [DataRow("(01) 0950 6000 1343 52 (10) ABC 123 (21) X", "010950600013435210ABC123>821X", "(01) 0950 6000 1343 52 (10) ABC 123 (21) X")]
        [DataRow("  (01)09506000134352   (10)A", "010950600013435210A", "(01)09506000134352   (10)A")]
        [DataRow("(10)AB-12", "10AB12", "(10)AB-12")]
        [DataRow("(30)1A2", "3012", "(30)1A2")]
        [DataRow("(11)12x3456", "11123456", "(11)12x3456")]
        [DataRow("(10)A (21)B ", "10A>821B", "(10)A (21)B ")]
        public void ModeDFieldSeparatorsAndWhitespace(string content, string prepared, string interpretation)
        {
            AssertModeD(content, prepared, interpretation);
        }

        [TestMethod]
        [DataRow("0037610425002123456", "00376104250021234569", "0037610425002123456" + "9")]
        [DataRow("00376104250021234560", "00376104250021234569", "00376104250021234569")]
        [DataRow("00 3761 0425 0021 2345 60  ", "00376104250021234569", "00 3761 0425 0021 2345 69  ")]
        [DataRow("(0037610425002123456", "00376104250021234569", "(0037610425002123456" + "9")]
        public void ModeDFallbackNormalizesSscc(string content, string prepared, string interpretation)
        {
            AssertModeD(content, prepared, interpretation);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("ABC")]
        [DataRow("(01")]
        [DataRow("(99)123")]
        [DataRow("(01)1234(10)A")]
        [DataRow("(01)1234")]
        [DataRow("(00)123")]
        [DataRow("(10)")]
        [DataRow("(10)---")]
        [DataRow("(10)123456789012345678901")]
        [DataRow("(30)123456789")]
        [DataRow("(3170)123456")]
        [DataRow("(31)123456")]
        [DataRow("(310)123456")]
        [DataRow("(31/0)123456")]
        [DataRow("(310A)123456")]
        [DataRow("(3300)123456")]
        [DataRow("(320)123456")]
        [DataRow("(320A)123456")]
        [DataRow("(3210)123456")]
        public void ModeDInvalidContentThrows(string content)
        {
            Assert.ThrowsExactly<Exception>(() => ZplCode128Symbology.Encode(content, Code128CodeSet.Code128, false, "D", false));
        }
    }
}
