using System;
using System.Linq;
using BepInEx.Harmony;
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
                var i = HarmonyWrapper.PatchAll(typeof(Hooks));
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

                    var t = Traverse.Create(__instance);
                    foreach (var sliderUi in t.Field("face").Field<InputSliderUI[]>("sliders").Value)
                        sliderUi.AddOnChangeAction(OnUpdate);
                    foreach (var sliderUi in t.Field("body").Field<InputSliderUI[]>("sliders").Value)
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
