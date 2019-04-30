using Harmony;
using KKAPI.Maker;
using Studio;

namespace KKABMX.Core
{
    public partial class KKABMX_Core
    {
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
