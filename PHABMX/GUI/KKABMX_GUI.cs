﻿using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using KKABMX.Core;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using UniRx;
using UnityEngine;

namespace KKABMX.GUI
{
    public class KKABMX_GUI : MonoBehaviour
    {
        private const int LimitRaiseAmount = 2;
        private static readonly Color _settingColor = new Color(1f, 0.84f, 0.57f);

        private BoneController _boneController;
        private readonly List<Action> _updateActionList = new List<Action>();

        private static MakerLoadToggle _faceLoadToggle;
        private static MakerLoadToggle _bodyLoadToggle;
        internal static bool LoadFace => _faceLoadToggle == null || _faceLoadToggle.Value;
        internal static bool LoadBody => _bodyLoadToggle == null || _bodyLoadToggle.Value;

        internal static void OnIsAdvancedModeChanged(object sender, EventArgs args) => IsAdvancedModeChanged?.Invoke(sender, args);
        private static event EventHandler IsAdvancedModeChanged;

        public static bool XyzMode
        {
            get => KKABMX_Core.XyzMode.Value;
            set => KKABMX_Core.XyzMode.Value = value;
        }

        public static bool RaiseLimits
        {
            get => KKABMX_Core.RaiseLimits.Value;
            set => KKABMX_Core.RaiseLimits.Value = value;
        }


        private void Start()
        {
            MakerAPI.RegisterCustomSubCategories += OnRegisterCustomSubCategories;
            MakerAPI.MakerBaseLoaded += OnEarlyMakerFinishedLoading;
            MakerAPI.MakerExiting += OnMakerExiting;
        }

        private void RegisterCustomControls(RegisterCustomControlsEvent callback)
        {
            foreach (var categoryBones in InterfaceData.BoneControls.GroupBy(x => x.Category))
            {
                var category = categoryBones.Key;

                var first = true;
                foreach (var boneMeta in categoryBones)
                {
                    //if (boneMeta.IsSeparator || !first)
                    //    callback.AddControl(new MakerSeparator(category, KKABMX_Core.Instance) { TextColor = _settingColor });

                    RegisterSingleControl(category, boneMeta, callback);
                    first = false;
                }

                // todo separate category?
                if (Equals(category, InterfaceData.FingerCategory))
                {
                    //if (!first)
                    //    callback.AddControl(new MakerSeparator(category, KKABMX_Core.Instance) { TextColor = _settingColor });

                    RegisterFingerControl(category, callback);
                }
            }

            _faceLoadToggle = callback.AddLoadToggle(new MakerLoadToggle("Face Bonemod"));
            _bodyLoadToggle = callback.AddLoadToggle(new MakerLoadToggle("Body Bonemod"));

            //callback.AddCoordinateLoadToggle(new MakerCoordinateLoadToggle("Bonemod"))
            //    .ValueChanged.Subscribe(b => GetRegistration().MaintainCoordinateState = !b);

            // todo not enough space, need custom cats or scrolling
            //callback.AddSidebarControl(new SidebarToggle("Split XYZ", XyzMode, KKABMX_Core.Instance))
            //    .ValueChanged.Subscribe(b => XyzMode = b);

            callback.AddSidebarControl(new SidebarToggle("Bonemod", false, KKABMX_Core.Instance))
                .ValueChanged.Subscribe(b =>
                {
                    if (b) KKABMX_AdvancedGUI.Enable(MakerAPI.GetCharacterControl().GetComponent<BoneController>());
                    else KKABMX_AdvancedGUI.Disable();
                });
        }

        private static CharacterApi.ControllerRegistration GetRegistration()
        {
            return CharacterApi.GetRegisteredBehaviour(typeof(BoneController));
        }

