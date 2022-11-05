using MessagePack;
using UnityEngine;

namespace KKABMX.Core
{
    /// <summary>
    /// Values applied to a bone to modify its scale, position and/or location.
    /// </summary>
    [MessagePackObject]
    public sealed class BoneModifierData
    {
        /// <summary>
        /// Empty data, same as creating a new instance.
        /// </summary>
        public static BoneModifierData Default => new BoneModifierData();

        /// <summary>
        /// Added to localScale
        /// </summary>
        [Key(0)]
        public Vector3 ScaleModifier;

        /// <summary>
        /// Scales transform's position from the parent transform
        /// </summary>
        [Key(1)]
        public float LengthModifier;

        /// <summary>
        /// Added to localPosition
        /// </summary>
        [Key(2)]
        public Vector3 PositionModifier;

        /// <summary>
        /// Added to localRotation
        /// </summary>
        [Key(3)]
        public Vector3 RotationModifier;

        /// <summary>
        /// Create an empty modifier
        /// </summary>
        public BoneModifierData() : this(Vector3.one, 1, Vector3.zero, Vector3.zero) { }
        /// <summary>
        /// Create a legacy modifier
        /// </summary>
        public BoneModifierData(Vector3 scaleModifier, float lengthModifier) : this(scaleModifier, lengthModifier, Vector3.zero, Vector3.zero) { }
        /// <summary>
        /// Create a new modifier
        /// </summary>
        public BoneModifierData(Vector3 scaleModifier, float lengthModifier, Vector3 positionModifier, Vector3 rotationModifier)
        {
            ScaleModifier = scaleModifier;
            LengthModifier = lengthModifier;
            PositionModifier = positionModifier;
            RotationModifier = rotationModifier;
        }

        /// <summary>
        /// Create a copy of this modifier
        /// </summary>
        /// <returns></returns>
        public BoneModifierData Clone()
        {
            return (BoneModifierData)MemberwiseClone();
        }

        /// <summary>
        /// Length is not empty
        /// </summary>
        public bool HasLength()
        {
            return LengthModifier != 1;
        }

        /// <summary>
        /// Scale is not empty
        /// </summary>
        public bool HasScale()
        {
            return ScaleModifier.x != 1 || ScaleModifier.y != 1 || ScaleModifier.z != 1;
        }

        /// <summary>
        /// Position is not empty
        /// </summary>
        public bool HasPosition()
        {
            return PositionModifier.x != 0 || PositionModifier.y != 0 || PositionModifier.z != 0;
        }

        /// <summary>
        /// Rotation is not empty
        /// </summary>
        public bool HasRotation()
        {
            return RotationModifier.x != 0 || RotationModifier.y != 0 || RotationModifier.z != 0;
        }

        /// <summary>
        /// True if all data in this modifier is empty/default
        /// </summary>
        public bool IsEmpty()
        {
            return ScaleModifier.x == 1 && ScaleModifier.y == 1 && ScaleModifier.z == 1 &&
                PositionModifier.x == 0 && PositionModifier.y == 0 && PositionModifier.z == 0 &&
                RotationModifier.x == 0 && RotationModifier.y == 0 && RotationModifier.z == 0 &&
                LengthModifier == 1;
        }

        /// <summary>
        /// Empty all data in this modifier
        /// </summary>
        public void Clear()
        {
            ScaleModifier = Vector3.one;
            RotationModifier = Vector3.zero;
            PositionModifier = Vector3.zero;
            LengthModifier = 1;
        }

        /// <summary>
        /// Copy data from this modifier to the other modifier
        /// </summary>
        public void CopyTo(BoneModifierData other)
        {
            other.ScaleModifier = ScaleModifier;
            other.RotationModifier = RotationModifier;
            other.PositionModifier = PositionModifier;
            other.LengthModifier = LengthModifier;
        }
    }
}
