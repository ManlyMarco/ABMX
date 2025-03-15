using System;
using HarmonyLib;
using KKAPI.Chara;
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
                var hi = Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
                UncensorSelectorSupport.InstallHooks(hi);

#if EC
                ExtendedSave.CardBeingImported += importedData =>
                {
                    if (importedData.TryGetValue(GUID, out var pluginData) && pluginData != null)
                    {
                        var modifiers = BoneController.ReadModifiers(pluginData);

                        // Only 1st coord is used in EC so remove others
                        foreach (var modifier in modifiers) modifier.MakeNonCoordinateSpecific(0);

                        importedData[GUID] = BoneController.SaveModifiers(modifiers, false);
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

                            for (var i = 0; i < newArr.Length; i++)
                            {
                                if (newArr[i] == null)
                                    newArr[i] = new BoneModifierData();
                            }

                            modifier.CoordinateModifiers = newArr;
                        }

                        data[ExtDataGUID] = BoneController.SaveModifiers(modifiers, false);
                    }
                };
#endif
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

            #region Cache invalidation

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
