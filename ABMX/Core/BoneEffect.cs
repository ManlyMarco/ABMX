using System.Collections.Generic;

#if KK
using CoordinateType = ChaFileDefine.CoordinateType;
#elif EC
using CoordinateType = KoikatsuCharaFile.ChaFileDefine.CoordinateType;
#endif

namespace KKABMX.Core
{
    public abstract class BoneEffect
    {
        public abstract IEnumerable<string> GetAffectedBones(BoneController origin);
        public abstract BoneModifierData GetEffect(string bone, BoneController origin, CoordinateType coordinate);
    }
}
