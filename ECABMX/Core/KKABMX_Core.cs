using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using BepInEx.Logging;
using EC.Core.ExtensibleSaveFormat;
using KKABMX.GUI;
using KKAPI.Chara;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "KKABMX", Version)]
    [BepInDependency(ExtendedSave.GUID)]
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = Metadata.Version;
        public const string GUID = Metadata.GUID;
        public const string ExtDataGUID = Metadata.ExtDataGUID;

        public static ConfigWrapper<bool> XyzMode { get; private set; }
        public static ConfigWrapper<bool> RaiseLimits { get; private set; }

        internal static KKABMX_Core Instance { get; private set; }

        private void Start()
        {
            Instance = this;

            if (!KKAPI.KoikatuAPI.CheckRequiredPlugin(this, KKAPI.KoikatuAPI.GUID, new Version(KKAPI.KoikatuAPI.VersionConst)))
                return;

            XyzMode = Config.Wrap("GUI", Metadata.XyzModeName, Metadata.XyzModeDesc, false);
            RaiseLimits = Config.Wrap("GUI", Metadata.RaiseLimitsName, Metadata.RaiseLimitsDesc, false);
            XyzMode.SettingChanged += KKABMX_GUI.OnIsAdvancedModeChanged;

            gameObject.AddComponent<KKABMX_GUI>();

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            HarmonyWrapper.PatchAll(typeof(Hooks));
        }

        public static void Log(LogLevel level, string text)
        {
            Instance.Logger.Log(level, text);
        }
    }
}
