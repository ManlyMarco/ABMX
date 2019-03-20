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

        internal static bool MakerBodyDataLoad { get; set; } = true;
        internal static bool MakerCardDataLoad { get; set; } = true;

        private void Start()
        {
            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            HarmonyInstance.Create(GUID).PatchAll(typeof(Hooks));
        }
    }
}
