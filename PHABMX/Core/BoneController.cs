using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using IllusionUtility.GetUtility;
using KKAPI;
using KKAPI.Chara;
using MessagePack;
using UnityEngine;
using ExtensibleSaveFormat;
using UniRx;

namespace KKABMX.Core
{
    /// <summary>
    /// Placeholder for AIS to keep the API compatibility
    /// </summary>
    public enum CoordinateType
    {
        /// <summary>
        /// Current coordinate in AIS
        /// </summary>
        Unknown = 0
    }

    /// <summary>
    /// Manages and applies bone modifiers for a single character.
    /// </summary>
    public class BoneController : CharaCustomFunctionController
    {
        private const string ExtDataBoneDataKey = "boneData";

        private readonly FindAssist _boneSearcher = new FindAssist();
        private bool? _baselineKnown;

        /// <summary>
        /// Trigger a full bone modifier refresh on the next update
        /// </summary>
        public bool NeedsFullRefresh { get; set; }
        /// <summary>
        /// Trigger all modifiers to collect new baselines on the next update
        /// </summary>
        public bool NeedsBaselineUpdate { get; set; }

        /// <summary>
        /// All bone modifiers assigned to this controller
        /// </summary>
        public List<BoneModifier> Modifiers { get; private set; } = new List<BoneModifier>();

        /// <summary>
        /// Additional effects that other plugins can apply to a character
        /// </summary>
        public IEnumerable<BoneEffect> AdditionalBoneEffects => _additionalBoneEffects;
        private readonly List<BoneEffect> _additionalBoneEffects = new List<BoneEffect>();
        private bool _isDuringHScene;

        /// <summary>
        /// Signals that new modifier data was loaded and custom Modifiers and AdditionalBoneEffects might need to be updated
        /// </summary>
        public event EventHandler NewDataLoaded;

        /// <summary>
        /// Placeholder to keep the API compatibility
        /// </summary>
        public BehaviorSubject<CoordinateType> CurrentCoordinate = new BehaviorSubject<CoordinateType>(CoordinateType.Unknown);

        /// <summary>
        /// Add a new bone modifier. Make sure it doesn't exist yet.
        /// </summary>
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

        /// <summary>
        /// Get a modifier if it exists.
        /// </summary>
        /// <param name="boneName">Name of the bone that the modifier targets</param>
        public BoneModifier GetModifier(string boneName)
        {
            if (boneName == null) throw new ArgumentNullException(nameof(boneName));
            return Modifiers.FirstOrDefault(x => x.BoneName == boneName);
        }

        /// <summary>
        /// Removes the specified modifier and resets the affected bone to its original state
        /// </summary>
        /// <param name="modifier">Modifier added to this controller</param>
        public void RemoveModifier(BoneModifier modifier)
        {
            modifier.Reset();
            Modifiers.Remove(modifier);

            ForceBodyRecalc();
        }

        /// <summary>
        /// Get all transform names under the character object that could be bones
        /// </summary>
        public IEnumerable<string> GetAllPossibleBoneNames()
        {
            if (_boneSearcher.dictObjName == null)
                _boneSearcher.Initialize(GetBodyRootTransform());
            return _boneSearcher.dictObjName.Keys
                .Where(x => !x.StartsWith("f_t_", StringComparison.Ordinal) &&
                            !x.StartsWith("f_pv_", StringComparison.Ordinal) &&
                            !x.StartsWith("f_k_", StringComparison.Ordinal) &&
                            !x.StartsWith("m_t_", StringComparison.Ordinal) &&
                            !x.StartsWith("m_pv_", StringComparison.Ordinal) &&
                            !x.StartsWith("m_k_", StringComparison.Ordinal));
        }

        private Transform GetBodyRootTransform()
        {
            return ChaControl.transform.Find(ChaControl.sex == SEX.FEMALE ? "p_cf_anim" : "p_cm_anim");
        }

