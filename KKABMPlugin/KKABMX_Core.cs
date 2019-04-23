using System;
using BepInEx;
using Harmony;
using KKAPI.Chara;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "KKABMX Core", Version)]
    [BepInDependency("com.bepis.bepinex.extendedsave")]
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = "3.0.1";
        public const string GUID = "KKABMX.Core";
        public const string ExtDataGUID = "KKABMPlugin.ABMData";

        private void Start()
        {
            if(!KKAPI.KoikatuAPI.CheckRequiredPlugin(this, KKAPI.KoikatuAPI.GUID, new Version(KKAPI.KoikatuAPI.VersionConst)))
                return;

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            HarmonyInstance.Create(GUID).PatchAll(typeof(Hooks));
        }
    }
}
