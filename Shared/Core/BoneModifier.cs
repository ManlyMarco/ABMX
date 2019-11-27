using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;

#if KK
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

        private bool _lenModForceUpdate;
        private bool _lenModNeedsPositionRestore;
        private Vector3 _positionBaseline;

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

        [Key(0)]
        public string BoneName { get; }

        [IgnoreMember]
        public Transform BoneTransform { get; internal set; }

        [Key(1)]
        // Needs a public set to make serializing work
        public BoneModifierData[] CoordinateModifiers { get; set; }

        public void Apply(CoordinateType coordinate, ICollection<BoneModifierData> additionalModifiers, bool isDuringHScene)
        {
            if (BoneTransform == null) return;

            var modifier = GetModifier(coordinate);

            if (additionalModifiers != null && additionalModifiers.Count > 0)
                modifier = CombineModifiers(modifier, additionalModifiers);

            if (CanApply(modifier))
            {
                BoneTransform.localScale = new Vector3(
                    _sclBaseline.x * modifier.ScaleModifier.x,
                    _sclBaseline.y * modifier.ScaleModifier.y,
                    _sclBaseline.z * modifier.ScaleModifier.z);

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
                    }
                }
            }
        }

        private static BoneModifierData CombineModifiers(BoneModifierData baseModifier, IEnumerable<BoneModifierData> additionalModifiers)
        {
            var scale = baseModifier.ScaleModifier;
            var len = baseModifier.LengthModifier;

            foreach (var additionalModifier in additionalModifiers)
            {
                scale = new Vector3(
                    scale.x * additionalModifier.ScaleModifier.x,
                    scale.y * additionalModifier.ScaleModifier.y,
                    scale.z * additionalModifier.ScaleModifier.z);
                len *= additionalModifier.LengthModifier;
            }

            return new BoneModifierData(scale, len);
        }

        public void CollectBaseline()
        {
            if (BoneTransform == null) return;

            _sclBaseline = BoneTransform.localScale;
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
#if AI
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
