using System;
using HarmonyLib;
using KKAPI.Maker;
using Studio;
using UnityEngine;

namespace KKABMX.Core
{
    public partial class KKABMX_Core
    {
        private static class Hooks
        {
            public static void Init()
            {
                Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(EditMode), nameof(EditMode.Setup))]
            public static void EditModeSetup(EditMode __instance)
            {
                try
                {
                    var cha = MakerAPI.GetCharacterControl() ?? throw new ArgumentNullException($"MakerAPI.GetCharacterControl()");
                    var co = cha.GetComponent<BoneController>() ?? throw new ArgumentNullException($"cha.GetComponent<BoneController>()");

                    void OnUpdate(float x) => co.NeedsBaselineUpdate = true;
                    
                    foreach (var sliderUi in __instance.face.sliders)
                        sliderUi.AddOnChangeAction(OnUpdate);
                    foreach (var sliderUi in __instance.body.sliders)
                        sliderUi.AddOnChangeAction(OnUpdate);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.ActiveKinematicMode))]
            public static void ActiveKinematicModePost(OCIChar __instance)
            {
                var controller = __instance.charInfo?.human?.GetComponent<BoneController>();
                if (controller != null)
                    controller.NeedsFullRefresh = true;
            }
            
            [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), nameof(OCIChar.LoadAnime))]
            public static void LoadAnimePost(OCIChar __instance)
            {
                var controller = __instance.charInfo?.human?.GetComponent<BoneController>();
                if (controller != null)
                    controller.NeedsFullRefresh = true;
            }
        }
    }
}
