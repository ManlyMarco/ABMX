using System.ComponentModel;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKABMX.GUI;
using KKAPI;
using KKAPI.Chara;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "AIABMX (BonemodX)", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = Metadata.Version;
        public const string GUID = Metadata.GUID;
        public const string ExtDataGUID = Metadata.ExtDataGUID;

        [DisplayName(Metadata.XyzModeName)]
        [Description(Metadata.XyzModeDesc)]
        internal static ConfigWrapper<bool> XyzMode { get; private set; }

        [DisplayName(Metadata.RaiseLimitsName)]
        [Description(Metadata.RaiseLimitsDesc)]
        internal static ConfigWrapper<bool> RaiseLimits { get; private set; }

        internal static KKABMX_Core Instance { get; private set; }
        internal static new ManualLogSource Logger { get; private set; }

        private void Start()
        {
            Instance = this;
            Logger = base.Logger;

            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
            {
                XyzMode = Config.Wrap("Maker", Metadata.XyzModeName, Metadata.XyzModeDesc, false);
                RaiseLimits = Config.Wrap("Maker", Metadata.RaiseLimitsName, Metadata.RaiseLimitsDesc, false);
                XyzMode.SettingChanged += KKABMX_GUI.OnIsAdvancedModeChanged;

                var showAdv = Config.Wrap("Maker", "Show Advanced Bonemod Window", "", false);
                showAdv.SettingChanged += (sender, args) => gameObject.GetComponent<KKABMX_AdvancedGUI>().enabled = showAdv.Value;

                gameObject.AddComponent<KKABMX_GUI>();
            }

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            Hooks.Init();
        }
    }
}
