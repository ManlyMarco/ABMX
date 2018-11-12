using System.Collections.Generic;
using System.Reflection;
using Studio;
using UnityEngine;

namespace KKABMX.Core
{
    public static class Utilities
    {
        //public static readonly string MakerDefaultFileName = "ill_default_female.png";
        public const int ManualBoneId = -1;

        private static readonly FieldInfo FieldPvCopyBone = typeof(PVCopy).GetField("bone",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly FieldInfo FieldPvCopyPv = typeof(PVCopy).GetField("pv",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly PropertyInfo FieldChaControlSibBody = typeof(ChaControl).GetProperty("sibBody",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

        private static readonly PropertyInfo FieldChaControlSibFace = typeof(ChaControl).GetProperty("sibFace",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

        private static readonly FieldInfo FieldShapeInfoBaseDictDst = typeof(ShapeInfoBase).GetField("dictDst",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

        public static GameObject[] GetBoneArray(this PVCopy pvCopy)
        {
            return (GameObject[])FieldPvCopyBone.GetValue(pvCopy);
        }

        public static GameObject[] GetPvArray(this PVCopy pvCopy)
        {
            return (GameObject[])FieldPvCopyPv.GetValue(pvCopy);
        }

        public static ShapeInfoBase GetSibFace(this ChaControl chaControl)
        {
            return (ShapeInfoBase)FieldChaControlSibFace.GetValue(chaControl, null);
        }

        public static ShapeInfoBase GetSibBody(this ChaControl chaControl)
        {
            return (ShapeInfoBase)FieldChaControlSibBody.GetValue(chaControl, null);
        }

        public static Dictionary<int, ShapeInfoBase.BoneInfo> GetDictDst(this ShapeInfoBase sibBody)
        {
            return (Dictionary<int, ShapeInfoBase.BoneInfo>)FieldShapeInfoBaseDictDst.GetValue(sibBody);
        }

        /// <summary>
        /// Bones that have separate values for each coordinate / uniform type
        /// </summary>
        public static readonly string[] CoordinateBoneNames =
        {
            "cf_d_sk_top",
            "cf_d_sk_00_00",
            "cf_d_sk_07_00",
            "cf_d_sk_06_00",
            "cf_d_sk_05_00",
            "cf_d_sk_04_00",
            "cf_d_sk_01_00",
            "cf_d_sk_02_00",
            "cf_d_sk_03_00"
        };

        public static readonly int[] ScaleBodyBonesF =
        {
            0,
            1,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            2,
            20,
            21,
            22,
            23,
            24,
            25,
            26,
            27,
            28,
            29,
            3,
            30,
            31,
            32,
            33,
            34,
            35,
            36,
            37,
            4,
            40,
            41,
            42,
            43,
            44,
            45,
            46,
            47,
            48,
            49,
            5,
            50,
            51,
            52,
            54,
            55,
            56,
            57,
            58,
            59,
            6,
            60,
            61,
            62,
            63,
            64,
            65,
            66,
            67,
            68,
            69,
            7,
            70,
            71,
            72,
            73,
            74,
            75,
            76,
            77,
            8,
            9
        };

        public static readonly int[] ScaleBodyBonesM = new int[0];

        public static readonly int[] ScaleFaceBonesF =
        {
            0,
            10,
            11,
            12,
            22,
            23,
            3,
            47,
            48,
            49,
            5,
            50,
            6,
            7,
            8,
            9
        };

        public static readonly int[] ScaleFaceBonesM = new int[0];
    }
}