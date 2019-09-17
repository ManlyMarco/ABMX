using System.ComponentModel;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using KKABMX.GUI;
using KKAPI.Chara;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "AIABMX (BonemodX)", Version)]
    [BepInDependency(ExtendedSave.GUID)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = Metadata.Version;
        public const string GUID = "ManlyMarco.AIABMX";
        public const string ExtDataGUID = "AIABMX";

        [DisplayName(Metadata.XyzModeName)]
        [Description(Metadata.XyzModeDesc)]
        public static ConfigWrapper<bool> XyzMode { get; private set; }

        [DisplayName(Metadata.RaiseLimitsName)]
        [Description(Metadata.RaiseLimitsDesc)]
        public static ConfigWrapper<bool> RaiseLimits { get; private set; }

        internal static KKABMX_Core Instance { get; private set; }
        internal static new ManualLogSource Logger { get; private set; }

        private void Start()
        {
            Instance = this;
            Logger = base.Logger;

            XyzMode = Config.Wrap("Maker", Metadata.XyzModeName, Metadata.XyzModeDesc, false);
            RaiseLimits = Config.Wrap("Maker", Metadata.RaiseLimitsName, Metadata.RaiseLimitsDesc, false);
            XyzMode.SettingChanged += KKABMX_GUI.OnIsAdvancedModeChanged;

            var showAdv = Config.Wrap("Maker", "Show Advanced Bonemod Window", "", false);
            showAdv.SettingChanged += (sender, args) => gameObject.GetComponent<KKABMX_AdvancedGUI>().enabled = showAdv.Value;

            gameObject.AddComponent<KKABMX_GUI>();

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            Hooks.Init();
        }
    }
}
