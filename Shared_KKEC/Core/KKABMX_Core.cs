using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using KKABMX.GUI;
using KKAPI.Chara;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "KKABMX", Version)]
    [BepInDependency(ExtendedSave.GUID)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = Metadata.Version;
        public const string GUID = Metadata.GUID;
        public const string ExtDataGUID = Metadata.ExtDataGUID;

        internal static ConfigEntry<bool> XyzMode { get; private set; }
        internal static ConfigEntry<bool> RaiseLimits { get; private set; }
        internal static ConfigEntry<bool> TransparentAdvancedWindow { get; private set; }

        internal static KKABMX_Core Instance { get; private set; }
        internal static new ManualLogSource Logger { get; private set; }

        private void Start()
        {
            Instance = this;
            Logger = base.Logger;

            if (File.Exists(Path.Combine(Paths.PluginPath, "KKABMPlugin.dll")) || File.Exists(Path.Combine(Paths.PluginPath, "KKABMGUI.dll")))
            {
                Logger.Log(LogLevel.Message | LogLevel.Error, "Old version of ABM found! Remove KKABMPlugin.dll and KKABMGUI.dll and restart the game.");
                return;
            }

            XyzMode = Config.Bind("Maker", Metadata.XyzModeName, false, Metadata.XyzModeDesc);
            RaiseLimits = Config.Bind("Maker", Metadata.RaiseLimitsName, false, Metadata.RaiseLimitsDesc);
            TransparentAdvancedWindow = Config.Bind("General", Metadata.AdvTransparencyName, false, Metadata.AdvTransparencyDesc);
            XyzMode.SettingChanged += KKABMX_GUI.OnIsAdvancedModeChanged;

            gameObject.AddComponent<KKABMX_GUI>();

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            Hooks.Init();
        }

        internal static void Log(LogLevel level, string text)
        {
            Logger.Log(level, text);
        }

        // Bones that misbehave with rotation adjustments
        internal static HashSet<string> NoRotationBones = new HashSet<string>
        {
            //todo
        };
    }
}
