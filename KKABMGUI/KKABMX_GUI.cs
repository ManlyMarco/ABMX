using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using KKABMX.Core;
using MakerAPI;
using UniRx;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace KKABMX.GUI
{
    [BepInPlugin("KKABMX.GUI", "KKABMX GUI", "1.0")]
    [BepInDependency(MakerAPI.MakerAPI.GUID)]
    [BepInDependency(KKABMX_Core.GUID)]
    public class KKABMX_GUI : BaseUnityPlugin
    {
        private static readonly Color SettingColor = new Color(1f, 0.84f, 0.57f);

        private BoneController _boneController;
        private readonly List<Action> _updateActionList = new List<Action>();

        [DisplayName("Enable legacy bonemod GUI")]
        [Description("Shows the old bone list UI in a separate window. Restart the game to apply changes.")]
        public ConfigWrapper<bool> EnableLegacyGui { get; }

        public KKABMX_GUI()
        {
            EnableLegacyGui = new ConfigWrapper<bool>(nameof(EnableLegacyGui), this);
        }

        private void Start()
        {
            var makerApi = MakerAPI.MakerAPI.Instance;
            makerApi.RegisterCustomSubCategories += OnRegisterCustomSubCategories;
            makerApi.MakerBaseLoaded += OnEarlyMakerFinishedLoading;
            makerApi.MakerExiting += OnMakerExiting;

            if (EnableLegacyGui.Value)
                gameObject.AddComponent<KKABMX_LegacyGUI>();
        }

        private void RegisterCustomControls(RegisterCustomControlsEvent callback)
        {
            foreach (var categoryBones in InterfaceData.BoneControls.GroupBy(x => x.Category))
            {
                var first = true;
                var category = categoryBones.Key;

                foreach (var boneMeta in categoryBones)
                {
                    if (!boneMeta.IsSeparator && !_boneController.Modifiers.ContainsKey(boneMeta.BoneName))
                    {
                        // TODO handle differently? Add but hide?
                        Logger.Log(LogLevel.Warning, "[KKABMX_GUI] Bone does not exist on the character: " + boneMeta.BoneName);
                        continue;
                    }

                    first = !RegisterSingleControl(category, boneMeta, first, callback);
                }

                if (ReferenceEquals(category, InterfaceData.BodyHands))
                {
                    if (!first)
                        callback.AddControl(new MakerSeparator(category, this) { TextColor = SettingColor });
                    RegisterFingerControl(category, callback);
                }
            }

            var bonesInMetadata = InterfaceData.BoneControls.Select(x => x.BoneName).Distinct()
                .Concat(InterfaceData.BoneControls.Select(x => x.RightBoneName).Distinct());

            foreach (var unusedBone in _boneController.Modifiers.Keys.Except(bonesInMetadata))
            {
                Logger.Log(LogLevel.Debug, $"[KKABMX_GUI] No GUI data for bone {unusedBone} " +
                                           $"(isScaleBone={_boneController.Modifiers[unusedBone].ScaleBone}, " +
                                           $"isNotManual={_boneController.Modifiers[unusedBone].IsNotManual})");
            }
        }

        private void RegisterFingerControl(MakerCategory category, RegisterCustomControlsEvent callback)
        {
            var rbSide = callback.AddControl(new MakerRadioButtons(category, this, "Hand to edit", "Both", "Left", "Right") { TextColor = SettingColor });
            var rbFing = callback.AddControl(new MakerRadioButtons(category, this, "Finger to edit", "All", "1", "2", "3", "4", "5") { TextColor = SettingColor });
            var rbSegm = callback.AddControl(new MakerRadioButtons(category, this, "Segment to edit", "Base", "Center", "Tip") { TextColor = SettingColor });

            IEnumerable<string> GetBoneNames()
            {
                var fingers = rbFing.Value == 0 ? InterfaceData.FingerNamePrefixes : new[] { InterfaceData.FingerNamePrefixes[rbFing.Value - 1] };
                var segmented = fingers.Select(fName => $"{fName}0{rbSegm.Value + 1}").ToList();
                var sided = Enumerable.Empty<string>();
                if (rbSide.Value <= 1)
                    sided = segmented.Select(s => s + "_L");
                if (rbSide.Value == 0 || rbSide.Value == 2)
                    sided = sided.Concat(segmented.Select(s => s + "_R"));
                return sided;
            }

            var x = callback.AddControl(new MakerSlider(category, "Scale X", 0, 3, 1, this) { TextColor = SettingColor });
            var y = callback.AddControl(new MakerSlider(category, "Scale Y", 0, 3, 1, this) { TextColor = SettingColor });
            var z = callback.AddControl(new MakerSlider(category, "Scale Z", 0, 3, 1, this) { TextColor = SettingColor });

            void UpdateDisplay(int _)
            {
                SetSliders(_boneController.Modifiers[GetBoneNames().First()]);
            }

            var isUpdatingValue = false;

            void SetSliders(BoneModifierBody bone)
            {
                isUpdatingValue = true;
                if (x != null) x.Value = bone.SclMod.x;
                if (y != null) y.Value = bone.SclMod.y;
                if (z != null) z.Value = bone.SclMod.z;
                isUpdatingValue = false;
            }

            void PushValueToControls()
            {
                UpdateDisplay(0);
            }

            _updateActionList.Add(PushValueToControls);
            PushValueToControls();

            rbSide.ValueChanged.Subscribe(UpdateDisplay);
            rbFing.ValueChanged.Subscribe(UpdateDisplay);
            rbSegm.ValueChanged.Subscribe(UpdateDisplay);

            void PullValuesToBone(float _)
            {
                if (isUpdatingValue) return;

                foreach (var boneName in GetBoneNames())
                {
                    var bone = _boneController.Modifiers[boneName];
                    var newValue = new Vector3(x.Value, y.Value, z.Value);
                    bone.SclMod = newValue;
                }
            }
            var obs = Observer.Create<float>(PullValuesToBone);
            x?.ValueChanged.Subscribe(obs);
            y?.ValueChanged.Subscribe(obs);
            z?.ValueChanged.Subscribe(obs);
        }

        private bool RegisterSingleControl(MakerCategory category, BoneMeta boneMeta, bool isFirstElement, RegisterCustomControlsEvent callback)
        {
            if (boneMeta.IsSeparator)
            {
                callback.AddControl(new MakerSeparator(category, this) { TextColor = SettingColor });
                return false;
            }

            if (!isFirstElement)
                callback.AddControl(new MakerSeparator(category, this) { TextColor = SettingColor });

            MakerRadioButtons rb = null;
            if (!string.IsNullOrEmpty(boneMeta.RightBoneName))
            {
                rb = callback.AddControl(new MakerRadioButtons(category, this, "Side to edit", "Both", "Left", "Right") { TextColor = SettingColor });
            }

            var x = boneMeta.X ? callback.AddControl(new MakerSlider(category, boneMeta.XDisplayName, boneMeta.Min, boneMeta.Max, 1, this) { TextColor = SettingColor }) : null;
            var y = boneMeta.Y ? callback.AddControl(new MakerSlider(category, boneMeta.YDisplayName, boneMeta.Min, boneMeta.Max, 1, this) { TextColor = SettingColor }) : null;
            var z = boneMeta.Z ? callback.AddControl(new MakerSlider(category, boneMeta.ZDisplayName, boneMeta.Min, boneMeta.Max, 1, this) { TextColor = SettingColor }) : null;
            var l = boneMeta.L ? callback.AddControl(new MakerSlider(category, boneMeta.LDisplayName, boneMeta.LMin, boneMeta.LMax, 1, this) { TextColor = SettingColor }) : null;

            var isUpdatingValue = false;

            void SetSliders(BoneModifierBody bone)
            {
                isUpdatingValue = true;
                if (x != null) x.Value = bone.SclMod.x;
                if (y != null) y.Value = bone.SclMod.y;
                if (z != null) z.Value = bone.SclMod.z;
                if (l != null) l.Value = bone.LenMod;
                isUpdatingValue = false;
            }

            void PushValueToControls()
            {
                var bone = _boneController.Modifiers[boneMeta.BoneName];

                if (rb != null)
                {
                    var bone2 = _boneController.Modifiers[boneMeta.RightBoneName];
                    if (bone.SclMod != bone2.SclMod)
                    {
                        if (rb.Value == 0)
                        {
                            rb.Value = 1;
                        }
                        else if (rb.Value == 2)
                        {
                            bone = bone2;
                        }
                    }
                    else
                    {
                        rb.Value = 0;
                    }
                }

                SetSliders(bone);
            }
            _updateActionList.Add(PushValueToControls);
            PushValueToControls();

            rb?.ValueChanged.Subscribe(
                i =>
                {
                    if (i == 1)
                        SetSliders(_boneController.Modifiers[boneMeta.BoneName]);
                    else if (i == 2)
                        SetSliders(_boneController.Modifiers[boneMeta.RightBoneName]);
                });

            void PullValuesToBone(float _)
            {
                if (isUpdatingValue) return;

                var bone = _boneController.Modifiers[boneMeta.BoneName];
                var newValue = new Vector3(x?.Value ?? bone.SclMod.x, y?.Value ?? bone.SclMod.y, z?.Value ?? bone.SclMod.y);

                if (rb != null)
                {
                    if (rb.Value != 1)
                    {
                        var bone2 = _boneController.Modifiers[boneMeta.RightBoneName];
                        if (rb.Value == 0)
                        {
                            bone2.SclMod = newValue;
                            if (l != null) bone2.LenMod = l.Value;
                        }
                        else if (rb.Value == 2)
                        {
                            bone2.SclMod = newValue;
                            if (l != null) bone2.LenMod = l.Value;
                            return;
                        }
                    }
                }

                bone.SclMod = newValue;
                if (l != null) bone.LenMod = l.Value;
            }
            var obs = Observer.Create<float>(PullValuesToBone);
            x?.ValueChanged.Subscribe(obs);
            y?.ValueChanged.Subscribe(obs);
            z?.ValueChanged.Subscribe(obs);
            l?.ValueChanged.Subscribe(obs);

            return true;
        }

        private static void OnRegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            foreach (var subCategory in InterfaceData.BoneControls
                .Select(x => x.Category)
                .Distinct())
            {
                e.AddSubCategory(subCategory);
            }
        }

        private void OnEarlyMakerFinishedLoading(object sender, RegisterCustomControlsEvent e)
        {
            _boneController = FindObjectOfType<BoneController>();
            var modifiers = _boneController?.Modifiers?.Values.ToArray();
            if (modifiers == null || modifiers.Length <= 0)
            {
                Logger.Log(LogLevel.Error, "[KKABMX_GUI] Failed to find a BoneController or there are no bone modifiers");
                return;
            }

            BoneControllerMgr.Instance.MakerLimitedLoad += (s, args) =>
            {
                if (!ReferenceEquals(_boneController, args.Controller)) return;

                foreach (var action in _updateActionList)
                    action();
            };
            _boneController.CurrentCoordinateChanged += (o, args) =>
            {
                foreach (var action in _updateActionList)
                    action();
            };

            RegisterCustomControls(e);
        }

        private void OnMakerExiting(object sender, EventArgs e)
        {
            _updateActionList.Clear();
            _boneController = null;
        }
    }
}