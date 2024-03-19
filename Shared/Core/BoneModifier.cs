using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;

#if KK || KKS
using CoordinateType = ChaFileDefine.CoordinateType;
#elif EC
using CoordinateType = KoikatsuCharaFile.ChaFileDefine.CoordinateType;
#endif

namespace KKABMX.Core
{
    /// <summary>
    /// Class that handles applying modifiers to bones
    /// </summary>
    [MessagePackObject]
    public sealed class BoneModifier
    {
        private bool _hasBaseline;
        internal Vector3 _sclBaseline = Vector3.one;
        internal Vector3 _posBaseline = Vector3.zero;
        internal Quaternion _rotBaseline;

        private bool _changedScale, _changedRotation, _changedPosition;

        private bool _lenModForceUpdate;
        private bool _forceApply;

        /// <summary> Use other overloads instead </summary>
        [Obsolete]
        public BoneModifier(string boneName) : this(boneName, BoneLocation.Unknown, new[] { new BoneModifierData() }) { }

        /// <summary> Use other overloads instead </summary>
        [Obsolete]
        public BoneModifier(string boneName, BoneModifierData[] coordinateModifiers) : this(boneName, BoneLocation.Unknown, coordinateModifiers) { }

        /// <summary>
        /// Create empty modifier that is not coordinate specific
        /// </summary>
        /// <param name="boneName">Name of the bone transform to affect</param>
        /// <param name="boneLocation">Where the bone transform to affect is located</param>
        public BoneModifier(string boneName, BoneLocation boneLocation) : this(boneName, boneLocation, new[] { new BoneModifierData() }) { }

        /// <summary>
        /// Create empty modifier
        /// </summary>
        /// <param name="boneName">Name of the bone transform to affect</param>
        /// <param name="boneLocation">Where the bone transform to affect is located</param>
        /// <param name="coordinateModifiers">
        /// Needs to be either 1 long to apply to all coordinates or 7 to apply to specific
        /// coords
        /// </param>
        public BoneModifier(string boneName, BoneLocation boneLocation, BoneModifierData[] coordinateModifiers)
        {
            if (string.IsNullOrEmpty(boneName))
                throw new ArgumentException("Invalid boneName - " + boneName, nameof(boneName));
            if (coordinateModifiers == null)
                throw new ArgumentNullException(nameof(coordinateModifiers));
            if (coordinateModifiers.Length < 1)
                throw new ArgumentException("Need at least 1 element in coordinateModifiers", nameof(coordinateModifiers));
            if (coordinateModifiers.Any(x => x == null))
                throw new ArgumentException("coordinateModifiers can't have any nulls in it", nameof(coordinateModifiers));

            BoneName = boneName;
            BoneLocation = boneLocation;
            CoordinateModifiers = coordinateModifiers.ToArray();
        }

        /// <summary> Use other overloads instead </summary>
        [SerializationConstructor, Obsolete("Only for deserialization", true)]
        public BoneModifier(string boneName, BoneModifierData[] coordinateModifiers, BoneLocation boneLocation) : this(boneName, boneLocation, coordinateModifiers) { }

        /// <summary>
        /// Name of the targetted bone
        /// </summary>
        [Key(0)]
        public string BoneName { get; }

        /// <summary>
        /// Transform of the targetted bone
        /// </summary>
        [IgnoreMember]
        public Transform BoneTransform { get; internal set; }

        /// <summary>
        /// Actual modifier values, split for different coordinates if required
        /// </summary>
        [Key(1)]
        // Needs a public set to make serializing work
        public BoneModifierData[] CoordinateModifiers { get; set; }

        /// <summary>
        /// What part of the character the bone is on.
        /// </summary>
        [Key(2)]
        public BoneLocation BoneLocation { get; internal set; }

        /// <summary>
        /// Apply the modifiers
        /// </summary>
        public void Apply(CoordinateType coordinate, IList<BoneModifierData> additionalModifiers)
        {
            if (BoneTransform == null || !_hasBaseline) return;

            var modifier = GetModifier(coordinate);

            if (additionalModifiers != null && additionalModifiers.Count > 0)
                modifier = CombineModifiers(modifier, additionalModifiers);
            else if (modifier == null)
                return;

            if (!CanApply(modifier)) return;

            if (modifier.HasScale())
            {
                BoneTransform.localScale = new Vector3(
                    _sclBaseline.x * modifier.ScaleModifier.x,
                    _sclBaseline.y * modifier.ScaleModifier.y,
                    _sclBaseline.z * modifier.ScaleModifier.z);
                _changedScale = true;
            }
            else if (_changedScale)
            {
                BoneTransform.localScale = _sclBaseline;
                _changedScale = false;
            }

            if (modifier.HasRotation()/* && !KKABMX_Core.NoRotationBones.Contains(BoneTransform.name)*/)
            {
                // Multiplying Quaternions has same effect as applying them in order
                BoneTransform.localRotation = _rotBaseline * Quaternion.Euler(modifier.RotationModifier);
                _changedRotation = true;
            }
            else if (_changedRotation)
            {
                BoneTransform.localRotation = _rotBaseline;
                _changedRotation = false;
            }

            if (_lenModForceUpdate || modifier.HasLength())
            {
                BoneTransform.localPosition = _posBaseline * modifier.LengthModifier + modifier.PositionModifier;
                _lenModForceUpdate = false;
                _changedPosition = true;
            }
            else if (modifier.HasPosition())
            {
                BoneTransform.localPosition = _posBaseline + modifier.PositionModifier;
                _changedPosition = true;
            }
            else if (_changedPosition)
            {
                BoneTransform.localPosition = _posBaseline;
                _changedPosition = false;
            }
        }

