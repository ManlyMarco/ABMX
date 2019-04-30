using Harmony;
using KKAPI.Maker;

namespace KKABMX.Core
{
    public partial class KKABMX_Core
    {
        private static class Hooks
        {
            [HarmonyPostfix, HarmonyPatch(typeof(ShapeInfoBase), nameof(ShapeInfoBase.ChangeValue), typeof(int), typeof(float))]
            public static void ChangeValuePost(bool __result)
            {
                if (!__result) return;

                var controller = MakerAPI.GetCharacterControl()?.GetComponent<BoneController>();
                if (controller != null)
                    controller.NeedsBaselineUpdate = true;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(ShapeInfoBase), nameof(ShapeInfoBase.ChangeValue), typeof(int), typeof(int), typeof(int), typeof(float))]
            public static void ChangeValuePost2(bool __result)
            {
                if (!__result) return;

                var controller = MakerAPI.GetCharacterControl()?.GetComponent<BoneController>();
                if (controller != null)
                    controller.NeedsBaselineUpdate = true;
            }
        }
    }
}
