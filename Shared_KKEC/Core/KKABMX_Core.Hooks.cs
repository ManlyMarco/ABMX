using HarmonyLib;
#if EC || KKS
using System.Linq;
using ExtensibleSaveFormat;
#endif
#if KK || KKS
using Studio;
#endif
#if KK
using ActionGame.Chara;
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

#if EC
                ExtendedSave.CardBeingImported += importedData =>
                {
                    if (importedData.TryGetValue(GUID, out var pluginData) && pluginData != null)
                    {
                        var modifiers = BoneController.ReadModifiers(pluginData);

                        // Only 1st coord is used in EC so remove others
                        foreach (var modifier in modifiers) modifier.MakeNonCoordinateSpecific();

                        importedData[GUID] = BoneController.SaveModifiers(modifiers);
                    }
                };
#elif KKS
                ExtendedSave.CardBeingImported += (data, mapping) =>
                {
                    if (data.TryGetValue(ExtDataGUID, out var pluginData) && pluginData != null)
                    {
                        var modifiers = BoneController.ReadModifiers(pluginData);
                        var coordCount = (int)mapping.Values.Max(x => x) + 1;
                        foreach (var modifier in modifiers)
                        {
                            if (!modifier.IsCoordinateSpecific()) continue;

                            var newArr = new BoneModifierData[coordCount];

                            foreach (var map in mapping)
                            {
                                // Discard unused
                                if (map.Value == null) continue;

                                if (map.Key < modifier.CoordinateModifiers.Length)
                                    newArr[(int)map.Value] = modifier.CoordinateModifiers[map.Key];
                                else
                                    newArr[(int)map.Value] = new BoneModifierData();
                            }

                            modifier.CoordinateModifiers = newArr;
                        }

                        data[ExtDataGUID] = BoneController.SaveModifiers(modifiers);
                    }
                };
#endif
            }

#if KK
            [HarmonyPostfix, HarmonyPatch(typeof(NPC), nameof(NPC.Pause))]
            public static void PausePost(NPC __instance)
            {
                var chaControl = __instance.chaCtrl;
                if (chaControl == null)
                    return;

                var boneController = chaControl.GetComponent<BoneController>();
                if (boneController == null)
                    return;

                boneController.enabled = !__instance.isPause;
            }
#endif

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
