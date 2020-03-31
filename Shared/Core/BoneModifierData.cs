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
        
        public BoneModifierData() : this(Vector3.one, 1, Vector3.zero, Vector3.zero) { }
        public BoneModifierData(Vector3 scaleModifier, float lengthModifier) : this(scaleModifier, lengthModifier, Vector3.zero, Vector3.zero) { }
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
            return ScaleModifier.x != 1 || ScaleModifier.y != 1 || ScaleModifier.z != 1;
        }

        public bool HasPosition()
        {
            return PositionModifier.x != 0 || PositionModifier.y != 0 || PositionModifier.z != 0;
        }

        public bool HasRotation()
        {
            return RotationModifier.x != 0 || RotationModifier.y != 0 || RotationModifier.z != 0;
        }

        public bool IsEmpty()
        {
            return ScaleModifier.x == 1 && ScaleModifier.y == 1 && ScaleModifier.z == 1 &&
                PositionModifier.x == 0 && PositionModifier.y == 0 && PositionModifier.z == 0 &&
                RotationModifier.x == 0 && RotationModifier.y == 0 && RotationModifier.z == 0 &&
                LengthModifier == 1;
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
