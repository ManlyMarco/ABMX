using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KKABMX.Core
{
    public static class Utilities
    {
        //public static readonly string MakerDefaultFileName = "ill_default_female.png";
        public const int ManualBoneId = -1;

#if KK
        private static readonly FieldInfo FieldPvCopyBone = typeof(Studio.PVCopy).GetField("bone",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly FieldInfo FieldPvCopyPv = typeof(Studio.PVCopy).GetField("pv",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public static GameObject[] GetBoneArray(this Studio.PVCopy pvCopy)
        {
            return (GameObject[])FieldPvCopyBone.GetValue(pvCopy);
        }

        public static GameObject[] GetPvArray(this Studio.PVCopy pvCopy)
        {
            return (GameObject[])FieldPvCopyPv.GetValue(pvCopy);
        }
#endif

        private static readonly PropertyInfo FieldChaControlSibBody = typeof(ChaControl).GetProperty("sibBody",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

        private static readonly PropertyInfo FieldChaControlSibFace = typeof(ChaControl).GetProperty("sibFace",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

        private static readonly FieldInfo FieldShapeInfoBaseDictDst = typeof(ShapeInfoBase).GetField("dictDst",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

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
    }
}