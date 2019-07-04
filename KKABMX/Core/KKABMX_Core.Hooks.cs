using System.Linq;
using Harmony;
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
                var i = HarmonyInstance.Create(GUID);
                i.PatchAll(typeof(Hooks));

                foreach (var target in AccessTools.GetDeclaredMethods(typeof(ShapeInfoBase)).Where(x => x.Name == nameof(ShapeInfoBase.ChangeValue)))
                    i.Patch(target, null, new HarmonyMethod(typeof(Hooks), nameof(ChangeValuePost)));
            }

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

            [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.LoadAnime))]
            public static void LoadAnimePost(OCIChar __instance)
            {
                var controller = __instance.charInfo?.GetComponent<BoneController>();
                if (controller != null)
                    controller.NeedsFullRefresh = true;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), "LateUpdate")]
            public static void HScenePostUpdateHook(HSceneProc __instance)
            {
                __instance.flags.player?.transform?.GetComponent<BoneController>()?.DoUpdate();
                foreach (var heroine in __instance.flags.lstHeroine)
                    heroine?.chaCtrl?.GetComponent<BoneController>()?.DoUpdate();
            }
        }
    }
}