        //#if !AI //No coordinate saving in AIS
        //        protected override void OnCoordinateBeingLoaded(CustomParameter coordinate, bool maintainState)
        //        {
        //            if (maintainState) return;
        //
        //            // Clear previous data for this coordinate from coord specific modifiers
        //            foreach (var modifier in Modifiers.Where(x => x.IsCoordinateSpecific()))
        //                modifier.GetModifier(CurrentCoordinate.Value).Clear();
        //
        //            var data = GetCoordinateExtendedData(coordinate);
        //            if (data != null)
        //            {
        //                try
        //                {
        //                    if (data.version != 2)
        //                        throw new NotSupportedException($"Save version {data.version} is not supported");
        //
        //                    var boneData = LZ4MessagePackSerializer.Deserialize<Dictionary<string, BoneModifierData>>((byte[])data.data[ExtDataBoneDataKey]);
        //                    if (boneData != null)
        //                    {
        //                        foreach (var modifier in boneData)
        //                        {
        //                            var target = GetModifier(modifier.Key);
        //                            if (target == null)
        //                            {
        //                                // Add any missing modifiers
        //                                target = new BoneModifier(modifier.Key);
        //                                AddModifier(target);
        //                            }
        //                            target.MakeCoordinateSpecific();
        //                            target.CoordinateModifiers[(int)CurrentCoordinate.Value] = modifier.Value;
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    KKABMX_Core.Logger.LogError("[KKABMX] Failed to load coordinate extended data - " + ex);
        //                }
        //            }
        //
        //            StartCoroutine(OnDataChangedCo());
        //        }
        //
        //        protected override void OnCoordinateBeingSaved(CustomParameter coordinate)
        //        {
        //            var toSave = Modifiers
        //                .Where(x => !x.IsEmpty())
        //                .Where(x => x.IsCoordinateSpecific())
        //                .ToDictionary(x => x.BoneName, x => x.GetModifier(CurrentCoordinate.Value));
        //
        //            if (toSave.Count == 0)
        //                SetCoordinateExtendedData(coordinate, null);
        //            else
        //            {
        //                var pluginData = new PluginData { version = 2 };
        //                pluginData.data.Add(ExtDataBoneDataKey, LZ4MessagePackSerializer.Serialize(toSave));
        //                SetCoordinateExtendedData(coordinate, pluginData);
        //            }
        //        }
        //#endif

        /// <inheritdoc />
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

        internal void RevertChanges() => OnReload(KoikatuAPI.GetCurrentGameMode(), false);

        /// <inheritdoc />
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
                try
                {
                    var data = GetExtendedData();
                    if (data != null)
                    {
                        try
                        {
                            switch (data.version)
                            {
                                case 2:
                                    newModifiers =
                                        LZ4MessagePackSerializer.Deserialize<List<BoneModifier>>(
                                            (byte[])data.data[ExtDataBoneDataKey]);
                                    break;

                                default:
                                    throw new NotSupportedException($"Save version {data.version} is not supported");
                            }
                        }
                        catch (Exception ex)
                        {
                            KKABMX_Core.Logger.LogError("[KKABMX] Failed to load extended data - " + ex);
                        }
                    }
                    else
                    {
                        var legacyData = OldDataConverter.ImportOldData(CharacterApi.GetLastLoadedCardPath(ChaControl), ChaControl);
                        if (legacyData != null)
                            newModifiers = legacyData;
                    }
                }
                catch (Exception ex)
                {
                    KKABMX_Core.Logger.LogError("Failed to load bonemod data: " + ex);
                }

                if (GUI.KKABMX_GUI.LoadBody && GUI.KKABMX_GUI.LoadFace)
                {
                    Modifiers = newModifiers;
                }
                else
                {
                    var bodyRoot = GetBodyRootTransform();
                    var headRoot = bodyRoot.FindLoop(ChaControl.sex == SEX.MALE ? "cm_J_Head" : "cf_J_Head");

                    var headBones = new HashSet<string>(headRoot.GetComponentsInChildren<Transform>().Select(x => x.name));
                    headBones.Add(headRoot.name);
                    if (GUI.KKABMX_GUI.LoadFace)
                    {
                        Modifiers.RemoveAll(x => headBones.Contains(x.BoneName));
                        Modifiers.AddRange(newModifiers.Where(x => headBones.Contains(x.BoneName)));
                    }
                    else if (GUI.KKABMX_GUI.LoadBody)
                    {
                        var bodyBones = new HashSet<string>(bodyRoot.GetComponentsInChildren<Transform>().Select(x => x.name).Except(headBones));

                        Modifiers.RemoveAll(x => bodyBones.Contains(x.BoneName));
                        Modifiers.AddRange(newModifiers.Where(x => bodyBones.Contains(x.BoneName)));
                    }
                }
            }

