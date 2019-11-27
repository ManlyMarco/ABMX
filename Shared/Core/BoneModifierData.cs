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

        public bool IsEmpty()
        {
            return ScaleModifier.x == 1 && ScaleModifier.y == 1 && ScaleModifier.z == 1 && LengthModifier == 1;
        }

        public void Clear()
        {
            ScaleModifier = Vector3.one;
            LengthModifier = 1;
        }
    }
}
