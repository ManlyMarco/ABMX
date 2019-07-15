using System;
using System.ComponentModel;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using KKABMX.GUI;
using KKAPI.Chara;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "KKABMX", Version)]
    [BepInDependency("com.bepis.bepinex.extendedsave")]
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = Metadata.Version;
        public const string GUID = Metadata.GUID;
        public const string ExtDataGUID = Metadata.ExtDataGUID;

        [DisplayName(Metadata.XyzModeName)]
        [Description(Metadata.XyzModeDesc)]
        public static ConfigWrapper<bool> XyzMode { get; private set; }

        [DisplayName(Metadata.RaiseLimitsName)]
        [Description(Metadata.RaiseLimitsDesc)]
        public static ConfigWrapper<bool> RaiseLimits { get; private set; }

        internal static KKABMX_Core Instance { get; private set; }

        private void Start()
        {
            Instance = this;

            if (!KKAPI.KoikatuAPI.CheckRequiredPlugin(this, KKAPI.KoikatuAPI.GUID, new Version(KKAPI.KoikatuAPI.VersionConst)))
                return;

            if (File.Exists(Path.Combine(Paths.PluginPath, "KKABMPlugin.dll")) || File.Exists(Path.Combine(Paths.PluginPath, "KKABMGUI.dll")))
            {
                Log(LogLevel.Message | LogLevel.Error, "Old version of ABM found! Remove KKABMPlugin.dll and KKABMGUI.dll and restart the game.");
                return;
            }

            XyzMode = new ConfigWrapper<bool>("XYZ-Scale-Mode", this, false);
            RaiseLimits = new ConfigWrapper<bool>("RaiseLimits", this, false);
            XyzMode.SettingChanged += KKABMX_GUI.OnIsAdvancedModeChanged;

            gameObject.AddComponent<KKABMX_GUI>();

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            Hooks.Init();
        }

        public static void Log(LogLevel logLevel, string text)
        {
            Logger.Log(logLevel, text);
        }
    }
}
