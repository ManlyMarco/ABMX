using System.Linq;
using BepInEx.Harmony;
using HarmonyLib;
using KKAPI.Maker;

namespace KKABMX.Core
{
    public partial class KKABMX_Core
    {
        private static class Hooks
        {
            public static void Init()
            {
                var i = HarmonyWrapper.PatchAll(typeof(Hooks));

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
        }
    }
}
