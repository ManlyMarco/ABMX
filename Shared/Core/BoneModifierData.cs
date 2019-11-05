using System;
using MessagePack;
using UnityEngine;

namespace KKABMX.Core
{
    [MessagePackObject]
    public sealed class BoneModifierData
    {
        public static readonly BoneModifierData Default = new BoneModifierData();

        [Key(0)]
        public Vector3 ScaleModifier;

        [Key(1)]
        public float LengthModifier;

        public BoneModifierData() : this(Vector3.one, 1) { }

        public BoneModifierData(Vector3 scaleModifier, float lengthModifier)
        {
            ScaleModifier = scaleModifier;
            LengthModifier = lengthModifier;
        }

        public BoneModifierData Clone()
        {
            return (BoneModifierData) MemberwiseClone();
        }

        public bool HasLength()
        {
            return Math.Abs(LengthModifier - 1f) > 0.001f;
        }

        public bool HasScale()
        {
            return ScaleModifier != Vector3.one;
        }

        public bool IsEmpty()
        {
            return !HasLength() && !HasScale();
        }

        public void Clear()
        {
            ScaleModifier = Vector3.one;
            LengthModifier = 1;
        }
    }
}
