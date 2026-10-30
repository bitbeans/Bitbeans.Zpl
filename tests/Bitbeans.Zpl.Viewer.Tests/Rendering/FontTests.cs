using Bitbeans.Zpl.Viewer.Helpers;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class FontTests
    {
        private const float HeightScale = 1.1f;

        // ---------- FontScale ----------

        [TestMethod]
        public void FontScale_BitmappedSizeUsesTheDensityTable()
        {
            // font E differs per density: 21 (6 dpmm), 28 (8), 42 (12) and 42 (24)
            Assert.AreEqual(21 * HeightScale, FontScale.GetBitmappedFontSize("E", 1, 6).Value, 0.001f);
            Assert.AreEqual(28 * HeightScale, FontScale.GetBitmappedFontSize("E", 1, 8).Value, 0.001f);
            Assert.AreEqual(42 * HeightScale, FontScale.GetBitmappedFontSize("E", 1, 12).Value, 0.001f);
            Assert.AreEqual(42 * 3 * HeightScale, FontScale.GetBitmappedFontSize("E", 3, 24).Value, 0.001f);
            Assert.AreEqual(9 * 2 * HeightScale, FontScale.GetBitmappedFontSize("A", 2, 8).Value, 0.001f);
        }

        [TestMethod]
        public void FontScale_UnknownFontOrDensityHasNoBitmappedSize()
        {
            Assert.IsNull(FontScale.GetBitmappedFontSize("0", 1, 8));
            Assert.IsNull(FontScale.GetBitmappedFontSize("A", 1, 10));
            Assert.IsNull(FontScale.GetBitmappedFontSize("A", 1, 0));
        }

        [TestMethod]
        public void FontScale_KnownFontWithHeightOnly()
        {
            // font B is 11x7 at 8 dpmm: 22 dots high is exactly twice the cell
            (float size, float scaleX) = FontScale.GetFontScaling("B", 22, 0, 8);

            Assert.AreEqual(22 * HeightScale, size, 0.001f);
            Assert.AreEqual(1.0f, scaleX);
        }

        [TestMethod]
        public void FontScale_KnownFontWithHeightAndWidth()
        {
            // heights are rounded to whole multiples of the cell: 33 / 11 = 3, 14 / 7 = 2
            (float size, float scaleX) = FontScale.GetFontScaling("B", 33, 14, 8);

            Assert.AreEqual(33 * HeightScale, size, 0.001f);
            Assert.AreEqual(2f / 3f, scaleX, 0.001f);
        }

        [TestMethod]
        public void FontScale_KnownFontNeverScalesBelowOneCell()
        {
            (float size, float scaleX) = FontScale.GetFontScaling("B", 2, 1, 8);

            Assert.AreEqual(11 * HeightScale, size, 0.001f);
            Assert.AreEqual(1.0f, scaleX);
        }

        [TestMethod]
        public void FontScale_KnownFontWithWidthOnly()
        {
            // 21 / 7 = 3 cells, applied to the height as well
            (float size, float scaleX) = FontScale.GetFontScaling("B", 0, 21, 8);

            Assert.AreEqual(11 * 3 * HeightScale, size, 0.001f);
            Assert.AreEqual(1.0f, scaleX);
        }

        [TestMethod]
        public void FontScale_KnownFontWithoutSizeUsesOneCell()
        {
            (float size, float scaleX) = FontScale.GetFontScaling("B", 0, 0, 8);

            Assert.AreEqual(11 * HeightScale, size, 0.001f);
            Assert.AreEqual(1.0f, scaleX);
        }

        [TestMethod]
        public void FontScale_UnknownFontUsesTheRequestedSize()
        {
            (float height, float heightScaleX) = FontScale.GetFontScaling("0", 40, 0, 8);
            (float both, float bothScaleX) = FontScale.GetFontScaling("0", 40, 20, 8);
            (float same, float sameScaleX) = FontScale.GetFontScaling("0", 40, 40, 8);
            (float width, float widthScaleX) = FontScale.GetFontScaling("0", 0, 30, 8);
            (float none, float noneScaleX) = FontScale.GetFontScaling("0", 0, 0, 8);

            Assert.AreEqual(40f, height);
            Assert.AreEqual(1f, heightScaleX);
            Assert.AreEqual(40f, both);
            Assert.AreEqual(0.5f, bothScaleX);
            Assert.AreEqual(40f, same);
            Assert.AreEqual(1f, sameScaleX);
            Assert.AreEqual(30f, width);
            Assert.AreEqual(1f, widthScaleX);
            Assert.AreEqual(15f, none);
            Assert.AreEqual(1f, noneScaleX);
        }

        [TestMethod]
        public void FontScale_UnsupportedDensityUsesTheRequestedSize()
        {
            (float size, float scaleX) = FontScale.GetFontScaling("A", 30, 15, 10);

            Assert.AreEqual(30f, size);
            Assert.AreEqual(0.5f, scaleX);
        }

        // ---------- FontManager ----------

        [TestMethod]
        public void FontManager_ProvidesDefaultTypefaces()
        {
            FontManager manager = new();

            SKTypeface zero = manager.Typeface0;
            SKTypeface mono = manager.TypefaceA;

            Assert.IsNotNull(zero);
            Assert.IsNotNull(mono);
            Assert.IsNotNull(manager.TypefaceGS);
            // cached
            Assert.AreSame(zero, manager.Typeface0);
            Assert.AreSame(mono, manager.TypefaceA);
        }

        [TestMethod]
        public void FontManager_FontLoaderMapsFontZeroAndOthers()
        {
            FontManager manager = new();

            Assert.AreSame(manager.Typeface0, manager.FontLoader("0"));
            Assert.AreSame(manager.TypefaceA, manager.FontLoader("A"));
            Assert.AreSame(manager.TypefaceA, manager.FontLoader("anything"));
        }

        [TestMethod]
        public void FontManager_FontLoaderCanBeReplaced()
        {
            FontManager manager = new();
            SKTypeface custom = manager.TypefaceGS;
            manager.FontLoader = name => custom;

            Assert.AreSame(custom, manager.FontLoader("0"));
        }

        [TestMethod]
        public void FontManager_DefaultStacksNameTheDocumentedFonts()
        {
            FontManager manager = new();

            CollectionAssert.Contains(manager.FontStack0, "Arial");
            CollectionAssert.Contains(manager.FontStack0, "Helvetica");
            CollectionAssert.Contains(manager.FontStackA, "DejaVu Sans Mono");
        }

        [TestMethod]
        public void FontManager_RegisteredTypefaceIsPreferred()
        {
            FontManager manager = new();
            SKTypeface registered = manager.TypefaceGS;
            manager.RegisterTypeface(registered);
            manager.FontStack0 = ["NoSuchFontFamilyXyz", registered.FamilyName];
            manager.FontStackA = [registered.FamilyName.ToUpperInvariant()];

            // the family lookup ignores the case
            Assert.AreSame(registered, manager.Typeface0);
            Assert.AreSame(registered, manager.TypefaceA);
        }

        [TestMethod]
        public void FontManager_RegisteringTheSameFamilyTwiceKeepsBothAndPicksTheBestStyle()
        {
            FontManager manager = new();
            SKTypeface first = manager.TypefaceGS;
            using SKTypeface second = SKTypeface.FromStream(new System.IO.MemoryStream(Bitbeans.Zpl.Viewer.Properties.Resources.ZplGS));
            manager.RegisterTypeface(first);
            manager.RegisterTypeface(second);
            manager.FontStack0 = [first.FamilyName];

            Assert.IsNotNull(manager.Typeface0);
            Assert.AreEqual(first.FamilyName, manager.Typeface0.FamilyName);
        }

        [TestMethod]
        public void FontManager_SystemFontIsUsedWhenNothingIsRegistered()
        {
            FontManager manager = new();
            string family = SKTypeface.Default.FamilyName;
            manager.FontStack0 = [family];

            Assert.IsNotNull(manager.Typeface0);
            Assert.AreEqual(family, manager.Typeface0.FamilyName);
        }

        [TestMethod]
        public void FontManager_FallsBackToTheDefaultTypefaceWhenNoFontMatches()
        {
            FontManager manager = new();
            manager.FontStack0 = ["NoSuchFontFamilyXyz"];
            manager.FontStackA = [];

            Assert.IsNotNull(manager.Typeface0);
            Assert.IsNotNull(manager.TypefaceA);
            Assert.AreEqual(SKTypeface.Default.FamilyName, manager.TypefaceA.FamilyName);
        }

        [TestMethod]
        public void FontManager_UsesTheSystemMatcherForUnregisteredFamilies()
        {
            FontManager manager = new();
            List<string> families = [];
            SKTypeface expected = manager.TypefaceGS;
            manager.SystemTypefaceMatcher = (family, style) =>
            {
                families.Add(family);
                return family == "Second" ? expected : null;
            };
            manager.FontStack0 = ["First", "Second", "Third"];

            Assert.AreSame(expected, manager.Typeface0);
            CollectionAssert.AreEqual(new[] { "First", "Second" }, families);
        }

        [TestMethod]
        public void FontManager_FallsBackToSkiaDefaultWhenTheSystemHasNoFonts()
        {
            FontManager manager = new();
            manager.SystemTypefaceMatcher = (family, style) => null;

            Assert.AreSame(SKTypeface.Default, manager.Typeface0);
            Assert.AreSame(SKTypeface.Default, manager.TypefaceA);
        }

        [TestMethod]
        public void FontManager_HasNoStaticSkiaState()
        {
            // A static SkiaSharp field loads libSkiaSharp in the type initializer; when that fails the type stays
            // unusable and the cause is hidden in a TypeInitializationException
            FieldInfo[] skiaFields = typeof(FontManager)
                .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.FieldType.Namespace == typeof(SKObject).Namespace)
                .ToArray();

            Assert.IsEmpty(skiaFields);
        }
    }
}
