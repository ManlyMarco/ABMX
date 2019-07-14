using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using KKABMX.Core;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Maker.UI.Sidebar;
using UniRx;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace KKABMX.GUI
{
    [BepInPlugin("KKABMX.GUI", "KKABMX GUI", KKABMX_Core.Version)]
    [BepInDependency(KoikatuAPI.GUID)]
    [BepInDependency(KKABMX_Core.GUID)]
    public class KKABMX_GUI : BaseUnityPlugin
    {
        private const int LimitRaiseAmount = 2;
        private static readonly Color _settingColor = new Color(1f, 0.84f, 0.57f);

        private BoneController _boneController;
        private readonly List<Action> _updateActionList = new List<Action>();

        private static MakerLoadToggle _faceLoadToggle;
        private static MakerLoadToggle _bodyLoadToggle;
        internal static bool LoadFace => _faceLoadToggle == null || _faceLoadToggle.Value;
        internal static bool LoadBody => _bodyLoadToggle == null || _bodyLoadToggle.Value;

        [DisplayName("Increase slider limits 2x")]
        [Description("Can cause even more horrifying results. Only enable when working on furries and superdeformed charas.")]
        public ConfigWrapper<bool> RaiseLimits { get; }

        public KKABMX_GUI()
        {
            RaiseLimits = new ConfigWrapper<bool>(nameof(RaiseLimits), this);
        }

        private void Start()
        {
            KoikatuAPI.CheckRequiredPlugin(this, KoikatuAPI.GUID, new Version(KoikatuAPI.VersionConst));

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
                    if (boneMeta.IsSeparator || !first)
                        callback.AddControl(new MakerSeparator(category, this) { TextColor = _settingColor });

                    RegisterSingleControl(category, boneMeta, callback);
                    first = false;
                }

                if (ReferenceEquals(category, InterfaceData.BodyHands))
                {
                    if (!first)
                        callback.AddControl(new MakerSeparator(category, this) { TextColor = _settingColor });

                    RegisterFingerControl(category, callback);
                }
            }

            _faceLoadToggle = callback.AddLoadToggle(new MakerLoadToggle("Face Bonemod"));
            _bodyLoadToggle = callback.AddLoadToggle(new MakerLoadToggle("Body Bonemod"));

            callback.AddCoordinateLoadToggle(new MakerCoordinateLoadToggle("Bonemod"))
                .ValueChanged.Subscribe(b => GetRegistration().MaintainCoordinateState = !b);

            callback.AddSidebarControl(new SidebarToggle("Show advanced bonemod controls", false, this))
                .ValueChanged.Subscribe(b => gameObject.GetComponent<KKABMX_AdvancedGUI>().enabled = b);
        }

        private static CharacterApi.ControllerRegistration GetRegistration()
        {
            return CharacterApi.GetRegisteredBehaviour(typeof(BoneController));
        }

        private void RegisterFingerControl(MakerCategory category, RegisterCustomControlsEvent callback)
        {
            var rbSide = callback.AddControl(new MakerRadioButtons(category, this, "Hand to edit", "Both", "Left", "Right") { TextColor = _settingColor });
            var rbFing = callback.AddControl(new MakerRadioButtons(category, this, "Finger to edit", "All", "1", "2", "3", "4", "5") { TextColor = _settingColor });
            var rbSegm = callback.AddControl(new MakerRadioButtons(category, this, "Segment to edit", "Base", "Center", "Tip") { TextColor = _settingColor });

            IEnumerable<string> GetFingerBoneNames()
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

            var maxFingerValue = RaiseLimits.Value ? 3 * LimitRaiseAmount : 3;
            var x = callback.AddControl(new MakerSlider(category, "Scale X", 0, maxFingerValue, 1, this) { TextColor = _settingColor });
            var y = callback.AddControl(new MakerSlider(category, "Scale Y", 0, maxFingerValue, 1, this) { TextColor = _settingColor });
            var z = callback.AddControl(new MakerSlider(category, "Scale Z", 0, maxFingerValue, 1, this) { TextColor = _settingColor });

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
                if (x != null) x.Value = mod.x;
                if (y != null) y.Value = mod.y;
                if (z != null) z.Value = mod.z;
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

                foreach (var boneName in GetFingerBoneNames())
                {
                    var bone = _boneController.GetModifier(boneName);
                    var modifier = bone?.GetModifier(_boneController.CurrentCoordinate.Value);

                    var prevValue = modifier?.ScaleModifier ?? Vector3.one;
                    var newValue = new Vector3(x?.Value ?? prevValue.x, y?.Value ?? prevValue.y, z?.Value ?? prevValue.z);

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
            }

            var obs = Observer.Create<float>(PullValuesToBone);
            x?.ValueChanged.Subscribe(obs);
            y?.ValueChanged.Subscribe(obs);
            z?.ValueChanged.Subscribe(obs);
        }

        private void RegisterSingleControl(MakerCategory category, BoneMeta boneMeta, RegisterCustomControlsEvent callback)
        {
            MakerRadioButtons rb = null;
            if (!string.IsNullOrEmpty(boneMeta.RightBoneName))
            {
                rb = callback.AddControl(new MakerRadioButtons(category, this, "Side to edit", "Both", "Left", "Right") { TextColor = _settingColor });
            }

            var max = RaiseLimits.Value ? boneMeta.Max * LimitRaiseAmount : boneMeta.Max;
            var lMax = RaiseLimits.Value ? boneMeta.LMax * LimitRaiseAmount : boneMeta.LMax;
            var x = boneMeta.X ? callback.AddControl(new MakerSlider(category, boneMeta.XDisplayName, boneMeta.Min, max, 1, this) { TextColor = _settingColor }) : null;
            var y = boneMeta.Y ? callback.AddControl(new MakerSlider(category, boneMeta.YDisplayName, boneMeta.Min, max, 1, this) { TextColor = _settingColor }) : null;
            var z = boneMeta.Z ? callback.AddControl(new MakerSlider(category, boneMeta.ZDisplayName, boneMeta.Min, max, 1, this) { TextColor = _settingColor }) : null;
            var l = boneMeta.L ? callback.AddControl(new MakerSlider(category, boneMeta.LDisplayName, boneMeta.LMin, lMax, 1, this) { TextColor = _settingColor }) : null;

            var isUpdatingValue = false;

            void SetSliders(Vector3 mod, float lenMod)
            {
                isUpdatingValue = true;
                if (x != null) x.Value = mod.x;
                if (y != null) y.Value = mod.y;
                if (z != null) z.Value = mod.z;
                if (l != null) l.Value = lenMod;
                isUpdatingValue = false;
            }

            void PushValueToControls()
            {
                var bone = GetBoneModifier(boneMeta.BoneName, boneMeta.UniquePerCoordinate);

                if (bone == null)
                {
                    SetSliders(Vector3.one, 1f);
                    return;
                }

                if (rb != null)
                {
                    var bone2 = GetBoneModifier(boneMeta.RightBoneName, boneMeta.UniquePerCoordinate);
                    if (bone2 == null) throw new ArgumentNullException(nameof(bone2));
                    if (bone.ScaleModifier != bone2.ScaleModifier)
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

                SetSliders(bone.ScaleModifier, bone.LengthModifier);
            }

            _updateActionList.Add(PushValueToControls);
            PushValueToControls();

            rb?.ValueChanged.Subscribe(
                i =>
                {
                    if (i == 1)
                    {
                        var modifierData = GetBoneModifier(boneMeta.BoneName, boneMeta.UniquePerCoordinate);
                        if (modifierData != null) SetSliders(modifierData.ScaleModifier, modifierData.LengthModifier);
                    }
                    else if (i == 2)
                    {
                        var modifierData = GetBoneModifier(boneMeta.RightBoneName, boneMeta.UniquePerCoordinate);
                        if (modifierData != null) SetSliders(modifierData.ScaleModifier, modifierData.LengthModifier);
                    }
                });

            void PullValuesToBone(float _)
            {
                if (isUpdatingValue) return;

                var bone = GetBoneModifier(boneMeta.BoneName, boneMeta.UniquePerCoordinate);
                var prevValue = bone?.ScaleModifier ?? Vector3.one;
                var newValue = new Vector3(x?.Value ?? prevValue.x, y?.Value ?? prevValue.y, z?.Value ?? prevValue.y);
                if (bone == null)
                {
                    var hasLen = l != null && Math.Abs(l.Value - 1f) > 0.001;
                    if (newValue == Vector3.one && !hasLen) return;

                    _boneController.AddModifier(new BoneModifier(boneMeta.BoneName));
                    bone = GetBoneModifier(boneMeta.BoneName, boneMeta.UniquePerCoordinate);
                }

                if (rb != null)
                {
                    if (rb.Value != 1)
                    {
                        var bone2 = GetBoneModifier(boneMeta.RightBoneName, boneMeta.UniquePerCoordinate);
                        if (bone2 == null)
                        {
                            _boneController.AddModifier(new BoneModifier(boneMeta.RightBoneName));
                            bone2 = GetBoneModifier(boneMeta.RightBoneName, boneMeta.UniquePerCoordinate);
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

                bone.ScaleModifier = newValue;
                if (l != null) bone.LengthModifier = l.Value;
            }

            var obs = Observer.Create<float>(PullValuesToBone);
            x?.ValueChanged.Subscribe(obs);
            y?.ValueChanged.Subscribe(obs);
            z?.ValueChanged.Subscribe(obs);
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
            _boneController = FindObjectOfType<BoneController>();
            if (_boneController == null)
            {
                Logger.Log(LogLevel.Error, "[KKABMX_GUI] Failed to find a BoneController or there are no bone modifiers");
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
                        KKABMX_Core.Log(LogLevel.Error, ex.ToString());
                    }
                }
            };

            gameObject.AddComponent<KKABMX_AdvancedGUI>().enabled = false;

            RegisterCustomControls(e);
        }

        private void OnMakerExiting(object sender, EventArgs e)
        {
            _updateActionList.Clear();
            _boneController = null;

            var reg = GetRegistration();
            reg.MaintainState = false;
            reg.MaintainCoordinateState = false;

            _bodyLoadToggle = null;
            _faceLoadToggle = null;

            Destroy(gameObject.GetComponent<KKABMX_AdvancedGUI>());
        }
    }
}
