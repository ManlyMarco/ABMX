﻿using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using KKABMX.GUI;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using UniRx;
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

        private void Start()
        {
            Instance = this;
            Logger = base.Logger;

            gameObject.AddComponent<KKABMX_AdvancedGUI>();

            XyzMode = Config.Bind("Maker", Metadata.XyzModeName, false, Metadata.XyzModeDesc);
            RaiseLimits = Config.Bind("Maker", Metadata.RaiseLimitsName, false, Metadata.RaiseLimitsDesc);
            TransparentAdvancedWindow = Config.Bind("General", Metadata.AdvTransparencyName, false, Metadata.AdvTransparencyDesc);
            _openEditorKey = Config.Bind("General", "Open bonemod editor", KeyboardShortcut.Empty, "Opens advanced bonemod window if there is a character that can be edited.");

#if EC
            if (KKAPI.Studio.StudioAPI.InsideStudio)
            {
                KKAPI.Studio.StudioAPI.GetOrCreateCurrentStateCategory(null)
                    .AddControl(new KKAPI.Studio.UI.CurrentStateCategorySwitch("Show Bonemod", c => false))
                    .Value.Subscribe(show =>
                    {
                        if (show) KKABMX_AdvancedGUI.Enable(GetCurrentVisibleGirl()?.GetComponent<BoneController>());
                        else KKABMX_AdvancedGUI.Disable();
                    });
            }
            else
#endif
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
                        KKABMX_AdvancedGUI.Enable(g.GetComponent<BoneController>());
                    else
                        Logger.LogMessage("No characters found to edit");
                }
            }
        }

        private Human GetCurrentVisibleGirl()
        {
            if (MakerAPI.InsideMaker)
                return MakerAPI.GetCharacterControl();

            var m = GameObject.Find("CharaPosition/MainFemale");
            var f = m?.GetComponentInChildren<Female>();
            if (f != null) return f;

            //if (KKAPI.Studio.StudioAPI.InsideStudio)
            //{
            //    var c = KKAPI.Studio.StudioAPI.GetSelectedControllers<BoneController>().FirstOrDefault();
            //    return c?.ChaControl;
            //}

            return GameObject.FindObjectOfType<Female>();
        }

        // Bones that misbehave with rotation adjustments
        internal static HashSet<string> NoRotationBones = new HashSet<string> { };
    }
}
