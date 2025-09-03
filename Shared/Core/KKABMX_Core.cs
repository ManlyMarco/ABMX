using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using KKABMX.GUI;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using UniRx;
#if AI || HS2
using AIChara;
#endif

namespace KKABMX.Core
{
    /// <summary>
    /// Entry point
    /// </summary>
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency(ExtendedSave.GUID)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        /// <summary> Version of this plugin </summary>
        public const string Version = Metadata.Version;
        /// <summary> GUID of this plugin </summary>
        public const string GUID = Metadata.GUID;
        /// <summary> GUID used for ext data of this plugin </summary>
        public const string ExtDataGUID = Metadata.ExtDataGUID;
        private const string Name = Metadata.Name;

        internal static ConfigEntry<bool> ShowSliders { get; private set; }
        internal static ConfigEntry<bool> XyzMode { get; private set; }
        internal static ConfigEntry<bool> RaiseLimits { get; private set; }
        internal static ConfigEntry<bool> ResetToLastLoaded { get; private set; }
        internal static ConfigEntry<string> Favorites;

        internal static KKABMX_Core Instance { get; private set; }
        internal static new ManualLogSource Logger { get; private set; }
        private ConfigEntry<KeyboardShortcut> _openEditorKey;

        private void Start()
        {
            Instance = this;
            Logger = base.Logger;

            gameObject.AddComponent<KKABMX_AdvancedGUI>();

            ShowSliders = Config.Bind("Maker", Metadata.ShowSlidersName, true, Metadata.ShowSlidersDesc);
            XyzMode = Config.Bind("Maker", Metadata.XyzModeName, false, Metadata.XyzModeDesc);
            RaiseLimits = Config.Bind("Maker", Metadata.RaiseLimitsName, false, Metadata.RaiseLimitsDesc);
            ResetToLastLoaded = Config.Bind("Maker", Metadata.ResetToLastLoadedName, true, Metadata.ResetToLastLoadedDesc);
            _openEditorKey = Config.Bind("General", Metadata.OpenEditorKeyName, KeyboardShortcut.Empty, Metadata.OpenEditorKeyDesc);
            Favorites = Config.Bind("Advanced", "Favorites", string.Empty, new ConfigDescription("Favorites in advanced window separated by /", null, "Advanced"));

#if !EC
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
            
            AccessoriesApi.AccessoryTransferred += OnAccCopy;
#if KK || KKS
            AccessoriesApi.AccessoriesCopied += OnAccCoordCopy;
#endif
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
                    if (g == null)
                        g = FindObjectsOfType<ChaControl>().OrderByDescending(x => x.isActiveAndEnabled).ThenBy(x => x.name).FirstOrDefault();

                    if (g != null)
                        KKABMX_AdvancedGUI.Enable(g.GetComponent<BoneController>());
                    else
                        Logger.LogMessage("No characters found to edit");
                }
            }
        }

        private ChaControl GetCurrentVisibleGirl()
        {
            if (MakerAPI.InsideMaker)
                return MakerAPI.GetCharacterControl();
#if !EC
            if (KKAPI.Studio.StudioAPI.InsideStudio)
            {
                var c = KKAPI.Studio.StudioAPI.GetSelectedControllers<BoneController>().FirstOrDefault();
                return c?.ChaControl;
            }
#endif
#if !EC && !AI && !HS2
            return GameAPI.GetCurrentHeroine()?.chaCtrl;
#else
      return null;
#endif
        }

        private static void OnAccCopy(object sender, AccessoryTransferEventArgs e)
        {
            var chara = MakerAPI.GetCharacterControl();
            if (chara == null) return;
            var ctrl = chara.GetComponent<BoneController>();
            if (ctrl == null) throw new ArgumentNullException(nameof(ctrl));

            var sourceKey = BoneLocation.Accessory + e.SourceSlotIndex;
            var targetKey = BoneLocation.Accessory + e.DestinationSlotIndex;

            ctrl.ModifierDict.TryGetValue(targetKey, out var existing);
            if (existing != null)
            {
                foreach (var mod in existing)
                {
                    mod.Reset();
                    mod.ClearBaseline();
                }
            }

            var sourceModifiers = ctrl.GetAllModifiers(sourceKey);
            ctrl.ModifierDict[targetKey] = sourceModifiers.Select(m =>
            {
                var mcopy = m.Clone();
                mcopy.BoneLocation = targetKey;
                return mcopy;
            }).ToList();

            // Need to refresh for both source and target accs. The source acccessory is destroyed and recreated.
            ctrl.NeedsFullRefresh = true;
        }
#if KK || KKS
        private static void OnAccCoordCopy(object sender, AccessoryCopyEventArgs e)
        {
            var chara = MakerAPI.GetCharacterControl();
            if (chara == null) return;
            var ctrl = chara.GetComponent<BoneController>();
            if (ctrl == null) throw new ArgumentNullException(nameof(ctrl));

            var any = false;
            foreach (var copiedSlotIndex in e.CopiedSlotIndexes)
            {
                var copiedAccKey = BoneLocation.Accessory + copiedSlotIndex;
                foreach (var boneModifier in ctrl.GetAllModifiers(copiedAccKey))
                {
                    if (boneModifier.IsCoordinateSpecific())
                    {
                        // Ensure all coords exist just in case one was just added
                        boneModifier.MakeCoordinateSpecific(chara.chaFile.coordinate.Length);

                        var src = boneModifier.GetModifier(e.CopySource);
                        var dest = boneModifier.GetModifier(e.CopyDestination);
                        src.CopyTo(dest);
                        any = true;
                    }
                }
            }

            // The source acccessory is destroyed and recreated, so need to refresh bone transforms.
            if (any)
                ctrl.NeedsFullRefresh = true;
        }
#endif
    }
}
