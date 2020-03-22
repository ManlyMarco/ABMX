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

        [Key(2)]
        public Vector3 PositionModifier;

        [Key(3)]
        public Vector3 RotationModifier;

        [IgnoreMember]
        public bool scaleSymmetry = true;
        [IgnoreMember]
        public bool rotSymmetry = true;
        [IgnoreMember]
        public bool posSymmetry = true;

        public BoneModifierData() : this(Vector3.one, 1, Vector3.zero, Vector3.zero) { }

        public BoneModifierData(Vector3 scaleModifier, float lengthModifier, Vector3 positionModifier, Vector3 rotationModifier)
        {
            ScaleModifier = scaleModifier;
            LengthModifier = lengthModifier;
            PositionModifier = positionModifier;
            RotationModifier = rotationModifier;
        }

        public BoneModifierData Clone()
        {
            return (BoneModifierData)MemberwiseClone();
        }

        public bool HasLength()
        {
            return LengthModifier != 1;
        }

        public bool HasScale()
        {
            return ScaleModifier != Vector3.one || PositionModifier != Vector3.zero || RotationModifier != Vector3.zero;
        }


        public bool IsEmpty()
        {
            return ScaleModifier == Vector3.one && PositionModifier == Vector3.zero && RotationModifier == Vector3.zero && LengthModifier == 1;
        }

        public void Clear()
        {
            ScaleModifier = Vector3.one;
            RotationModifier = Vector3.zero;
            PositionModifier = Vector3.zero;
            LengthModifier = 1;
        }
    }
}
