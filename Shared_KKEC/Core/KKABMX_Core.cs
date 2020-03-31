using System.Collections.Generic;
using BepInEx;

namespace KKABMX.Core
{
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        // Bones that misbehave with rotation adjustments
        internal static HashSet<string> NoRotationBones = new HashSet<string>
        {
            //todo
        };

    }
}