        private readonly BoneModifierData _combineModifiersCachedReturn = new BoneModifierData();
        private BoneModifierData CombineModifiers(BoneModifierData baseModifier, IList<BoneModifierData> additionalModifiers)
        {
            var scale = baseModifier?.ScaleModifier ?? Vector3.one;
            var len = baseModifier?.LengthModifier ?? 1f;
            var position = baseModifier?.PositionModifier ?? Vector3.zero;
            var rotation = baseModifier?.RotationModifier ?? Vector3.zero;

            for (var i = 0; i < additionalModifiers.Count; i++)
            {
                var additionalModifier = additionalModifiers[i];
                scale = new Vector3(
                    scale.x * additionalModifier.ScaleModifier.x,
                    scale.y * additionalModifier.ScaleModifier.y,
                    scale.z * additionalModifier.ScaleModifier.z);
                len *= additionalModifier.LengthModifier;

                position += additionalModifier.PositionModifier;
                rotation += additionalModifier.RotationModifier;
            }

            _combineModifiersCachedReturn.ScaleModifier = scale;
            _combineModifiersCachedReturn.LengthModifier = len;
            _combineModifiersCachedReturn.PositionModifier = position;
            _combineModifiersCachedReturn.RotationModifier = rotation;
            return _combineModifiersCachedReturn;
        }

        /// <summary>
        /// Set current values of the bone as its default/base values.
        /// Warning: Do not call after the modifier was applied, it has to be reset first!
        /// </summary>
        public void CollectBaseline()
        {
            if (BoneTransform == null) return;

            _sclBaseline = BoneTransform.localScale;
            _posBaseline = BoneTransform.localPosition;
            _rotBaseline = BoneTransform.localRotation;

            _hasBaseline = true;
        }

        /// <summary>
        /// Get data for a specific coordinate
        /// </summary>
        public BoneModifierData GetModifier(CoordinateType coordinate)
        {
            if (!IsCoordinateSpecific()) return CoordinateModifiers[0];
            if (CoordinateModifiers.Length <= (int)coordinate)
            {
#if DEBUG
                Console.WriteLine($"CoordinateModifiers.Length={CoordinateModifiers.Length} <= (int)coordinate={(int)coordinate}");
#endif
                return null;
            }

            return CoordinateModifiers[(int)coordinate];
        }

        /// <summary>
        /// Check if this modifier has any data in it that can be applied
        /// </summary>
        public bool IsEmpty()
        {
            for (var i = 0; i < CoordinateModifiers.Length; i++)
            {
                if (!CoordinateModifiers[i].IsEmpty())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if this modifier has unique values for each coordinate, or one set of values for all coordinates
        /// </summary>
        public bool IsCoordinateSpecific()
        {
#if AI || HS2
            // No coordinate saving in AIS
            return false;
#else
            return CoordinateModifiers.Length > 1;
#endif
        }

        /// <summary>
        /// If this modifier is not coordinate specific, make it coordinate specific (one set of values for each outfit)
        /// </summary>
        public void MakeCoordinateSpecific(int coordinateCount)
        {
            if (coordinateCount <= 1) throw new ArgumentOutOfRangeException(nameof(coordinateCount), "Must be more than 1");

            if (!IsCoordinateSpecific())
                CoordinateModifiers = Enumerable.Range(0, coordinateCount).Select(_ => CoordinateModifiers[0].Clone()).ToArray();
            else if (coordinateCount != CoordinateModifiers.Length)
                OnCoordinateCountChanged(coordinateCount);
        }

        private void OnCoordinateCountChanged(int coordinateCount)
        {
            //if (!IsCoordinateSpecific()) return;

            // Trim slots if there's less than before
            var modifiers = CoordinateModifiers.Take(coordinateCount);

            // Add extra slots if there's more than before
            var additionalCount = coordinateCount - CoordinateModifiers.Length;
            if (additionalCount > 0)
                modifiers = modifiers.Concat(Enumerable.Range(0, additionalCount).Select(_ => new BoneModifierData()));

            CoordinateModifiers = modifiers.ToArray();
        }

        /// <summary>
        /// If this modifier is coordinate specific, make it not coordinate specific (one set of values for all outfits)
        /// </summary>
        public void MakeNonCoordinateSpecific()
        {
            if (CoordinateModifiers.Length > 1)
                CoordinateModifiers = new[] { CoordinateModifiers[0] };
        }

        /// <summary>
        /// Resets bone transform values to their original values
        /// </summary>
        public void Reset()
        {
            if (BoneTransform == null) return;

            if (_hasBaseline)
            {
                BoneTransform.localScale = _sclBaseline;
                BoneTransform.localRotation = _rotBaseline;
                BoneTransform.localPosition = _posBaseline;
            }
        }

        private bool CanApply(BoneModifierData data)
        {
            if (!data.IsEmpty())
            {
                _forceApply = true;
                return true;
            }

            // The point is to let the boner run for 1 extra frame after it's disabled to properly reset stuff
            if (_forceApply)
            {
                _forceApply = false;
                _lenModForceUpdate = true;
                return true;
            }

            if (_lenModForceUpdate)
                return true;

            return false;
        }

        /// <summary>
        /// Create a copy of this modifier
        /// </summary>
        public BoneModifier Clone()
        {
            return new BoneModifier(BoneName, BoneLocation, CoordinateModifiers.Select(x => x.Clone()).ToArray());
        }

        /// <summary>
        /// Check if length can be applied in current state
        /// </summary>
        public bool CanApplyLength()
        {
            if (_hasBaseline) return _posBaseline != Vector3.zero;
            if (BoneTransform != null) return BoneTransform.position != Vector3.zero;
            return false;
        }

        /// <summary>
        /// Clear the stored baseline, if any
        /// </summary>
        public void ClearBaseline()
        {
            _hasBaseline = false;
        }
    }
}
