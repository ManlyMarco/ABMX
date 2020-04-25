using MessagePack;
using UnityEngine;

namespace BoneModHarmony
{
    /// <summary>
    /// Code borrowed from BoneModHarmony
    /// </summary>
    [MessagePackObject(true)]
    internal class BoneModifier
    {
        public BoneModifier(string boneName = "")
        {
            this.boneName = boneName;
        }

        [SerializationConstructor]
        public BoneModifier(string boneName, Vector3 Scale, Vector3 Rotation, Vector3 Position, bool isScale, bool isRotate, bool isPosition)
        {
            this.boneName = boneName;
            this.Scale = Scale;
            this.Rotation = Rotation;
            this.Position = Position;
            this.isScale = isScale;
            this.isRotate = isRotate;
            this.isPosition = isPosition;
        }

        public void PasteValue(BoneModifier modifier)
        {
            this.Scale = modifier.Scale;
            this.Rotation = modifier.Rotation;
            this.Position = modifier.Position;
            this.isScale = modifier.isScale;
            this.isRotate = modifier.isRotate;
            this.isPosition = modifier.isPosition;
        }

        public BoneModifier Clone()
        {
            return (BoneModifier)base.MemberwiseClone();
        }

        public string boneName;

        public Vector3 Scale = Vector3.one;

        public Vector3 Rotation = Vector3.zero;

        public Vector3 Position = Vector3.zero;

        public bool isScale;

        public bool isRotate;

        public bool isPosition;
    }
}