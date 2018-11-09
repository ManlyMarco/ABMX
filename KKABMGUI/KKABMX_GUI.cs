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
        private BoneController _boneController;
        private readonly List<Action> _updateActionList = new List<Action>();

        public KKABMX_GUI()
        {
            EnableLegacyGui = new ConfigWrapper<bool>(nameof(EnableLegacyGui), this);
        }

        [DisplayName("Enable legacy bonemod GUI")]
        [Description("Shows the old bone list UI in a separate window. Restart the game to apply changes.")]
        public ConfigWrapper<bool> EnableLegacyGui { get; }

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
            foreach (var categoryBones in InterfaceData.Metadata.GroupBy(x => new MakerCategory(x.Category, x.SubCategory)))
            {
                var first = true;
                var category = categoryBones.Key;

                foreach (var boneMeta in categoryBones)
                {
                    if (!boneMeta.IsSeparator && !_boneController.modifiers.ContainsKey(boneMeta.BoneName))
                    {
                        // TODO handle differently? Add but hide?
                        Logger.Log(LogLevel.Warning, "Bone does not exist on the character: " + boneMeta.BoneName);
                        continue;
                    }

                    first = !RegisterSingleControl(category, boneMeta, first, callback);
                }
            }

            var bonesInMetadata = InterfaceData.Metadata.Select(x => x.BoneName).Distinct()
                .Concat(InterfaceData.Metadata.Select(x => x.RightBoneName).Distinct());

            foreach (var unusedBone in _boneController.modifiers.Keys.Except(bonesInMetadata))
            {
                Logger.Log(LogLevel.Debug, $"[KKABMX_GUI] No GUI data for bone {unusedBone} " +
                                           $"(isScaleBone={_boneController.modifiers[unusedBone].isScaleBone}, " +
                                           $"isNotManual={_boneController.modifiers[unusedBone].isNotManual})");
            }
        }

        private bool RegisterSingleControl(MakerCategory category, BoneMeta boneMeta, bool isFirstElement, RegisterCustomControlsEvent callback)
        {
            if (boneMeta.IsSeparator)
            {
                callback.AddControl(new MakerSeparator(category));
                return false;
            }

            if (!isFirstElement)
                callback.AddControl(new MakerSeparator(category));

            MakerRadioButtons rb = null;
            if (!string.IsNullOrEmpty(boneMeta.RightBoneName))
            {
                rb = callback.AddControl(new MakerRadioButtons(category, "Side to edit", "Both", "Left", "Right"));
            }

            var x = callback.AddControl(new MakerSlider(category, boneMeta.DisplayName + " X", boneMeta.Min, boneMeta.Max, 1));
            var y = callback.AddControl(new MakerSlider(category, boneMeta.DisplayName + " Y", boneMeta.Min, boneMeta.Max, 1));
            var z = callback.AddControl(new MakerSlider(category, boneMeta.DisplayName + " Z", boneMeta.Min, boneMeta.Max, 1));

            void PushValueToControls()
            {
                var bone = _boneController.modifiers[boneMeta.BoneName];

                if (rb != null)
                {
                    var bone2 = _boneController.modifiers[boneMeta.RightBoneName];
                    if (bone.sclMod != bone2.sclMod)
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

                x.Value = bone.sclMod.x;
                y.Value = bone.sclMod.y;
                z.Value = bone.sclMod.z;
            }
            _updateActionList.Add(PushValueToControls);
            PushValueToControls();

            void PullValuesToBone(float _)
            {
                var newValue = new Vector3(x.Value, y.Value, z.Value);
                var bone = _boneController.modifiers[boneMeta.BoneName];

                if (rb != null)
                {
                    if (rb.Value != 1)
                    {
                        var bone2 = _boneController.modifiers[boneMeta.RightBoneName];
                        if (rb.Value == 0)
                        {
                            bone2.sclMod = newValue;
                        }
                        else if (rb.Value == 2)
                        {
                            bone2.sclMod = newValue;
                            return;
                        }
                    }
                }

                bone.sclMod = newValue;
            }
            var obs = Observer.Create<float>(PullValuesToBone);
            x.ValueChanged.Subscribe(obs);
            y.ValueChanged.Subscribe(obs);
            z.ValueChanged.Subscribe(obs);

            return true;
        }

        private static void OnRegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            foreach (var subCategory in InterfaceData.Metadata
                .Select(x => new MakerCategory(x.Category, x.SubCategory))
                .Distinct())
            {
                e.AddSubCategory(subCategory);
            }
        }

        private void OnEarlyMakerFinishedLoading(object sender, RegisterCustomControlsEvent e)
        {
            _boneController = FindObjectOfType<BoneController>();
            var modifiers = _boneController?.modifiers?.Values.ToArray();
            if (modifiers == null || modifiers.Length <= 0)
            {
                Logger.Log(LogLevel.Error, "Failed to find a BoneController or there are no bone modifiers");
                return;
            }

            BoneControllerMgr.Instance.MakerLimitedLoad += (s, args) =>
            {
                if (!ReferenceEquals(_boneController, args.Controller)) return;

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