        private void RegisterFingerControl(MakerCategory category, RegisterCustomControlsEvent callback)
        {
            var rbSide = callback.AddControl(new MakerDropdown("Hand to edit", new[] { "Both", "Left", "Right" }, category, 0, KKABMX_Core.Instance) { TextColor = _settingColor });
            rbSide.Visible.OnNext(false);
            var rbFing = callback.AddControl(new MakerDropdown("Finger to edit", new[] { "All", "1", "2", "3", "4", "5" }, category, 0, KKABMX_Core.Instance) { TextColor = _settingColor });
            var rbSegm = callback.AddControl(new MakerDropdown("Finger segment", new[] { "Base", "Center", "Tip" }, category, 0, KKABMX_Core.Instance) { TextColor = _settingColor });

            IEnumerable<string> GetFingerBoneNames()
            {
                var fingers = rbFing.Value == 0 ? InterfaceData.FingerNamePrefixes : new[] { InterfaceData.FingerNamePrefixes[rbFing.Value - 1] };
                var segmented = fingers.Select(fName => $"{fName}0{rbSegm.Value + 1}").ToList();
                var sided = Enumerable.Empty<string>();
                if (rbSide.Value <= 1)
                    sided = segmented.Select(s => s + "_L");
                if (rbSide.Value == 0 || rbSide.Value == 2)
                    sided = sided.Concat(segmented.Select(s => s + "_R"));
                if (MakerAPI.GetMakerSex() == SEX.MALE) sided = sided.Select(x => x.Replace("cf_", "cm_"));
                return sided;
            }

            var isAdvanced = true;
            var maxFingerValue = RaiseLimits ? 3 * LimitRaiseAmount : 3;
            //var x = callback.AddControl(new MakerSlider(category, "Finger segment scale X", 0, maxFingerValue, 1, KKABMX_Core.Instance) { TextColor = _settingColor });
            //var y = callback.AddControl(new MakerSlider(category, "Finger segment scale Y", 0, maxFingerValue, 1, KKABMX_Core.Instance) { TextColor = _settingColor });
            //var z = callback.AddControl(new MakerSlider(category, "Finger segment scale Z", 0, maxFingerValue, 1, KKABMX_Core.Instance) { TextColor = _settingColor });
            var v = callback.AddControl(new MakerSlider(category, "Finger segment scale", 0, maxFingerValue, 1, KKABMX_Core.Instance) { TextColor = _settingColor });

            void UpdateDisplay(int _)
            {
                var boneMod = _boneController.GetModifier(GetFingerBoneNames().First());
                var mod = boneMod?.GetModifier(_boneController.CurrentCoordinate.Value);
                SetSliders(mod?.ScaleModifier ?? Vector3.one);
            }

            var isUpdatingValue = false;

            void SetSliders(Vector3 mod)
            {
                isUpdatingValue = true;
                //if (x != null) x.Value = mod.x;
                //if (y != null) y.Value = mod.y;
                //if (z != null) z.Value = mod.z;
                if (v != null) v.Value = (mod.x + mod.y + mod.z) / 3f;
                isUpdatingValue = false;
            }

            void PushValueToControls()
            {
                UpdateDisplay(0);

                ActivateSliders();
            }

            _updateActionList.Add(PushValueToControls);
            PushValueToControls();

            rbSide.ValueChanged.Subscribe(UpdateDisplay);
            rbFing.ValueChanged.Subscribe(UpdateDisplay);
            rbSegm.ValueChanged.Subscribe(UpdateDisplay);

            void PullValuesToBone(float _)
            {
                if (isUpdatingValue) return;

                foreach (var boneName in GetFingerBoneNames())
                {
                    var bone = _boneController.GetModifier(boneName);
                    var modifier = bone?.GetModifier(_boneController.CurrentCoordinate.Value);

                    var prevValue = modifier?.ScaleModifier ?? Vector3.one;
                    var newValue = /*isAdvanced
                        ? new Vector3(x?.Value ?? prevValue.x, y?.Value ?? prevValue.y, z?.Value ?? prevValue.z)
                        :*/ new Vector3(v?.Value ?? prevValue.x, v?.Value ?? prevValue.y, v?.Value ?? prevValue.z);

                    if (modifier == null)
                    {
                        if (newValue == Vector3.one)
                            return;

                        if (bone == null)
                            bone = new BoneModifier(boneName);

                        _boneController.AddModifier(bone);
                        modifier = bone.GetModifier(_boneController.CurrentCoordinate.Value);
                    }

                    modifier.ScaleModifier = newValue;
                }

                SetSliders(/*isAdvanced
                    ? new Vector3(x?.Value ?? 1f, y?.Value ?? 1f, z?.Value ?? 1f)
                    :*/ new Vector3(v?.Value ?? 1f, v?.Value ?? 1f, v?.Value ?? 1f));
            }

            bool IsAdvanced()
            {
                return false;
                //if (XyzMode) return true;
                //if (x == null || y == null || z == null) return true;
                //var slidersAreEqual = Mathf.Approximately(x.Value, y.Value) && Mathf.Approximately(x.Value, z.Value);
                //return !slidersAreEqual;
            }

            void ActivateSliders()
            {
                isAdvanced = IsAdvanced();
                //x?.Visible.OnNext(isAdvanced);
                //y?.Visible.OnNext(isAdvanced);
                //z?.Visible.OnNext(isAdvanced);
                v?.Visible.OnNext(!isAdvanced);
            }

            IsAdvancedModeChanged += (_, __) => ActivateSliders();
            ActivateSliders();

            var obs = Observer.Create<float>(PullValuesToBone);
            //x?.ValueChanged.Subscribe(obs);
            //y?.ValueChanged.Subscribe(obs);
            //z?.ValueChanged.Subscribe(obs);
            v?.ValueChanged.Subscribe(obs);
        }

