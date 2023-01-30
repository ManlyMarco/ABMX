using System;
using AIChara;
using HarmonyLib;
using KKAPI.Chara;

namespace KKABMX.Core
{
    public partial class KKABMX_Core
    {
        private static class Hooks
        {
            public static void Init()
            {
                var hi = Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
                UncensorSelectorSupport.InstallHooks(hi);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetShapeBodyValue))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.SetShapeFaceValue))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessoryParent))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomFaceWithoutCustomTexture))]
            public static void ChangeValuePost(ChaControl __instance)
            {
                if (__instance != null)
                {
                    var controller = __instance.GetComponent<BoneController>();
                    if (controller != null)
                        controller.NeedsBaselineUpdate = true;
                }
            }

            #region Cache invalidation

            // Handles hair changes, otherwise they cause issues. Also catches clothing changes which might also help.
            // bug setting wetRate also triggers this, so changing this setting in maker triggers this every update
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaInfo), nameof(ChaInfo.updateWet), MethodType.Setter)]
            private static void UpdateWetHook(ChaInfo __instance, bool value)
            {
                if(value) OnBodyChangedHook(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaInfo), nameof(ChaInfo.objBody), MethodType.Setter)]
            [HarmonyPatch(typeof(ChaInfo), nameof(ChaInfo.objHead), MethodType.Setter)]
            private static void OnBodyChangedHook(ChaInfo __instance)
            {
                var boneController = __instance.GetComponent<BoneController>();
                if (boneController != null && boneController.BoneSearcher != null)
                {
                    boneController.BoneSearcher.ClearCache(true);
                    boneController.NeedsBaselineUpdate = true;
                }
            }

            private static class UncensorSelectorSupport
            {
                public static void InstallHooks(Harmony hi)
                {
#if KK
                    const string typeName = "KK_Plugins.UncensorSelector, KK_UncensorSelector";
#elif EC
                    const string typeName = "KK_Plugins.UncensorSelector, EC_UncensorSelector";
#elif KKS
                    const string typeName = "KK_Plugins.UncensorSelector, KKS_UncensorSelector";
#elif HS2
                    const string typeName = "KK_Plugins.UncensorSelector, HS2_UncensorSelector";
#elif AI
                    const string typeName = "KK_Plugins.UncensorSelector, AI_UncensorSelector";
#endif
                    var mi = Type.GetType(typeName, false)?
                        .GetNestedType("UncensorSelectorController", AccessTools.all)?
                        .GetMethod("TransferBones", AccessTools.all);

                    if (mi == null)
                        Logger.LogWarning("Could not find UncensorSelectorController.TransferBones - Make sure your UncensorSelector is up to date!");
                    else
                        hi.Patch(mi, postfix: new HarmonyMethod(typeof(UncensorSelectorSupport), nameof(TransferBonesHook)));
                }

                private static void TransferBonesHook(CharaCustomFunctionController __instance)
                {
                    OnBodyChangedHook(__instance.ChaControl);
                }
            }

            #endregion
        }
    }
}
