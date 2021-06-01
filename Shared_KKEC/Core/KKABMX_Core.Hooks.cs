using HarmonyLib;
#if EC || KKS
using System.Linq;
using ExtensibleSaveFormat;
#endif
#if KK || KKS
using Studio;
#endif

namespace KKABMX.Core
{
    public partial class KKABMX_Core
    {
        private static class Hooks
        {
            public static void Init()
            {
                Harmony.CreateAndPatchAll(typeof(Hooks), GUID);

#if KKS || EC
                ExtendedSave.CardBeingImported += importedData =>
                {
                    if (importedData.TryGetValue(GUID, out var pluginData) && pluginData != null)
                    {
                        var modifiers = BoneController.ReadModifiers(pluginData);

                        // Only keep 1st coord
                        foreach (var modifier in modifiers)
                        {
                            if (modifier.IsCoordinateSpecific())
                            {
                                // Trim the coordinate array to correct size
                                modifier.CoordinateModifiers = modifier.CoordinateModifiers.Take(BoneModifier.CoordinateCount).ToArray();
                                // Clear coord data from coords other than the 1st one
                                foreach (var boneModifierData in modifier.CoordinateModifiers.Skip(1))
                                    boneModifierData.Clear();
                            }
                        }

                        importedData[GUID] = BoneController.SaveModifiers(modifiers);
                    }
                };
#endif
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

#if KK || KKS
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
#endif
        }
    }
}
