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
using ExtensibleSaveFormat;
using KKAPI.Utilities;
using UniRx;

namespace KKABMX.Core
{
#if KK
    using CoordinateType = ChaFileDefine.CoordinateType;
#elif EC
    using CoordinateType = KoikatsuCharaFile.ChaFileDefine.CoordinateType;
#elif AI || HS2
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
#endif

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

#if EC
        /// <summary>
        /// Placeholder to keep the API compatibility, all coordinate logic targets the KK School01 slot
        /// </summary>
        public BehaviorSubject<CoordinateType> CurrentCoordinate = new BehaviorSubject<CoordinateType>(CoordinateType.School01);
#elif AI || HS2
        /// <summary>
        /// Placeholder to keep the API compatibility
        /// </summary>
        public BehaviorSubject<CoordinateType> CurrentCoordinate = new BehaviorSubject<CoordinateType>(CoordinateType.Unknown);
#endif

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
            for (var i = 0; i < Modifiers.Count; i++)
            {
                var x = Modifiers[i];
                if (x.BoneName == boneName) return x;
            }

            return null;
        }

        /// <summary>
        /// Removes the specified modifier and resets the affected bone to its original state
        /// </summary>
        /// <param name="modifier">Modifier added to this controller</param>
        public void RemoveModifier(BoneModifier modifier)
        {
            modifier.Reset();
            Modifiers.Remove(modifier);

            ChaControl.updateShapeFace = true;
            ChaControl.updateShapeBody = true;
        }

        /// <summary>
        /// Get all transform names under the character object that could be bones
        /// </summary>
        public IEnumerable<string> GetAllPossibleBoneNames()
        {
            if (_boneSearcher.dictObjName == null)
                _boneSearcher.Initialize(ChaControl.transform);
            return _boneSearcher.dictObjName.Keys
#if AI || HS2
                .Where(x => !x.StartsWith("f_t_", StringComparison.Ordinal) && !x.StartsWith("f_pv_", StringComparison.Ordinal) && !x.StartsWith("f_k_", StringComparison.Ordinal))
#elif KK || EC
                .Where(x => !x.StartsWith("cf_t_", StringComparison.Ordinal) && !x.StartsWith("cf_pv_", StringComparison.Ordinal))
#endif
                ;
        }

#if !AI && !HS2 //No coordinate saving in AIS
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
                    KKABMX_Core.Logger.LogError("[KKABMX] Failed to load coordinate extended data - " + ex);
                }
            }

            StartCoroutine(OnDataChangedCo());
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            var toSave = Modifiers
                .Where(x => !x.IsEmpty() && x.IsCoordinateSpecific())
                .ToDictionary(x => x.BoneName, x => x.GetModifier(CurrentCoordinate.Value));

            if (toSave.Count == 0)
                SetCoordinateExtendedData(coordinate, null);
            else
            {
                var pluginData = new PluginData { version = 2 };
                pluginData.data.Add(ExtDataBoneDataKey, LZ4MessagePackSerializer.Serialize(toSave));
                SetCoordinateExtendedData(coordinate, pluginData);
            }
        }
#endif

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
                List<BoneModifier> newModifiers = null;
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

#if KK || EC
                            case 1:
                                KKABMX_Core.Logger.LogDebug($"[KKABMX] Loading legacy embedded ABM data from card: {ChaFileControl.parameter?.fullname}");
                                newModifiers = OldDataConverter.MigrateOldExtData(data);
                                break;
