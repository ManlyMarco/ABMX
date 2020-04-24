using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using KKABMX.GUI;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;
using UnityEngine;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency(ExtendedSave.GUID)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = Metadata.Version;
        public const string GUID = Metadata.GUID;
        public const string ExtDataGUID = Metadata.ExtDataGUID;
        private const string Name = Metadata.Name;

        internal static ConfigEntry<bool> XyzMode { get; private set; }
        internal static ConfigEntry<bool> RaiseLimits { get; private set; }
        internal static ConfigEntry<bool> TransparentAdvancedWindow { get; private set; }

        internal static KKABMX_Core Instance { get; private set; }
        internal static new ManualLogSource Logger { get; private set; }
        private ConfigEntry<KeyboardShortcut> _openEditorKey;

        private IEnumerator Start()
        {
            Instance = this;
            Logger = base.Logger;

            XyzMode = Config.Bind("Maker", Metadata.XyzModeName, false, Metadata.XyzModeDesc);
            RaiseLimits = Config.Bind("Maker", Metadata.RaiseLimitsName, false, Metadata.RaiseLimitsDesc);
            TransparentAdvancedWindow = Config.Bind("General", Metadata.AdvTransparencyName, false, Metadata.AdvTransparencyDesc);
            // Use key B to be an in-place replacement for bonemodharmony
            _openEditorKey = Config.Bind("General", "Open bonemod editor", new KeyboardShortcut(KeyCode.B), "Opens advanced bonemod window if there is a character that can be edited.");

            // Wait for BMM to get loaded
            yield return null;

            var bmmExists = Type.GetType("BoneModHarmony.BMMHuman, BoneModHarmony") != null;
            if (bmmExists)
            {
                Logger.Log(LogLevel.Error | LogLevel.Message, "PHABMX can't load because it is incompatible with BoneModHarmony. Remove BoneModHarmony and try again.");
                enabled = false;
                yield break;
            }

            gameObject.AddComponent<KKABMX_AdvancedGUI>();

            if (StudioAPI.InsideStudio)
            {
                //todo
                //KKAPI.Studio.StudioAPI.GetOrCreateCurrentStateCategory(null)
                //    .AddControl(new KKAPI.Studio.UI.CurrentStateCategorySwitch("Show Bonemod", c => false))
                //    .Value.Subscribe(show =>
                //    {
                //        if (show) KKABMX_AdvancedGUI.Enable(GetCurrentVisibleGirl()?.GetComponent<BoneController>());
                //        else KKABMX_AdvancedGUI.Disable();
                //    });
            }
            else
            {
                gameObject.AddComponent<KKABMX_GUI>();
                XyzMode.SettingChanged += KKABMX_GUI.OnIsAdvancedModeChanged;
            }

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            Hooks.Init();
        }

        private void Update()
        {
            if (_openEditorKey.Value.IsDown())
            {
                if (KKABMX_AdvancedGUI.Enabled)
                {
                    KKABMX_AdvancedGUI.Disable();
                }
                else
                {
                    var g = GetCurrentVisibleGirl();
                    if (g != null)
                    {
                        KKABMX_AdvancedGUI.Enable(g.GetComponent<BoneController>());
                    }
                    else
                    {
                        if (StudioAPI.InsideStudio)
                            Logger.LogMessage("No characters selected. Select a character to edit its bones.");
                        else
                            Logger.LogMessage("No characters found to edit");
                    }
                }
            }
        }

        private static Human GetCurrentVisibleGirl()
        {
            if (MakerAPI.InsideMaker)
                return MakerAPI.GetCharacterControl();

            var m = GameObject.Find("CharaPosition/MainFemale");
            var f = m?.GetComponentInChildren<Female>();
            if (f != null) return f;

            if (StudioAPI.InsideStudio)
            {
                var c = StudioAPI.GetSelectedControllers<BoneController>().FirstOrDefault();
                return c?.ChaControl;
            }

            return (Human)FindObjectOfType<Female>() ?? FindObjectOfType<Male>();
        }

        // Bones that misbehave with rotation adjustments
        internal static HashSet<string> NoRotationBones = new HashSet<string> { };
    }
}
