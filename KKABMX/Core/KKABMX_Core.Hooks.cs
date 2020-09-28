using System.Linq;
using BepInEx.Harmony;
using HarmonyLib;
using KKAPI.Maker;
using Studio;

namespace KKABMX.Core
{
    public partial class KKABMX_Core
    {
        private static class Hooks
        {
            public static void Init()
            {
                HarmonyWrapper.PatchAll(typeof(Hooks));
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetShapeBodyValue))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetShapeFaceValue))]
            public static void ChangeValuePost(ChaControl __instance)
            {
                if (__instance != null)
                {
                    var controller = __instance.GetComponent<BoneController>();
                    if (controller != null)
                        controller.NeedsBaselineUpdate = true;
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveKinematicMode))]
            public static void ActiveKinematicModePost(OCIChar __instance)
            {
                var controller = __instance.charInfo?.GetComponent<BoneController>();
                if (controller != null)
                    controller.NeedsFullRefresh = true;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.LoadAnime))]
            public static void LoadAnimePost(OCIChar __instance)
            {
                var controller = __instance.charInfo?.GetComponent<BoneController>();
                if (controller != null)
                    controller.NeedsFullRefresh = true;
            }
        }
    }
}
