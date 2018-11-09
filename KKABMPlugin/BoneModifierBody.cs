using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KKABMX.Core
{
    // ReSharper disable InconsistentNaming
    public class BoneModifierBody
    {
        public const int ManualBoneId = -1;

        private static readonly FieldInfo dictDstBoneInfo = typeof(ShapeInfoBase).GetField("dictDst",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

        private static readonly int[] scaleBodyBonesF =
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

        private static readonly int[] scaleBodyBonesM = new int[0];

        private static readonly int[] scaleFaceBonesF =
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

        private static readonly int[] scaleFaceBonesM = new int[0];

        private readonly ShapeInfoBase.BoneInfo _boneInfo;
        public readonly ShapeInfoBase shapeInfoBase;

        public readonly int boneIndex;
        public string boneName;
        public bool isScaleBone;
        public Transform manualTarget;
        public readonly bool isNotManual;

        private bool _enabled = true;

        private bool hasBaseline;
        public float lenBaseline;
        public float lenMod = 1f;
        public Vector3 sclBaseline = Vector3.one;
        public Vector3 sclMod = Vector3.one;

        /*
        private Vector3 lastPos;
        private Vector3 lastRot;
        private Vector3 lastScl;
        */

        public BoneModifierBody(int boneIndex, ShapeInfoBase sib)
        {
            this.boneIndex = boneIndex;
            shapeInfoBase = sib;
            isNotManual = boneIndex != ManualBoneId;

            if (isNotManual && shapeInfoBase != null && GetDict(shapeInfoBase).ContainsKey(boneIndex))
                _boneInfo = GetDict(shapeInfoBase)[boneIndex];
        }

        public bool enabled
        {
            get
            {
                if (!Vector3.one.Equals(sclMod))
                    _enabled = true;
                else
                {
                    if (_enabled)
                        _enabled = false;
                    else
                        return false;
                }
                //return _enabled;
                return true;
            }
            set
            {
                if (_enabled && !value)
                    Reset();
                _enabled = value;
            }
        }

        public void Apply()
        {
            if (enabled)
            {
                var target = GetTarget();
                if (target != null)
                {
                    var localScale = new Vector3(sclBaseline.x * sclMod.x, sclBaseline.y * sclMod.y,
                        sclBaseline.z * sclMod.z);
                    target.localScale = localScale;
                    if (lenMod != 1f && target.localPosition != Vector3.zero && lenBaseline != 0f)
                        target.localPosition = target.localPosition / target.localPosition.magnitude * lenBaseline * lenMod;
                }
            }
        }

        public void Clear()
        {
            enabled = true;
            sclMod = Vector3.one;
            lenMod = 1f;
        }

        public void Reset()
        {
            var target = GetTarget();
            if (target != null && enabled && hasBaseline)
            {
                target.localScale = sclBaseline;
                if (lenMod != 1f && target.localPosition != Vector3.zero && lenBaseline != 0f)
                    target.localPosition = target.localPosition / target.localPosition.magnitude * lenBaseline;
            }
        }

        public static SortedDictionary<string, BoneModifierBody> CreateListForBody(ShapeInfoBase sibBody)
        {
            var sortedDictionary = new SortedDictionary<string, BoneModifierBody>();
            var dict = GetDict(sibBody);
            foreach (var key in dict.Keys)
            {
                var boneInfo = dict[key];
                var boneModifierBody = new BoneModifierBody(key, sibBody)
                {
                    boneName = boneInfo.trfBone.name,
                    //enabled = false,
                    sclMod = Vector3.one,
                    isScaleBone = IsScaleBone(boneInfo, sibBody, key)
                };
                sortedDictionary.Add(boneModifierBody.boneName, boneModifierBody);
            }
            return sortedDictionary;
        }

        public static void AddFaceBones(ShapeInfoBase sibFace, SortedDictionary<string, BoneModifierBody> result)
        {
            var dict = GetDict(sibFace);
            foreach (var key in dict.Keys)
            {
                var boneInfo = dict[key];
                var boneModifierBody = new BoneModifierBody(key, sibFace)
                {
                    boneName = boneInfo.trfBone.name,
                    //enabled = false,
                    sclMod = Vector3.one,
                    isScaleBone = IsScaleBone(boneInfo, sibFace, key)
                };
                result.Add(boneModifierBody.boneName, boneModifierBody);
            }
        }

        private Transform GetTarget()
        {
            if (boneIndex == ManualBoneId)
                return manualTarget;
            return _boneInfo.trfBone;
        }

        private static Dictionary<int, ShapeInfoBase.BoneInfo> GetDict(ShapeInfoBase sibBody)
        {
            return dictDstBoneInfo.GetValue(sibBody) as Dictionary<int, ShapeInfoBase.BoneInfo>;
        }

        public void CollectBaseline()
        {
            var target = GetTarget();
            if (target != null)
            {
                sclBaseline = target.localScale;
                lenBaseline = target.localPosition.magnitude;
                hasBaseline = true;
                /*if (isNotManual)
                {
                    var boneInfo = _boneInfo;
                    lastPos = boneInfo.vctPos;
                    lastScl = boneInfo.vctScl;
                    lastRot = boneInfo.vctRot;
                }*/
            }
        }

        public bool CheckBaselineChanged()
        {
            if (isNotManual && isScaleBone)
            {
                var boneInfo = _boneInfo;
                if (boneInfo != null && boneInfo.trfBone != null && sclBaseline != boneInfo.trfBone.localScale)
                {
                    if (_enabled)
                    {
                        var vector = new Vector3(sclBaseline.x * sclMod.x, sclBaseline.y * sclMod.y,
                            sclBaseline.z * sclMod.z);
                        if (boneInfo.trfBone.localScale == vector)
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }

        private static bool IsScaleBone(ShapeInfoBase.BoneInfo boneInfo, ShapeInfoBase sibBody, int boneIndex)
        {
            if (boneInfo == null || boneInfo.trfBone == null)
                return false;
            //boneInfo.trfBone.name.Contains("_s_");
            int[] array = null;
            if (sibBody is ShapeBodyInfoFemale)
                array = scaleBodyBonesF;
            else if (sibBody is ShapeBodyInfoMale)
                array = scaleBodyBonesM;
            else if (sibBody is ShapeHeadInfoFemale)
                array = scaleFaceBonesF;
            else if (sibBody is ShapeHeadInfoMale)
                array = scaleFaceBonesM;
            return array != null && array.Contains(boneIndex);
        }
    }
}