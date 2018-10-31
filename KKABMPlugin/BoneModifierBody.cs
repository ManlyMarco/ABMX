using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace KKABMPlugin
{
    public class BoneModifierBody
    {
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


        private bool _enabled;


        public readonly int boneIndex;


        public string boneName;


        private bool hasBaseline;


        public bool isScaleBone;

        /*
        private Vector3 lastPos;


        private Vector3 lastRot;


        private Vector3 lastScl;*/


        public float lenBaseline;


        public float lenMod = 1f;


        public Transform manualTarget;


        public Vector3 sclBaseline = Vector3.one;


        public Vector3 sclMod = Vector3.one;


        public readonly ShapeInfoBase shapeInfoBase;
        private readonly ShapeInfoBase.BoneInfo _boneInfo;

        // (get) Token: 0x06000023 RID: 35 RVA: 0x00002D5B File Offset: 0x00000F5B
        // (set) Token: 0x06000024 RID: 36 RVA: 0x00002D63 File Offset: 0x00000F63
        public bool enabled
        {
            get => _enabled;
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
                        target.localPosition =
                            target.localPosition / target.localPosition.magnitude * lenBaseline * lenMod;
                }
            }
        }


        public void Clear()
        {
            enabled = false;
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

        public BoneModifierBody(int boneIndex, ShapeInfoBase sib)
        {
            this.boneIndex = boneIndex;
            shapeInfoBase = sib;
            isNotManual = boneIndex != -1;

            if (isNotManual && shapeInfoBase != null && GetDict(shapeInfoBase).ContainsKey(boneIndex))
                _boneInfo = GetDict(shapeInfoBase)[boneIndex];
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
                    enabled = false,
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
                    enabled = false,
                    sclMod = Vector3.one,
                    isScaleBone = IsScaleBone(boneInfo, sibFace, key)
                };
                result.Add(boneModifierBody.boneName, boneModifierBody);
            }
        }


        private Transform GetTarget()
        {
            if (boneIndex == -1)
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
                if (isNotManual)
                {/*
                    var boneInfo = _boneInfo;
                    lastPos = boneInfo.vctPos;
                    lastScl = boneInfo.vctScl;
                    lastRot = boneInfo.vctRot;*/
                }
            }
        }

        public readonly bool isNotManual;

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