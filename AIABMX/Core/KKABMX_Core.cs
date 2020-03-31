using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKABMX.GUI;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "AIABMX (BonemodX)", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
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

            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
            {
                XyzMode = Config.Bind("Maker", Metadata.XyzModeName, false, Metadata.XyzModeDesc);
                RaiseLimits = Config.Bind("Maker", Metadata.RaiseLimitsName, false, Metadata.RaiseLimitsDesc);
                TransparentAdvancedWindow = Config.Bind("General", Metadata.AdvTransparencyName, false, Metadata.AdvTransparencyDesc);
                XyzMode.SettingChanged += KKABMX_GUI.OnIsAdvancedModeChanged;

                var showAdv = Config.Bind("Maker", "Show Advanced Bonemod Window", false);
                showAdv.SettingChanged += (sender, args) => gameObject.GetComponent<KKABMX_AdvancedGUI>().enabled = showAdv.Value;

                MakerAPI.MakerFinishedLoading += (sender, args) => showAdv.Value = false;

                gameObject.AddComponent<KKABMX_GUI>();
            }

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            Hooks.Init();
        }

        // Bones that misbehave with rotation adjustments
        internal static HashSet<string> NoRotationBones = new HashSet<string>
        {
            "cf_J_Hips",
            "cf_J_Head",
            "cf_J_Neck",
            "cf_J_Spine01",
            "cf_J_Spine02",
            "cf_J_Spine03",
            "cf_J_Kosi01",
            "cf_J_LegUp00_R",
            "cf_J_LegUp00_L",
            "cf_J_LegLow01_R",
            "cf_J_LegLow01_L",
            "cf_J_Foot01_R",
            "cf_J_Foot02_R",
            "cf_J_Foot01_L",
            "cf_J_Foot02_L",
            "cf_J_Toes01_R",
            "cf_J_Toes01_L",
            "cf_J_Shoulder_R",
            "cf_J_Shoulder_L",
            "cf_J_ArmUp00_R",
            "cf_J_ArmUp00_L",
            "cf_J_Hand_R",
            "cf_J_Hand_L",
            "cf_J_Hand_Thumb01_R",
            "cf_J_Hand_Thumb02_R",
            "cf_J_Hand_Thumb03_R",
            "cf_J_Hand_Thumb01_L",
            "cf_J_Hand_Thumb02_L",
            "cf_J_Hand_Thumb03_L",
            "cf_J_Index01_R",
            "cf_J_Index02_R",
            "cf_J_Index03_R",
            "cf_J_Index01_L",
            "cf_J_Index02_L",
            "cf_J_Index03_L",
            "cf_J_Middle01_R",
            "cf_J_Middle02_R",
            "cf_J_Middle03_R",
            "cf_J_Middle01_L",
            "cf_J_Middle02_L",
            "cf_J_Middle03_L",
            "cf_J_Ring01_R",
            "cf_J_Ring02_R",
            "cf_J_Ring03_R",
            "cf_J_Ring01_L",
            "cf_J_Ring02_L",
            "cf_J_Ring03_L",
            "cf_J_Little01_R",
            "cf_J_Little02_R",
            "cf_J_Little03_R",
            "cf_J_Little01_L",
            "cf_J_Little02_L",
            "cf_J_Little03_L",
            "cf_J_Thumb01_R",
            "cf_J_Thumb02_R",
            "cf_J_Thumb03_R",
            "cf_J_Thumb01_L",
            "cf_J_Thumb02_L",
            "cf_J_Thumb03_L",
            "cm_J_dan101_00",
            "cm_J_dan109_00",
            "cf_J_hair_FLa_01",
            "cf_J_hair_FLa_02",
            "cf_J_hair_FRa_01",
            "cf_J_hair_FRa_02",
            "cf_J_hair_BCa_01",
            "cf_J_sk_00_00",
            "cf_J_sk_00_01",
            "cf_J_sk_00_02",
            "cf_J_sk_00_03",
            "cf_J_sk_00_04",
            "cf_J_sk_00_05",
            "cf_J_sk_04_00",
            "cf_J_sk_04_01",
            "cf_J_sk_04_02",
            "cf_J_sk_04_03",
            "cf_J_sk_04_04",
            "cf_J_sk_04_05",
            "cf_J_Legsk_01_00",
            "cf_J_Legsk_01_01",
            "cf_J_Legsk_01_02",
            "cf_J_Legsk_01_03",
            "cf_J_Legsk_01_04",
            "cf_J_Legsk_01_05",
            "cf_J_Legsk_02_00",
            "cf_J_Legsk_02_01",
            "cf_J_Legsk_02_02",
            "cf_J_Legsk_02_03",
            "cf_J_Legsk_02_04",
            "cf_J_Legsk_02_05",
            "cf_J_Legsk_03_00",
            "cf_J_Legsk_03_01",
            "cf_J_Legsk_03_02",
            "cf_J_Legsk_03_03",
            "cf_J_Legsk_03_04",
            "cf_J_Legsk_03_05",
            "cf_J_Legsk_05_00",
            "cf_J_Legsk_05_01",
            "cf_J_Legsk_05_02",
            "cf_J_Legsk_05_03",
            "cf_J_Legsk_05_04",
            "cf_J_Legsk_05_05",
            "cf_J_Legsk_06_00",
            "cf_J_Legsk_06_01",
            "cf_J_Legsk_06_02",
            "cf_J_Legsk_06_03",
            "cf_J_Legsk_06_04",
            "cf_J_Legsk_06_05",
            "cf_J_Legsk_07_00",
            "cf_J_Legsk_07_01",
            "cf_J_Legsk_07_02",
            "cf_J_Legsk_07_03",
            "cf_J_Legsk_07_04",
            "cf_J_Legsk_07_05",
        };
    }
}
