using System;
using System.Collections.Generic;
using System.Linq;

namespace MakerAPI
{
    public static class MakerConstants
    {
        private static readonly List<MakerCategory> _builtInCategories = new List<MakerCategory>
        {
            new MakerCategory("00_FaceTop", "tglAll", 10),
            new MakerCategory("00_FaceTop", "tglEar", 20),
            new MakerCategory("00_FaceTop", "tglChin", 30),
            new MakerCategory("00_FaceTop", "tglCheek", 40),
            new MakerCategory("00_FaceTop", "tglEyebrow", 50),
            new MakerCategory("00_FaceTop", "tglEye01", 60),
            new MakerCategory("00_FaceTop", "tglEye02", 70),
            new MakerCategory("00_FaceTop", "tglNose", 80),
            new MakerCategory("00_FaceTop", "tglMouth", 90),
            new MakerCategory("00_FaceTop", "tglMole", 100),
            new MakerCategory("00_FaceTop", "tglMakeup", 110),
            new MakerCategory("00_FaceTop", "tglShape", 120),

            new MakerCategory("01_BodyTop", "tglAll", 10),
            new MakerCategory("01_BodyTop", "tglBreast", 20),
            new MakerCategory("01_BodyTop", "tglUpper", 30),
            new MakerCategory("01_BodyTop", "tglLower", 40),
            new MakerCategory("01_BodyTop", "tglArm", 50),
            new MakerCategory("01_BodyTop", "tglLeg", 60),
            new MakerCategory("01_BodyTop", "tglNail", 70),
            new MakerCategory("01_BodyTop", "tglUnderhair", 80),
            new MakerCategory("01_BodyTop", "tglSunburn", 90),
            new MakerCategory("01_BodyTop", "tglPaint", 100),
            new MakerCategory("01_BodyTop", "tglShape", 110),

            new MakerCategory("02_HairTop", "common", 10),
            new MakerCategory("02_HairTop", "tglBack", 20),
            new MakerCategory("02_HairTop", "tglFront", 30),
            new MakerCategory("02_HairTop", "tglSide", 40),
            new MakerCategory("02_HairTop", "tglExtension", 50),
            new MakerCategory("02_HairTop", "tglEtc", 60),

            new MakerCategory("03_ClothesTop", "tglTop", 10),
            new MakerCategory("03_ClothesTop", "tglBot", 20),
            new MakerCategory("03_ClothesTop", "tglBra", 30),
            new MakerCategory("03_ClothesTop", "tglShorts", 40),
            new MakerCategory("03_ClothesTop", "tglGloves", 50),
            new MakerCategory("03_ClothesTop", "tglPanst", 60),
            new MakerCategory("03_ClothesTop", "tglSocks", 70),
            new MakerCategory("03_ClothesTop", "tglInnerShoes", 80),
            new MakerCategory("03_ClothesTop", "tglOuterShoes", 90),
            new MakerCategory("03_ClothesTop", "tglCopy", 100),

            new MakerCategory("05_ParameterTop", "tglCharactor", 10),
            new MakerCategory("05_ParameterTop", "tglCharactorEx", 20),
            new MakerCategory("05_ParameterTop", "tglH", 30),
            new MakerCategory("05_ParameterTop", "tglQA", 40),
            new MakerCategory("05_ParameterTop", "tglAttribute", 50),
            new MakerCategory("05_ParameterTop", "tglADK", 60),
        };

        public static IEnumerable<MakerCategory> BuiltInCategories => _builtInCategories;