#endif

                            default:
                                throw new NotSupportedException($"Save version {data.version} is not supported");
                        }
                    }
                    catch (Exception ex)
                    {
                        KKABMX_Core.Logger.LogError("[KKABMX] Failed to load extended data - " + ex);
                    }
                }

                if (newModifiers == null) newModifiers = new List<BoneModifier>();

                if (GUI.KKABMX_GUI.LoadBody && GUI.KKABMX_GUI.LoadFace)
                {
                    Modifiers = newModifiers;
                }
                else
                {
#if AI || HS2
                    var headRoot = transform.FindLoop("cf_J_Head");
#else
                    var headRoot = transform.FindLoop("cf_j_head");
#endif
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

        /// <inheritdoc />
        protected override void Start()
        {
            base.Start();
            CurrentCoordinate.Subscribe(_ => StartCoroutine(OnDataChangedCo()));
#if KK // hs2 ais is HScene
            _isDuringHScene = "H".Equals(Scene.Instance.LoadSceneName, StringComparison.Ordinal);
#endif
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

        private readonly Dictionary<BoneModifier, List<BoneModifierData>> _effectsToUpdate = new Dictionary<BoneModifier, List<BoneModifierData>>();

        private void ApplyEffects()
        {
            _effectsToUpdate.Clear();

            foreach (var additionalBoneEffect in _additionalBoneEffects)
            {
                var affectedBones = additionalBoneEffect.GetAffectedBones(this);
                foreach (var affectedBone in affectedBones)
                {
                    var effect = additionalBoneEffect.GetEffect(affectedBone, this, CurrentCoordinate.Value);
                    if (effect != null && !effect.IsEmpty())
                    {
                        var modifier = GetModifier(affectedBone);
                        if (modifier == null)
                        {
                            modifier = new BoneModifier(affectedBone);
                            AddModifier(modifier);
                        }

                        if (!_effectsToUpdate.TryGetValue(modifier, out var list))
                        {
                            list = new List<BoneModifierData>();
                            _effectsToUpdate[modifier] = list;
                        }
                        list.Add(effect);
                    }
                }
            }

            for (var i = 0; i < Modifiers.Count; i++)
            {
                var modifier = Modifiers[i];
                if (!_effectsToUpdate.TryGetValue(modifier, out var list))
                {
                    // Clean up no longer necessary modifiers
                    //if (!GUI.KKABMX_AdvancedGUI.Enabled && modifier.IsEmpty())
                    //    RemoveModifier(modifier);
                }

#if KK || EC
                // Force reset baseline of bones affected by dynamicbones
                // todo do the same for ai and hs2
                if (modifier.BoneName.StartsWith("cf_d_sk_", StringComparison.Ordinal) || 
                    modifier.BoneName.StartsWith("cf_j_bust0", StringComparison.Ordinal) || 
                    modifier.BoneName.StartsWith("cf_d_siri01_", StringComparison.Ordinal) || 
                    modifier.BoneName.StartsWith("cf_j_siri_", StringComparison.Ordinal))
                {
                    modifier.Reset();
                    modifier.CollectBaseline();
                }
#endif

                modifier.Apply(CurrentCoordinate.Value, list, _isDuringHScene);
            }

            // Fix some bust physics issues
            // bug - causes gravity issues on its own
            if (Modifiers.Count > 0)
                ChaControl.UpdateBustGravity();
        }

        private IEnumerator OnDataChangedCo()
        {
            CleanEmptyModifiers();

            // Needed to let accessories load in
            yield return Utilities.WaitForEndOfFrame;

            ModifiersFillInTransforms();

            NeedsBaselineUpdate = false;

            NewDataLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void CollectBaseline()
        {
            StartCoroutine(CollectBaselineCo());
        }

        private float? _previousAnimSpeed;
        private IEnumerator CollectBaselineCo()
        {
            do yield return Utilities.WaitForEndOfFrame;
            while (ChaControl.animBody == null);

            // Stop the animation to prevent bones from drifting while taking the measurement
            // Check if there's a speed already stored in case the previous run of this coroutine didn't finish
            if (!_previousAnimSpeed.HasValue) _previousAnimSpeed = ChaControl.animBody.speed;
            ChaControl.animBody.speed = 0;

#if KK || AI || HS2 // Only for studio
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

            yield return Utilities.WaitForEndOfFrame;

            // Ensure that the baseline is correct
            ChaControl.updateShapeFace = true;
            ChaControl.updateShapeBody = true;
            ChaControl.LateUpdateForce();

            ModifiersFillInTransforms();

            foreach (var modifier in Modifiers)
                modifier.CollectBaseline();

            _baselineKnown = true;

            yield return Utilities.WaitForEndOfFrame;

#if KK || AI || HS2 // Only for studio
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

            ChaControl.animBody.speed = _previousAnimSpeed ?? 1f;
            _previousAnimSpeed = null;
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