            StartCoroutine(OnDataChangedCo());
        }

        /// <inheritdoc />
        protected override void Start()
        {
            base.Start();
            CurrentCoordinate.Subscribe(_ => StartCoroutine(OnDataChangedCo()));
            //_isDuringHScene = "H".Equals(Scene.Instance.LoadSceneName, StringComparison.Ordinal);
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

            foreach (var additionalBoneEffect in _additionalBoneEffects)
            {
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
                    if (!GUI.KKABMX_AdvancedGUI.Enabled && modifier.IsEmpty())
                        RemoveModifier(modifier);
                }

                modifier.Apply(CurrentCoordinate.Value, list, _isDuringHScene);
            }

            //// Fix some bust physics issues
            //if (Modifiers.Count > 0)
            //    ChaControl.body.bustWeight.UpdateBustGravity();
        }

        private IEnumerator OnDataChangedCo()
        {
            CleanEmptyModifiers();

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
            while (ChaControl.body.Anime == null) yield break;

            //var pvCopy = ChaControl.animBody.gameObject.GetComponent<Studio.PVCopy>();
            //bool[] currentPvCopy = null;
            //if (pvCopy != null)
            //{
            //    var pvCount = pvCopy.GetPvArray().Length;
            //    currentPvCopy = new bool[pvCount];
            //    for (var i = 0; i < currentPvCopy.Length; i++)
            //    {
            //        currentPvCopy[i] = pvCopy[i];
            //        pvCopy[i] = false;
            //    }
            //}

            yield return new WaitForEndOfFrame();

            // Ensure that the baseline is correct
            ForceBodyRecalc();

            foreach (var modifier in Modifiers)
                modifier.CollectBaseline();

            _baselineKnown = true;

            //yield return new WaitForEndOfFrame();
            //
            //if (pvCopy != null)
            //{
            //    var array = pvCopy.GetPvArray();
            //    var array2 = pvCopy.GetBoneArray();
            //    for (var j = 0; j < currentPvCopy.Length; j++)
            //    {
            //        if (currentPvCopy[j] && array2[j] && array[j])
            //        {
            //            array[j].transform.localScale = array2[j].transform.localScale;
            //            array[j].transform.position = array2[j].transform.position;
            //            array[j].transform.rotation = array2[j].transform.rotation;
            //        }
            //    }
            //}
        }

        private void ForceBodyRecalc()
        {
            //ChaControl.updateShapeFace = true;
            //ChaControl.updateShapeBody = true;
            ChaControl.body.ShapeApply();
            ChaControl.head.ShapeApply();
            NeedsBaselineUpdate = false;
        }

        /// <summary>
        /// Partial baseline update.
        /// Needed mainly to prevent vanilla sliders in chara maker from being overriden by bone modifiers.
        /// </summary>
        private void UpdateBaseline()
        {
            //var distSrc = ChaControl.GetSibFace().GetDictDst();
            //var distSrc2 = ChaControl.GetSibBody().GetDictDst();
            //var affectedBones = new HashSet<Transform>(distSrc.Concat(distSrc2).Select(x => x.Value.trfBone));
            //var affectedModifiers = Modifiers.Where(x => affectedBones.Contains(x.BoneTransform)).ToList();
            var affectedModifiers = Modifiers;

            // Prevent some scales from being added to the baseline, mostly skirt scale
            foreach (var boneModifier in affectedModifiers)
                boneModifier.Reset();

            // Force game to recalculate bone scales. Misses some so we need to reset above
            ForceBodyRecalc();

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
                        _boneSearcher.Initialize(GetBodyRootTransform());
                        goto Retry;
                    }
                }
            }
        }

        internal void CleanEmptyModifiers()
        {
            foreach (var modifier in Modifiers.Where(x => x.IsEmpty()).ToList())
            {
                modifier.Reset();
                Modifiers.Remove(modifier);
            }
        }
    }
}