        public static MakerCategory GetBuiltInCategory(string category, string subCategory)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));
            if (subCategory == null)
                throw new ArgumentNullException(nameof(subCategory));

            return _builtInCategories.First(x => x.CategoryName.Equals(category) && x.SubCategoryName.Equals(subCategory));
        }

        public static class Face
        {
            public static MakerCategory All { get { return GetBuiltInCategory("00_FaceTop", "tglAll"); } }
            public static MakerCategory Ear { get { return GetBuiltInCategory("00_FaceTop", "tglEar"); } }
            public static MakerCategory Chin { get { return GetBuiltInCategory("00_FaceTop", "tglChin"); } }
            public static MakerCategory Cheek { get { return GetBuiltInCategory("00_FaceTop", "tglCheek"); } }
            public static MakerCategory Eyebrow { get { return GetBuiltInCategory("00_FaceTop", "tglEyebrow"); } }
            public static MakerCategory Eye { get { return GetBuiltInCategory("00_FaceTop", "tglEye01"); } }
            public static MakerCategory Iris { get { return GetBuiltInCategory("00_FaceTop", "tglEye02"); } }
            public static MakerCategory Nose { get { return GetBuiltInCategory("00_FaceTop", "tglNose"); } }
            public static MakerCategory Mouth { get { return GetBuiltInCategory("00_FaceTop", "tglMouth"); } }
            public static MakerCategory Mole { get { return GetBuiltInCategory("00_FaceTop", "tglMole"); } }
            public static MakerCategory Makeup { get { return GetBuiltInCategory("00_FaceTop", "tglMakeup"); } }
            public static MakerCategory Shape { get { return GetBuiltInCategory("00_FaceTop", "tglShape"); } }
        }
        public static class Body
        {
            public static MakerCategory All { get { return GetBuiltInCategory("01_BodyTop", "tglAll"); } }
            public static MakerCategory Breast { get { return GetBuiltInCategory("01_BodyTop", "tglBreast"); } }
            public static MakerCategory Upper { get { return GetBuiltInCategory("01_BodyTop", "tglUpper"); } }
            public static MakerCategory Lower { get { return GetBuiltInCategory("01_BodyTop", "tglLower"); } }
            public static MakerCategory Arm { get { return GetBuiltInCategory("01_BodyTop", "tglArm"); } }
            public static MakerCategory Leg { get { return GetBuiltInCategory("01_BodyTop", "tglLeg"); } }
            public static MakerCategory Nail { get { return GetBuiltInCategory("01_BodyTop", "tglNail"); } }
            public static MakerCategory Underhair { get { return GetBuiltInCategory("01_BodyTop", "tglUnderhair"); } }
            public static MakerCategory Sunburn { get { return GetBuiltInCategory("01_BodyTop", "tglSunburn"); } }
            public static MakerCategory Paint { get { return GetBuiltInCategory("01_BodyTop", "tglPaint"); } }
            public static MakerCategory Shape { get { return GetBuiltInCategory("01_BodyTop", "tglShape"); } }
        }
        public static class Hair
        {
            public static MakerCategory Common { get { return GetBuiltInCategory("02_HairTop", "common"); } }
            public static MakerCategory Back { get { return GetBuiltInCategory("02_HairTop", "tglBack"); } }
            public static MakerCategory Front { get { return GetBuiltInCategory("02_HairTop", "tglFront"); } }
            public static MakerCategory Side { get { return GetBuiltInCategory("02_HairTop", "tglSide"); } }
            public static MakerCategory Extension { get { return GetBuiltInCategory("02_HairTop", "tglExtension"); } }
            public static MakerCategory Etc { get { return GetBuiltInCategory("02_HairTop", "tglEtc"); } }
        }
        public static class Clothes
        {
            public static MakerCategory Top { get { return GetBuiltInCategory("03_ClothesTop", "tglTop"); } }
            public static MakerCategory Bottom { get { return GetBuiltInCategory("03_ClothesTop", "tglBot"); } }
            public static MakerCategory Bra { get { return GetBuiltInCategory("03_ClothesTop", "tglBra"); } }
            public static MakerCategory Shorts { get { return GetBuiltInCategory("03_ClothesTop", "tglShorts"); } }
            public static MakerCategory Gloves { get { return GetBuiltInCategory("03_ClothesTop", "tglGloves"); } }
            public static MakerCategory Panst { get { return GetBuiltInCategory("03_ClothesTop", "tglPanst"); } }
            public static MakerCategory Socks { get { return GetBuiltInCategory("03_ClothesTop", "tglSocks"); } }
            public static MakerCategory InnerShoes { get { return GetBuiltInCategory("03_ClothesTop", "tglInnerShoes"); } }
            public static MakerCategory OuterShoes { get { return GetBuiltInCategory("03_ClothesTop", "tglOuterShoes"); } }
            public static MakerCategory Copy { get { return GetBuiltInCategory("03_ClothesTop", "tglCopy"); } }
        }
        public static class Parameter
        {
            public static MakerCategory Character { get { return GetBuiltInCategory("05_ParameterTop", "tglCharactor"); } }
            public static MakerCategory CharacterEx { get { return GetBuiltInCategory("05_ParameterTop", "tglCharactorEx"); } }
            public static MakerCategory H { get { return GetBuiltInCategory("03_ClothesTop", "tglH"); } }
            public static MakerCategory QA { get { return GetBuiltInCategory("05_ParameterTop", "tglQA"); } }
            public static MakerCategory Attribute { get { return GetBuiltInCategory("05_ParameterTop", "tglAttribute"); } }
            public static MakerCategory ADK { get { return GetBuiltInCategory("05_ParameterTop", "tglADK"); } }
        }
    }
}