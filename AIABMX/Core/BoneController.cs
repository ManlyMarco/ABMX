using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IllusionUtility.GetUtility;
using KKAPI;
using KKAPI.Chara;
using Manager;
using MessagePack;
using UnityEngine;
using Logger = KKABMX.Core.KKABMX_Core;
using ExtensibleSaveFormat;
using KKAPI.Maker;
using UniRx;

namespace KKABMX.Core
{
    public enum CoordinateType
    {
        Unknown = 0
    }

    public class BoneController : CharaCustomFunctionController
    {
        private const string ExtDataBoneDataKey = "boneData";

        private readonly FindAssist _boneSearcher = new FindAssist();
        private bool? _baselineKnown;

        public bool NeedsFullRefresh { get; set; }
        public bool NeedsBaselineUpdate { get; set; }

        public List<BoneModifier> Modifiers { get; private set; } = new List<BoneModifier>();

        public IEnumerable<BoneEffect> AdditionalBoneEffects => _additionalBoneEffects;
        private readonly List<BoneEffect> _additionalBoneEffects = new List<BoneEffect>();
        private bool _isDuringHScene;


        public event EventHandler NewDataLoaded;

        public BehaviorSubject<CoordinateType> CurrentCoordinate = new BehaviorSubject<CoordinateType>(CoordinateType.Unknown);