        private void RegisterSingleControl(MakerCategory category, BoneMeta boneMeta, RegisterCustomControlsEvent callback)
        {
            var boneName = boneMeta.BoneName;
            var rightBoneName = boneMeta.RightBoneName;

            if (MakerAPI.GetMakerSex() == SEX.MALE)
            {
                boneName = boneName.Replace("cf_", "cm_");
                rightBoneName = rightBoneName.Replace("cf_", "cm_");
            }

            //MakerRadioButtons rb = null; todo
            int? rb = null;

            if (!string.IsNullOrEmpty(rightBoneName))
            {
                //rb = callback.AddControl(new MakerRadioButtons(category, KKABMX_Core.Instance, "Side to edit", "Both", "Left", "Right") { TextColor = _settingColor });
                rb = 0;
            }

            var isAdvanced = true;
            var max = RaiseLimits ? boneMeta.Max * LimitRaiseAmount : boneMeta.Max;
            var lMax = RaiseLimits ? boneMeta.LMax * LimitRaiseAmount : boneMeta.LMax;
            var x = boneMeta.X ? callback.AddControl(new MakerSlider(category, boneMeta.XDisplayName, boneMeta.Min, max, 1, KKABMX_Core.Instance) { TextColor = _settingColor }) : null;
            var y = boneMeta.Y ? callback.AddControl(new MakerSlider(category, boneMeta.YDisplayName, boneMeta.Min, max, 1, KKABMX_Core.Instance) { TextColor = _settingColor }) : null;
            var z = boneMeta.Z ? callback.AddControl(new MakerSlider(category, boneMeta.ZDisplayName, boneMeta.Min, max, 1, KKABMX_Core.Instance) { TextColor = _settingColor }) : null;
            var v = boneMeta.X && boneMeta.Y && boneMeta.Z ? callback.AddControl(new MakerSlider(category, boneMeta.DisplayName + boneMeta.XYZPostfix, boneMeta.Min, max, 1, KKABMX_Core.Instance) { TextColor = _settingColor }) : null;

            var l = boneMeta.L ? callback.AddControl(new MakerSlider(category, boneMeta.LDisplayName, boneMeta.LMin, lMax, 1, KKABMX_Core.Instance) { TextColor = _settingColor }) : null;

            var isUpdatingValue = false;

            void SetSliders(Vector3 mod, float? lenMod = null)
            {
                isUpdatingValue = true;
                if (x != null) x.Value = mod.x;
                if (y != null) y.Value = mod.y;
                if (z != null) z.Value = mod.z;
                if (v != null) v.Value = (mod.x + mod.y + mod.z) / 3f;
                if (l != null && lenMod.HasValue) l.Value = lenMod.Value;
                isUpdatingValue = false;
            }

            void PushValueToControls()
            {
                var bone = GetBoneModifier(boneName, boneMeta.UniquePerCoordinate) ?? BoneModifierData.Default;

                if (rb != null)
                {
                    var bone2 = GetBoneModifier(rightBoneName, boneMeta.UniquePerCoordinate) ?? BoneModifierData.Default;
                    if (bone.ScaleModifier != bone2.ScaleModifier)
                    {
                        if (rb.Value == 0)
                        {
                            //rb.Value = 1;
                            rb = 1;

                        }
                        else if (rb.Value == 2)
                        {
                            bone = bone2;
                        }
                    }
                    else
                    {
                        //rb.Value = 0;
                        rb = 0;
                    }
                }

                SetSliders(bone.ScaleModifier, bone.LengthModifier);

                ActivateSliders();
            }

            _updateActionList.Add(PushValueToControls);
            PushValueToControls();

            //rb?.ValueChanged.Subscribe(
            //    i =>
            //    {
            //        if (i == 1)
            //        {
            //            var modifierData = GetBoneModifier(boneMeta.BoneName, boneMeta.UniquePerCoordinate);
            //            if (modifierData != null) SetSliders(modifierData.ScaleModifier, modifierData.LengthModifier);
            //        }
            //        else if (i == 2)
            //        {
            //            var modifierData = GetBoneModifier(boneMeta.RightBoneName, boneMeta.UniquePerCoordinate);
            //            if (modifierData != null) SetSliders(modifierData.ScaleModifier, modifierData.LengthModifier);
            //        }
            //    });

            void PullValuesToBone(float _)
            {
                if (isUpdatingValue) return;

                var modifier = GetBoneModifier(boneName, boneMeta.UniquePerCoordinate);
                var prevValue = modifier?.ScaleModifier ?? Vector3.one;
                var newValue = isAdvanced
                    ? new Vector3(x?.Value ?? prevValue.x, y?.Value ?? prevValue.y, z?.Value ?? prevValue.z)
                    : new Vector3(v?.Value ?? prevValue.x, v?.Value ?? prevValue.y, v?.Value ?? prevValue.z);
                if (modifier == null)
                {
                    var hasLen = l != null && Math.Abs(l.Value - 1f) > 0.001;
                    if (newValue == Vector3.one && !hasLen) return;

                    _boneController.AddModifier(new BoneModifier(boneName));
                    modifier = GetBoneModifier(boneName, boneMeta.UniquePerCoordinate);
                }

                if (rb != null)
                {
                    if (rb.Value != 1)
                    {
                        var bone2 = GetBoneModifier(rightBoneName, boneMeta.UniquePerCoordinate);
                        if (bone2 == null)
                        {
                            _boneController.AddModifier(new BoneModifier(rightBoneName));
                            bone2 = GetBoneModifier(rightBoneName, boneMeta.UniquePerCoordinate);
                        }

                        if (rb.Value == 0)
                        {
                            bone2.ScaleModifier = newValue;
                            if (l != null) bone2.LengthModifier = l.Value;
                        }
                        else if (rb.Value == 2)
                        {
                            bone2.ScaleModifier = newValue;
                            if (l != null) bone2.LengthModifier = l.Value;
                            return;
                        }
                    }
                }

                modifier.ScaleModifier = newValue;
                if (l != null) modifier.LengthModifier = l.Value;

                SetSliders(newValue);
            }

            bool IsAdvanced()
            {
                if (XyzMode) return true;
                if (x == null || y == null || z == null) return true;
                var slidersAreEqual = Mathf.Approximately(x.Value, y.Value) && Mathf.Approximately(x.Value, z.Value);
                return !slidersAreEqual;
            }

            void ActivateSliders()
            {
                isAdvanced = IsAdvanced();
                x?.Visible.OnNext(isAdvanced);
                y?.Visible.OnNext(isAdvanced);
                z?.Visible.OnNext(isAdvanced);
                v?.Visible.OnNext(!isAdvanced);
            }

            IsAdvancedModeChanged += (_, __) => ActivateSliders();
            ActivateSliders();

            var obs = Observer.Create<float>(PullValuesToBone);
            x?.ValueChanged.Subscribe(obs);
            y?.ValueChanged.Subscribe(obs);
            z?.ValueChanged.Subscribe(obs);
            v?.ValueChanged.Subscribe(obs);
            l?.ValueChanged.Subscribe(obs);
        }

        private BoneModifierData GetBoneModifier(string boneName, bool coordinateUnique)
        {
            var boneMod = _boneController.GetModifier(boneName);
            if (boneMod == null)
                return null;

            if (coordinateUnique)
                boneMod.MakeCoordinateSpecific();

            return boneMod.GetModifier(_boneController.CurrentCoordinate.Value);
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
            _boneController = MakerAPI.GetCharacterControl().GetComponent<BoneController>();
            if (_boneController == null)
            {
                KKABMX_Core.Logger.LogError("[GUI] Failed to find a BoneController or there are no bone modifiers");
                return;
            }

            _boneController.NewDataLoaded += (s, args) =>
            {
                foreach (var action in _updateActionList)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        KKABMX_Core.Logger.LogError(ex.ToString());
                    }
                }
            };

            RegisterCustomControls(e);
        }

        private void OnMakerExiting(object sender, EventArgs e)
        {
            IsAdvancedModeChanged = null;

            _updateActionList.Clear();
            _boneController = null;

            var reg = GetRegistration();
            reg.MaintainState = false;
            reg.MaintainCoordinateState = false;

            _bodyLoadToggle = null;
            _faceLoadToggle = null;
        }
    }
}
