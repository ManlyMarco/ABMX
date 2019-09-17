using System.Collections.Generic;
using System.Reflection;
using AIChara;
using UnityEngine;

namespace KKABMX.Core
{
    public static class Utilities
    {
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