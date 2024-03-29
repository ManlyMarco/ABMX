﻿using System.Collections.Generic;

#if KK || KKS
using CoordinateType = ChaFileDefine.CoordinateType;
#elif EC
using CoordinateType = KoikatsuCharaFile.ChaFileDefine.CoordinateType;
#endif

namespace KKABMX.Core
{
    /// <summary>
    /// Base for custom bone modifiers that allow other mods to use ABMX's features.
    /// </summary>
    public abstract class BoneEffect
    {
        /// <summary>
        /// Should return names of all bones that can be affected by this effect.
        /// It's recommended to store the bone list in a static array and simply return that array.
        /// This may be called on every frame for each character, should be fast.
        /// </summary>
        /// <param name="origin">Bone controller that this effect applies to</param>
        public abstract IEnumerable<string> GetAffectedBones(BoneController origin);

        /// <summary>
        /// Get effect for the specified bone. If no effect should be applied, null should be returned.
        /// The modifier will be multiplied with other modifiers used on this bone, so 1 is the no-change value.
        /// This is called on every frame for each character, should be fast.
        /// </summary>
        /// <param name="bone">Bone to get effect for</param>
        /// <param name="origin">Bone controller that this effect applies to</param>
        /// <param name="coordinate">Coordinate the effect should apply to</param>
        public abstract BoneModifierData GetEffect(string bone, BoneController origin, CoordinateType coordinate);
    }
}
