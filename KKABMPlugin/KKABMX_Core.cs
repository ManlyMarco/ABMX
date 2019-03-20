using BepInEx;
using Harmony;
using KKAPI.Chara;
using KKAPI.Maker;
using Studio;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "KKABMX Core", Version)]
    [BepInDependency("com.bepis.bepinex.extendedsave")]
    public class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = "3.0";
        public const string GUID = "KKABMX.Core";
        public const string ExtDataGUID = "KKABMPlugin.ABMData";

        internal static bool MakerBodyDataLoad { get; set; } = true;
        internal static bool MakerCardDataLoad { get; set; } = true;

        private void Start()
        {
            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            HarmonyInstance.Create(GUID).PatchAll(typeof(Hooks));
        }

        private static class Hooks
        {
            [HarmonyPostfix, HarmonyPatch(typeof(ShapeInfoBase), nameof(ShapeInfoBase.ChangeValue))]
            public static void ChangeValuePost(bool __result)
            {
                if (!__result) return;

                var controller = MakerAPI.GetCharacterControl()?.GetComponent<BoneController>();
                if (controller != null)
                    controller.NeedsBaselineUpdate = true;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveKinematicMode))]
            public static void ActiveKinematicModePost(OCIChar __instance)
            {
                var controller = __instance.charInfo?.GetComponent<BoneController>();
                if (controller != null)
                    controller.NeedsFullRefresh = true;
            }
        }
    }
}
