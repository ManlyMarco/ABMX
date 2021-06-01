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
    [MessagePackObject]
    public sealed class BoneModifier
    {
        internal static readonly int CoordinateCount = Enum.GetValues(typeof(CoordinateType)).Length;

        private bool _hasBaseline;
        private float _lenBaseline;
        private Vector3 _sclBaseline = Vector3.one;
        private Vector3 _posBaseline = Vector3.zero;
        private Quaternion _rotBaseline;

        private bool _lenModForceUpdate;
        private bool _lenModNeedsPositionRestore;
        private Vector3 _positionBaseline;

        private bool _changedScale, _changedRotation, _changedPosition;

        private bool _forceApply;

        /// <summary>
        /// Create empty modifier that is not coordinate specific
        /// </summary>
        /// <param name="boneName">Name of the bone transform to affect</param>
        public BoneModifier(string boneName) : this(boneName, new[] { new BoneModifierData() }) { }

        /// <param name="boneName">Name of the bone transform to affect</param>
        /// <param name="coordinateModifiers">
        /// Needs to be either 1 long to apply to all coordinates or 7 to apply to specific
        /// coords
        /// </param>
        public BoneModifier(string boneName, BoneModifierData[] coordinateModifiers)
        {
            if (string.IsNullOrEmpty(boneName))
                throw new ArgumentException("Invalid boneName - " + boneName, nameof(boneName));
            if (coordinateModifiers == null)
                throw new ArgumentNullException(nameof(coordinateModifiers));
            if (coordinateModifiers.Length != 1 && coordinateModifiers.Length != CoordinateCount)
                throw new ArgumentException($"Need to set either 1 modifier or {CoordinateCount} modifiers, not {coordinateModifiers.Length}", nameof(coordinateModifiers));

            BoneName = boneName;
            CoordinateModifiers = coordinateModifiers.ToArray();
        }

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
        /// Apply the modifiers
        /// </summary>
        public void Apply(CoordinateType coordinate, IList<BoneModifierData> additionalModifiers, bool isDuringHScene)
        {
            if (BoneTransform == null) return;

            var modifier = GetModifier(coordinate);

            if (additionalModifiers != null && additionalModifiers.Count > 0)
                modifier = CombineModifiers(modifier, additionalModifiers);

            if (CanApply(modifier))
            {
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

                if (modifier.HasRotation() && !KKABMX_Core.NoRotationBones.Contains(BoneTransform.name))
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
                    if (HasLenBaseline())
                    {
                        var localPosition = BoneTransform.localPosition;
                        // Handle negative position modifiers, needed to prevent position sign changing on every frame
                        // (since negative modifier.LengthModifier would constantly flip it)
                        // Also needed for values near 0 to prevent losing the position data
                        if (modifier.LengthModifier < 0.1f || localPosition == Vector3.zero || isDuringHScene)
                        {
                            // Fall back to more aggresive mode
                            localPosition = _positionBaseline;
                            _lenModNeedsPositionRestore = true;
                        }

                        BoneTransform.localPosition = localPosition / localPosition.magnitude * _lenBaseline * modifier.LengthModifier;

                        _lenModForceUpdate = false;

                        if (modifier.HasPosition())
                        {
                            BoneTransform.localPosition = new Vector3(
                                BoneTransform.localPosition.x + modifier.PositionModifier.x,
                                BoneTransform.localPosition.y + modifier.PositionModifier.y,
                                BoneTransform.localPosition.z + modifier.PositionModifier.z
                            );
                            _changedPosition = true;
                        }
                        else if (_changedPosition)
                        {
                            BoneTransform.localPosition = _posBaseline;
                            _changedPosition = false;
                        }

                        return;
                    }
                }

                if (modifier.HasPosition())
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
        }

        private readonly BoneModifierData _combineModifiersCachedReturn = new BoneModifierData();
        private BoneModifierData CombineModifiers(BoneModifierData baseModifier, IList<BoneModifierData> additionalModifiers)
        {
            var scale = baseModifier.ScaleModifier;
            var len = baseModifier.LengthModifier;
            var position = baseModifier.PositionModifier;
            var rotation = baseModifier.RotationModifier;

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

        public void CollectBaseline()
        {
            if (BoneTransform == null) return;

            _sclBaseline = BoneTransform.localScale;
            _posBaseline = BoneTransform.localPosition;
            _rotBaseline = BoneTransform.localRotation;

            if (!HasLenBaseline())
            {
                _lenBaseline = BoneTransform.localPosition.magnitude;
                _positionBaseline = BoneTransform.localPosition;
                _lenModNeedsPositionRestore = false;
            }

            _hasBaseline = true;
        }

        public BoneModifierData GetModifier(CoordinateType coordinate)
        {
            if (CoordinateModifiers.Length == 1) return CoordinateModifiers[0];
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
            return CoordinateModifiers.Length == CoordinateCount;
#endif
        }

        /// <summary>
        /// If this modifier is not coordinate specific, make it coordinate specific (one set of values for each outfit)
        /// </summary>
        public void MakeCoordinateSpecific()
        {
            if (!IsCoordinateSpecific())
                CoordinateModifiers = Enumerable.Range(0, CoordinateCount).Select(_ => CoordinateModifiers[0].Clone()).ToArray();
        }

        /// <summary>
        /// If this modifier is coordinate specific, make it not coordinate specific (one set of values for all outfits)
        /// </summary>
        public void MakeNonCoordinateSpecific()
        {
            if (IsCoordinateSpecific())
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
                if (HasLenBaseline())
                {
                    var baseline = _lenBaseline;
                    // Flip position back to normal if necessary
                    if (_lenModNeedsPositionRestore || BoneTransform.localPosition == Vector3.zero)
                    {
                        BoneTransform.localPosition = _positionBaseline;
                        _lenModNeedsPositionRestore = false;
                    }
                    else
                    {
                        BoneTransform.localPosition = BoneTransform.localPosition / BoneTransform.localPosition.magnitude * baseline;
                    }
                }
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

        private bool HasLenBaseline()
        {
            return _positionBaseline != Vector3.zero;
        }
    }
}