        public List<string> NoRotationBones = new List<string>() {
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

        public void AddModifier(BoneModifier bone)
        {
            if (bone == null) throw new ArgumentNullException(nameof(bone));
            Modifiers.Add(bone);
            ModifiersFillInTransforms();
            bone.CollectBaseline();
        }

        /// <summary>
        /// Add specified bone effect and update state to make it work. If the effect is already added then this does nothing.
        /// </summary>
        public void AddBoneEffect(BoneEffect effect)
        {
            if (_additionalBoneEffects.Contains(effect)) return;

            _additionalBoneEffects.Add(effect);
        }

        public BoneModifier GetModifier(string boneName)
        {
            if (boneName == null) throw new ArgumentNullException(nameof(boneName));
            return Modifiers.FirstOrDefault(x => x.BoneName == boneName);
        }

        /// <summary>
        /// Get all transform names under the character object that could be bones
        /// </summary>
        public IEnumerable<string> GetAllPossibleBoneNames()
        {
            if (_boneSearcher.dictObjName == null)
                _boneSearcher.Initialize(ChaControl.transform);
            return _boneSearcher.dictObjName.Keys;
        }

        /* No coordinate saving in AIS
        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (maintainState) return;

            // Clear previous data for this coordinate from coord specific modifiers
            foreach (var modifier in Modifiers.Where(x => x.IsCoordinateSpecific()))
                modifier.GetModifier(CurrentCoordinate.Value).Clear();

            var data = GetCoordinateExtendedData(coordinate);
            if (data != null)
            {
                try
                {
                    if (data.version != 2)
                        throw new NotSupportedException($"Save version {data.version} is not supported");

                    var boneData = LZ4MessagePackSerializer.Deserialize<Dictionary<string, BoneModifierData>>((byte[])data.data[ExtDataBoneDataKey]);
                    if (boneData != null)
                    {
                        foreach (var modifier in boneData)
                        {
                            var target = GetModifier(modifier.Key);
                            if (target == null)
                            {
                                // Add any missing modifiers
                                target = new BoneModifier(modifier.Key);
                                AddModifier(target);
                            }
                            target.MakeCoordinateSpecific();
                            target.CoordinateModifiers[(int)CurrentCoordinate.Value] = modifier.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.LogError( "[KKABMX] Failed to load coordinate extended data - " + ex);
                }
            }

            StartCoroutine(OnDataChangedCo());
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            var toSave = Modifiers
                .Where(x => !x.IsEmpty())
                .Where(x => x.IsCoordinateSpecific())
                .ToDictionary(x => x.BoneName, x => x.GetModifier(CurrentCoordinate.Value));

            if (toSave.Count == 0)
                SetCoordinateExtendedData(coordinate, null);
            else
            {
                var pluginData = new PluginData { version = 2 };
                pluginData.data.Add(ExtDataBoneDataKey, LZ4MessagePackSerializer.Serialize(toSave));
                SetCoordinateExtendedData(coordinate, pluginData);
            }
        }*/

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            var toSave = Modifiers.Where(x => !x.IsEmpty()).ToList();

            if (toSave.Count == 0)
            {
                SetExtendedData(null);
                return;
            }

            var data = new PluginData { version = 2 };
            data.data.Add(ExtDataBoneDataKey, LZ4MessagePackSerializer.Serialize(toSave));
            SetExtendedData(data);
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            foreach (var modifier in Modifiers)
                modifier.Reset();

            // Stop baseline collection if it's running
            StopAllCoroutines();
            _baselineKnown = false;

            if (!maintainState && (GUI.KKABMX_GUI.LoadBody || GUI.KKABMX_GUI.LoadFace))
            {
                var newModifiers = new List<BoneModifier>();
                var data = GetExtendedData();
                if (data != null)
                {
                    try
                    {
                        switch (data.version)
                        {
                            case 2:
                                newModifiers = LZ4MessagePackSerializer.Deserialize<List<BoneModifier>>((byte[])data.data[ExtDataBoneDataKey]);
                                break;

                            default:
                                throw new NotSupportedException($"Save version {data.version} is not supported");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.LogError("[KKABMX] Failed to load extended data - " + ex);
                    }
                }

                if (GUI.KKABMX_GUI.LoadBody && GUI.KKABMX_GUI.LoadFace)
                {
                    Modifiers = newModifiers;
                }
                else
                {
                    var headRoot = transform.FindLoop("cf_j_head");
                    var headBones = new HashSet<string>(headRoot.GetComponentsInChildren<Transform>().Select(x => x.name));
                    headBones.Add(headRoot.name);
                    if (GUI.KKABMX_GUI.LoadFace)
                    {
                        Modifiers.RemoveAll(x => headBones.Contains(x.BoneName));
                        Modifiers.AddRange(newModifiers.Where(x => headBones.Contains(x.BoneName)));
                    }
                    else if (GUI.KKABMX_GUI.LoadBody)
                    {
                        var bodyBones = new HashSet<string>(transform.FindLoop("BodyTop").GetComponentsInChildren<Transform>().Select(x => x.name).Except(headBones));

                        Modifiers.RemoveAll(x => bodyBones.Contains(x.BoneName));
                        Modifiers.AddRange(newModifiers.Where(x => bodyBones.Contains(x.BoneName)));
                    }
                }
            }

            StartCoroutine(OnDataChangedCo());
        }

        protected override void Start()
        {
            base.Start();
            CurrentCoordinate.Subscribe(_ => StartCoroutine(OnDataChangedCo()));
            _isDuringHScene = "H".Equals(Scene.Instance.LoadSceneName, StringComparison.Ordinal);
        }

        private void LateUpdate()
        {
                if (NeedsFullRefresh)
            {
                OnReload(KoikatuAPI.GetCurrentGameMode(), true);
                NeedsFullRefresh = false;
                return;
            }

            if (_baselineKnown == true)
            {
                if (NeedsBaselineUpdate)
                    UpdateBaseline();

                ApplyEffects();
            }
            else if (_baselineKnown == false)
            {
                _baselineKnown = null;
                CollectBaseline();
            }

            NeedsBaselineUpdate = false;
        }

        private void ApplyEffects()
        {
            var toUpdate = new Dictionary<BoneModifier, List<BoneModifierData>>();

            for (var i = 0; i < _additionalBoneEffects.Count; i++)
            {
                var additionalBoneEffect = _additionalBoneEffects[i];
                var affectedBones = additionalBoneEffect.GetAffectedBones(this);
                foreach (var affectedBone in affectedBones)
                {
                    var effect = additionalBoneEffect.GetEffect(affectedBone, this, CurrentCoordinate.Value);

                    if (effect != null && !effect.IsEmpty())
                    {
                        var modifier = Modifiers.Find(x => string.Equals(x.BoneName, affectedBone, StringComparison.Ordinal));
                        if (modifier == null)
                        {
                            modifier = new BoneModifier(affectedBone);
                            AddModifier(modifier);
                        }

                        if (!toUpdate.TryGetValue(modifier, out var list))
                        {
                            list = new List<BoneModifierData>();
                            toUpdate[modifier] = list;
                        }
                        list.Add(effect);
                    }
                }
            }

            for (var i = 0; i < Modifiers.Count; i++)
            {
                var modifier = Modifiers[i];
                if (!toUpdate.TryGetValue(modifier, out var list))
                {
                    // Clean up no longer necessary modifiers
                    if (!MakerAPI.InsideMaker && modifier.IsEmpty())
                    {
                        modifier.Reset();
                        Modifiers.Remove(modifier);
                    }
                }

                modifier.Apply(CurrentCoordinate.Value, list, _isDuringHScene, NoRotationBones);
            }
            ChaControl.UpdateBustGravity();
        }

        private IEnumerator OnDataChangedCo()
        {
            foreach (var modifier in Modifiers.Where(x => x.IsEmpty()).ToList())
            {
                modifier.Reset();
                Modifiers.Remove(modifier);
            }

            // Needed to let accessories load in
            yield return new WaitForEndOfFrame();

            ModifiersFillInTransforms();

            NeedsBaselineUpdate = false;

            NewDataLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void CollectBaseline()
        {
            StartCoroutine(CollectBaselineCo());
        }

        private IEnumerator CollectBaselineCo()
        {
            yield return new WaitForEndOfFrame();
            while (ChaControl.animBody == null) yield break;

#if KK || AI
            var pvCopy = ChaControl.animBody.gameObject.GetComponent<Studio.PVCopy>();
            bool[] currentPvCopy = null;
            if (pvCopy != null)
            {
                var pvCount = pvCopy.GetPvArray().Length;
                currentPvCopy = new bool[pvCount];
                for (var i = 0; i < currentPvCopy.Length; i++)
                {
                    currentPvCopy[i] = pvCopy[i];
                    pvCopy[i] = false;
                }
            }
#endif

            yield return new WaitForEndOfFrame();

            // Ensure that the baseline is correct
            ChaControl.updateShapeFace = true;
            ChaControl.updateShapeBody = true;
            ChaControl.LateUpdateForce();
            foreach (var modifier in Modifiers)
                modifier.CollectBaseline();

            _baselineKnown = true;

            yield return new WaitForEndOfFrame();

#if KK || AI
            if (pvCopy != null)
            {
                var array = pvCopy.GetPvArray();
                var array2 = pvCopy.GetBoneArray();
                for (var j = 0; j < currentPvCopy.Length; j++)
                {
                    if (currentPvCopy[j] && array2[j] && array[j])
                    {
                        array[j].transform.localScale = array2[j].transform.localScale;
                        array[j].transform.position = array2[j].transform.position;
                        array[j].transform.rotation = array2[j].transform.rotation;
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Partial baseline update.
        /// Needed mainly to prevent vanilla sliders in chara maker from being overriden by bone modifiers.
        /// </summary>
        private void UpdateBaseline()
        {
            var distSrc = ChaControl.GetSibFace().GetDictDst();
            var distSrc2 = ChaControl.GetSibBody().GetDictDst();
            var affectedBones = new HashSet<Transform>(distSrc.Concat(distSrc2).Select(x => x.Value.trfBone));
            var affectedModifiers = Modifiers.Where(x => affectedBones.Contains(x.BoneTransform)).ToList();

            // Prevent some scales from being added to the baseline, mostly skirt scale
            foreach (var boneModifier in affectedModifiers)
                boneModifier.Reset();

            // Force game to recalculate bone scales. Misses some so we need to reset above
            ChaControl.UpdateShapeFace();
            ChaControl.UpdateShapeBody();

            foreach (var boneModifier in affectedModifiers)
                boneModifier.CollectBaseline();
        }

        private void ModifiersFillInTransforms()
        {
            if (Modifiers.Count == 0) return;

            var initializedBones = false;
            foreach (var modifier in Modifiers)
            {
                if (modifier.BoneTransform != null) continue;

                Retry:
                var boneObj = _boneSearcher.GetObjectFromName(modifier.BoneName);
                if (boneObj != null)
                    modifier.BoneTransform = boneObj.transform;
                else
                {
                    if (!initializedBones)
                    {
                        initializedBones = true;
                        _boneSearcher.Initialize(ChaControl.transform);
                        goto Retry;
                    }
                }
            }
        }
    }
}
