using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IllusionUtility.GetUtility;
using KKAPI;
using KKAPI.Chara;
using MessagePack;
using UnityEngine;
using ExtensibleSaveFormat;
using KKAPI.Maker;
using KKAPI.Utilities;
using UniRx;
#if AI || HS2
using AIChara;
#endif

namespace KKABMX.Core
{
#if KK || KKS
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

        /// <inheritdoc cref="BoneFinder"/>
        public BoneFinder BoneSearcher { get; private set; }

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
        [Obsolete("No longer works for changing modifiers, every get returns a new copy of the list and is expensive. Use AddModifier, GetModifier and GetAllModifiers instead.", true)]
        public List<BoneModifier> Modifiers => GetAllModifiers().ToList();

        /// <summary>
        /// Container of all bonemod data. Do not hold references to this, always get the current one!
        /// </summary>
        internal SortedDictionary<BoneLocation, List<BoneModifier>> ModifierDict { get; private set; } = new SortedDictionary<BoneLocation, List<BoneModifier>>();
        internal Dictionary<BoneLocation, List<BoneModifier>> ModifierDictBackup { get; private set; } = new Dictionary<BoneLocation, List<BoneModifier>>();

        /// <summary>
        /// Additional effects that other plugins can apply to a character
        /// </summary>
        public IEnumerable<BoneEffect> AdditionalBoneEffects => _additionalBoneEffects;
        private readonly List<BoneEffect> _additionalBoneEffects = new List<BoneEffect>();

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
        /// Get all bone modifiers currently present (some might be empty).
        /// </summary>
        public IEnumerable<BoneModifier> GetAllModifiers() => ModifierDict.SelectMany(x => x.Value);
        /// <summary>
        /// Get all bone modifiers currently present for a given location (some might be empty).
        /// If the location doesn't have any modifiers or doesn't exist, 0 items are returned.
        /// </summary>
        public IEnumerable<BoneModifier> GetAllModifiers(BoneLocation location)
        {
            ModifierDict.TryGetValue(location, out var modifiers);
            return modifiers ?? Enumerable.Empty<BoneModifier>();
        }

        /// <summary>
        /// Add a new bone modifier. Make sure that it doesn't exist yet!
        /// </summary>
        public void AddModifier(BoneModifier bone)
        {
            if (bone == null) throw new ArgumentNullException(nameof(bone));
            var modifiers = GetModifierListForLocation(bone.BoneLocation, true, ModifierDict);
            modifiers.Add(bone);
            BoneSearcher.AssignBone(bone);
            bone.CollectBaseline();
        }

        private static List<BoneModifier> GetModifierListForLocation(BoneLocation location, bool create, IDictionary<BoneLocation, List<BoneModifier>> modifierDict)
        {
            if (location <= BoneLocation.Unknown)
                location = BoneLocation.BodyTop;

            if (!modifierDict.TryGetValue(location, out var modifiers))
            {
                if (!create) return null;
                modifiers = new List<BoneModifier>();
                modifierDict[location] = modifiers;
            }

            return modifiers;
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
        /// Remove specified bone effect. If effect wasn't added, this does nothing.
        /// </summary>
        public void RemoveBoneEffect(BoneEffect effect)
        {
            _additionalBoneEffects.Remove(effect);
        }

        /// <inheritdoc cref="GetModifier(string,BoneLocation)"/>
        [Obsolete]
        public BoneModifier GetModifier(string boneName) => GetModifier(boneName, BoneLocation.Unknown);

        /// <summary>
        /// Get a modifier if it exists.
        /// </summary>
        /// <param name="boneName">Name of the bone that the modifier targets</param>
        /// <param name="location">Where the bone is located</param>
        public BoneModifier GetModifier(string boneName, BoneLocation location)
        {
            return GetModifierInt(boneName, location, GetModifierListForLocation(location, false, ModifierDict));
        }

        /// <summary>
        /// Get a modifier. If it doesn't exist, create a new empty one.
        /// </summary>
        /// <param name="boneName">Name of the bone that the modifier targets</param>
        /// <param name="location">Where the bone is located</param>
        public BoneModifier GetOrAddModifier(string boneName, BoneLocation location)
        {
            var m = GetModifier(boneName, location);
            if (m == null)
            {
                m = new BoneModifier(boneName, location);
                AddModifier(m);
            }
            return m;
        }

        private static BoneModifier GetModifierInt(string boneName, BoneLocation location, List<BoneModifier> modifierList)
        {
            if (boneName == null) throw new ArgumentNullException(nameof(boneName));
            if (modifierList == null) return null;

            for (var i = 0; i < modifierList.Count; i++)
            {
                var x = modifierList[i];
                if ((location == BoneLocation.Unknown || location == x.BoneLocation) && x.BoneName == boneName) return x;
            }

            return null;
        }

        /// <summary>
        /// Removes the specified modifier and resets the affected bone to its original state
        /// </summary>
        /// <param name="modifier">Modifier added to this controller</param>
        public void RemoveModifier(BoneModifier modifier)
        {
            if (modifier == null) throw new ArgumentNullException(nameof(modifier));
            modifier.Reset();
            GetModifierListForLocation(modifier.BoneLocation, false, ModifierDict)?.Remove(modifier);

            ChaControl.updateShapeFace = true;
            ChaControl.updateShapeBody = true;
        }

        /// <summary>
        /// Get all transform names under the character object that could be bones (excludes accessories).
        /// Warning: Expensive to run, ToList the result and cache it if you want to reuse it!
        /// </summary>
        public IEnumerable<string> GetAllPossibleBoneNames() => GetAllPossibleBoneNames(ChaControl.objBodyBone);

        /// <summary>
        /// Get all transform names under the rootObject that could be bones (could be from BodyTop or objAccessory, BodyTop excludes accessories).
        /// Warning: Expensive to run, ToList the result and cache it if you want to reuse it!
        /// </summary>
        public IEnumerable<string> GetAllPossibleBoneNames(GameObject rootObject)
        {
            return BoneSearcher.CreateBoneDic(rootObject).Keys
#if AI || HS2
                                .Where(x => !x.StartsWith("f_t_", StringComparison.Ordinal) && !x.StartsWith("f_pv_", StringComparison.Ordinal) && !x.StartsWith("f_k_", StringComparison.Ordinal));
#elif KK || EC || KKS
                                .Where(x => !x.StartsWith("cf_t_", StringComparison.Ordinal) && !x.StartsWith("cf_pv_", StringComparison.Ordinal));
#endif
        }

#if !EC && !AI && !HS2 //No coordinate saving in AIS
        /// <inheritdoc />
        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (maintainState) return;

            var currentCoord = CurrentCoordinate.Value;

            // Clear previous data for this coordinate from coord specific modifiers
            foreach (var modifier in ModifierDict.SelectMany(x => x.Value).Where(x => x.IsCoordinateSpecific()))
                modifier.GetModifier(currentCoord).Clear();

            var data = GetCoordinateExtendedData(coordinate);
            var modifiers = ReadCoordModifiers(data);
            foreach (var modifier in modifiers)
            {
                // Add any missing modifiers
                var target = GetOrAddModifier(modifier.BoneName, modifier.BoneLocation);
                target.MakeCoordinateSpecific(ChaFileControl.coordinate.Length);
                target.CoordinateModifiers[(int)currentCoord] = modifier.CoordinateModifiers[0];
            }

            StartCoroutine(OnDataChangedCo());
        }

        internal static List<BoneModifier> ReadCoordModifiers(PluginData data)
        {
            if (data != null)
            {
                try
                {
                    switch (data.version)
                    {
                        case 3:
                            return LZ4MessagePackSerializer.Deserialize<List<BoneModifier>>((byte[])data.data[ExtDataBoneDataKey]);

                        case 2:
                            return LZ4MessagePackSerializer.Deserialize<Dictionary<string, BoneModifierData>>((byte[])data.data[ExtDataBoneDataKey])
                                                           .Select(x => new BoneModifier(x.Key, BoneLocation.Unknown, new[] { x.Value }))
                                                           .ToList();
                        default:
                            throw new NotSupportedException($"Save version {data.version} is not supported");
                    }
                }
                catch (Exception ex)
                {
                    KKABMX_Core.Logger.LogError("[KKABMX] Failed to load coordinate extended data - " + ex);
                }
            }

            return new List<BoneModifier>();
        }

        /// <inheritdoc />
        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            var currentCoord = CurrentCoordinate.Value;
            var toSave = ModifierDict.SelectMany(x => x.Value)
                                     // This can remove accessory modifiers if they are for a different coordinate, but this method should only ever run for the current coord
                                     .Where(x => !x.IsEmpty() && x.IsCoordinateSpecific() && x.BoneTransform != null)
                                     .Select(x => new BoneModifier(x.BoneName, x.BoneLocation, new[] { x.GetModifier(currentCoord) }))
                                     .ToList();

            if (toSave.Count == 0)
                SetCoordinateExtendedData(coordinate, null);
            else
            {
                var pluginData = new PluginData { version = 3 };
                pluginData.data.Add(ExtDataBoneDataKey, LZ4MessagePackSerializer.Serialize(toSave));
                SetCoordinateExtendedData(coordinate, pluginData);
            }
        }
#endif

        /// <inheritdoc />
        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            var data = SaveModifiers(ModifierDict.SelectMany(x => x.Value));
            SetExtendedData(data);

            MakeModifiersBackup();
        }

        internal void RevertChanges()
        {
            foreach (var modifier in ModifierDict.SelectMany(x => x.Value))
                modifier.Reset();

            ModifierDict = new SortedDictionary<BoneLocation, List<BoneModifier>>(ModifierDictBackup.ToDictionary(x => x.Key, x => x.Value.Select(y => y.Clone()).ToList()));

            NeedsFullRefresh = true;
        }

        internal void RevertChangesModifier(string boneName, BoneLocation location)
        {
            var current = GetModifier(boneName, location);
            var orig = GetModifierBackup(boneName, location);
            if (current != null) RemoveModifier(current);
            if (orig != null) AddModifier(orig.Clone());
        }

        internal BoneModifier GetModifierBackup(string boneName, BoneLocation location)
        {
            return GetModifierInt(boneName, location, GetModifierListForLocation(location, false, ModifierDictBackup));
        }

        private void MakeModifiersBackup()
        {
            ModifierDictBackup = ModifierDict.ToDictionary(x => x.Key, x => x.Value.Select(y => y.Clone()).ToList());
        }

        /// <inheritdoc />
        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            foreach (var modifier in ModifierDict.SelectMany(x => x.Value))
                modifier.Reset();

            // Stop baseline collection if it's running
            StopAllCoroutines();
            _baselineKnown = false;

            var loadClothes = MakerAPI.GetCharacterLoadFlags()?.Clothes != false;
            var loadBody = GUI.KKABMX_GUI.LoadBody;
            var loadFace = GUI.KKABMX_GUI.LoadFace;
            if (!maintainState && (loadBody || loadFace || loadClothes))
            {
                var data = GetExtendedData();
                var newModifiers = ReadModifiers(data).GroupBy(x => x.BoneLocation).ToDictionary(x => x.Key, x => x.ToList());

                if (loadBody && loadFace && loadClothes)
                {
                    ModifierDict = new SortedDictionary<BoneLocation, List<BoneModifier>>(newModifiers);
                }
                else
                {
                    if (loadBody && loadFace)
                    {
                        ModifierDict.Remove(BoneLocation.BodyTop);
                        if (newModifiers.TryGetValue(BoneLocation.BodyTop, out var newList))
                            ModifierDict.Add(BoneLocation.BodyTop, newList);

                        ModifierDict.Remove(BoneLocation.Unknown);
                        if (newModifiers.TryGetValue(BoneLocation.Unknown, out newList))
                            ModifierDict.Add(BoneLocation.Unknown, newList);
                    }
                    else if (loadBody || loadFace)
                    {
                        void LoadSelectedModifiers(BoneLocation targetBoneLocation, HashSet<string> toReplace)
                        {
                            if (!ModifierDict.TryGetValue(targetBoneLocation, out var oldList))
                            {
                                oldList = new List<BoneModifier>();
                                ModifierDict.Add(targetBoneLocation, oldList);
                            }
                            else
                            {
                                oldList.RemoveAll(x => toReplace.Contains(x.BoneName));
                            }

                            if (newModifiers.TryGetValue(targetBoneLocation, out var newList))
                                oldList.AddRange(newList.Where(x => toReplace.Contains(x.BoneName)));
                            if (oldList.Count == 0)
                                ModifierDict.Remove(targetBoneLocation);
                        }
                        var headRoot = ChaControl.objHeadBone;
                        var headBones = new HashSet<string>(headRoot.GetComponentsInChildren<Transform>().Select(x => x.name));
                        headBones.Add(headRoot.name);

                        if (loadFace)
                        {
                            LoadSelectedModifiers(BoneLocation.Unknown, headBones);
                            LoadSelectedModifiers(BoneLocation.BodyTop, headBones);
                        }
                        else
                        {
                            var bodyBones = new HashSet<string>(transform.FindLoop("BodyTop").GetComponentsInChildren<Transform>().Select(x => x.name).Except(headBones));
                            LoadSelectedModifiers(BoneLocation.Unknown, bodyBones);
                            LoadSelectedModifiers(BoneLocation.BodyTop, bodyBones);
                        }
                    }

                    if (loadClothes)
                    {
                        foreach (var boneLocation in ModifierDict.Keys.Where(x => x >= BoneLocation.Accessory).ToList())
                            ModifierDict.Remove(boneLocation);

                        foreach (var newModifierList in newModifiers)
                        {
                            if (newModifierList.Key >= BoneLocation.Accessory)
                                ModifierDict.Add(newModifierList.Key, newModifierList.Value);
                        }
                    }
                }
            }

            StartCoroutine(OnDataChangedCo());
        }

        internal static List<BoneModifier> ReadModifiers(PluginData data)
        {
            if (data != null)
            {
                try
                {
                    switch (data.version)
                    {
                        case 2:
                            return LZ4MessagePackSerializer.Deserialize<List<BoneModifier>>((byte[])data.data[ExtDataBoneDataKey]);
#if KK || EC || KKS
                        case 1:
                            KKABMX_Core.Logger.LogDebug("[KKABMX] Loading legacy embedded ABM data");
                            return OldDataConverter.MigrateOldExtData(data);
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
            return new List<BoneModifier>();
        }

        internal static PluginData SaveModifiers(IEnumerable<BoneModifier> modifiers)
        {
            // Accessory modifiers might not have a BoneTransform if they are for an accessory in a non-current coordinate
            var toSave = modifiers.Where(x => !x.IsEmpty() && (x.BoneLocation >= BoneLocation.Accessory || x.BoneTransform != null)).ToList();

            if (toSave.Count == 0)
                return null;

            var data = new PluginData { version = 2 };
            data.data.Add(ExtDataBoneDataKey, LZ4MessagePackSerializer.Serialize(toSave));
            return data;
        }

        /// <inheritdoc />
        protected override void Start()
        {
            BoneSearcher = new BoneFinder(ChaControl);
            base.Start();
            CurrentCoordinate.Subscribe(_ => NeedsBaselineUpdate = true);
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
                    StartCoroutine(UpdateBaseline());

                ApplyEffects();
            }
            else if (_baselineKnown == false)
            {
                _baselineKnown = null;
                CollectBaseline();
            }
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
                        var modifier = GetOrAddModifier(affectedBone, BoneLocation.BodyTop); //todo allow targeting accessories?
                        if (!_effectsToUpdate.TryGetValue(modifier, out var list))
                        {
                            list = new List<BoneModifierData>();
                            _effectsToUpdate[modifier] = list;
                        }
                        list.Add(effect);
                    }
                }
            }

            var anyUnknown = false;
            // Modifiers must be sorted by location key
            foreach (var kvp in ModifierDict)
            {
                var boneLocation = kvp.Key;
                var boneModifiers = kvp.Value;

                ApplyEffectsToLocation(boneLocation, boneModifiers);

                if (boneLocation == BoneLocation.Unknown)
                {
                    anyUnknown = true;
                }
                else if (anyUnknown || boneLocation == BoneLocation.BodyTop)
                {
                    FixBustGravity();
                }
            }
        }

        /// <summary>
        /// Apply all modifiers to the specified location. Location set on modifiers is ignored.
        /// This is patched by C2A, needs to stay unless both plugins are updated.
        /// </summary>
        public void ApplyEffectsToLocation(BoneLocation boneLocation, List<BoneModifier> boneModifiers)
        {
            foreach (var modifier in boneModifiers)
            {
                HandleDynamicBoneModifiers(modifier);

                List<BoneModifierData> extraEffects = null;
                if (boneLocation == BoneLocation.Unknown || boneLocation == BoneLocation.BodyTop)
                    _effectsToUpdate.TryGetValue(modifier, out extraEffects);

                modifier.Apply(CurrentCoordinate.Value, extraEffects);
            }
        }

        private void FixBustGravity()
        {
            // Fix some bust physics issues
            // bug - causes gravity issues on its own
            ChaControl.UpdateBustGravity();
        }

        private IEnumerator OnDataChangedCo()
        {
            CleanEmptyModifiers();

            // Needed to let accessories load in
            yield return CoroutineUtils.WaitForEndOfFrame;

            ModifiersFillInTransforms();

            MakeModifiersBackup();

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
            do yield return CoroutineUtils.WaitForEndOfFrame;
            while (ChaControl.animBody == null);

            // Stop the animation to prevent bones from drifting while taking the measurement
            // Check if there's a speed already stored in case the previous run of this coroutine didn't finish
            if (!_previousAnimSpeed.HasValue) _previousAnimSpeed = ChaControl.animBody.speed;
            ChaControl.animBody.speed = 0;

#if KK || KKS || AI || HS2 // Only for studio
            var pvCopy = ChaControl.animBody.gameObject.GetComponent<Studio.PVCopy>();
            bool[] currentPvCopy = null;
            if (pvCopy != null)
            {
                var pvCount = pvCopy.pv.Length;
                currentPvCopy = new bool[pvCount];
                for (var i = 0; i < currentPvCopy.Length; i++)
                {
                    currentPvCopy[i] = pvCopy[i];
                    pvCopy[i] = false;
                }
            }
#endif

            yield return CoroutineUtils.WaitForEndOfFrame;

            // Ensure that the baseline is correct
            ChaControl.updateShapeFace = true;
            ChaControl.updateShapeBody = true;
            ChaControl.LateUpdateForce();

            ModifiersFillInTransforms();

            foreach (var modifier in ModifierDict.Values.SelectMany(x => x))
                modifier.CollectBaseline();

            _baselineKnown = true;

            yield return CoroutineUtils.WaitForEndOfFrame;

#if KK || KKS || AI || HS2 // Only for studio
            if (pvCopy != null)
            {
                var array = pvCopy.pv;
                var array2 = pvCopy.bone;
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

        private List<BoneModifier> _partialBaselineUpdateTargets;
        /// <summary>
        /// Partial baseline update.
        /// Needed mainly to prevent vanilla sliders in chara maker from being overriden by bone modifiers.
        /// </summary>
        private IEnumerator UpdateBaseline()
        {
            if (_partialBaselineUpdateTargets == null)
            {
                // Run after DynamicBones finish their late updates
                yield return new WaitForEndOfFrame();

                var distSrc = ChaControl.sibFace.dictDst;
                var distSrc2 = ChaControl.sibBody.dictDst;
                var affectedBones = new HashSet<Transform>(distSrc.Concat(distSrc2).Select(x => x.Value.trfBone));
                _partialBaselineUpdateTargets = ModifierDict.Values.SelectMany(x => x).Where(x => affectedBones.Contains(x.BoneTransform)).ToList();

                // Prevent some scales from being added to the baseline, mostly skirt scale
                foreach (var boneModifier in _partialBaselineUpdateTargets)
                {
                    boneModifier.Reset();
                    // Clear baseline so the modifier doesn't get applied anymore
                    boneModifier.ClearBaseline();
                }

                ModifiersFillInTransforms(_partialBaselineUpdateTargets);

                NeedsBaselineUpdate = true;
            }
            else
            {
                // This runs on next LateUpdate, before DynamicBones
                // Force the game to update all body bones to make sure we are getting the right baseline
                ChaControl.UpdateShapeFace();
                ChaControl.UpdateShapeBody();
                foreach (var boneModifier in _partialBaselineUpdateTargets)
                    boneModifier.CollectBaseline();

                _partialBaselineUpdateTargets = null;
                NeedsBaselineUpdate = false;
            }
        }

        private void ModifiersFillInTransforms(ICollection<BoneModifier> updateTargets = null)
        {
            if (ModifierDict.Count == 0) return;
            var dictRecalcNeeded = false;
            foreach (var modifiers in ModifierDict.Values)
            {
                foreach (var modifier in modifiers)
                {
                    if (modifier.BoneTransform != null)
                    {
                        continue;
                    }
                    else if (BoneSearcher.AssignBone(modifier))
                    {
                        dictRecalcNeeded = true;
                        updateTargets?.Add(modifier);
                    }
                    else
                    {
                        KKABMX_Core.Logger.Log(modifier.BoneLocation >= BoneLocation.Accessory ? BepInEx.Logging.LogLevel.Info : BepInEx.Logging.LogLevel.Warning,
                                               $"Could not find bone [{modifier.BoneName}] in location [{modifier.BoneLocation}] - modifier will be ignored");
                    }
                }
            }

            if (dictRecalcNeeded)
                ModifierDict = new SortedDictionary<BoneLocation, List<BoneModifier>>(ModifierDict.SelectMany(x => x.Value).GroupBy(x => x.BoneLocation).ToDictionary(x => x.Key, x => x.ToList()));
        }

        internal void CleanEmptyModifiers()
        {
            foreach (var kvp in ModifierDict.ToList())
            {
                kvp.Value.RemoveAll(modifier =>
                {
                    if (modifier.IsEmpty())
                    {
                        modifier.Reset();
                        return true;
                    }

                    return false;
                });
                if (kvp.Value.Count == 0)
                    ModifierDict.Remove(kvp.Key);
            }
        }

        /// <summary>
        /// Force reset baseline of bones affected by dynamicbones
        /// to avoid overwriting dynamicbone animations
        /// </summary>
        private static void HandleDynamicBoneModifiers(BoneModifier modifier)
        {
            // Skip non-body modifiers to speed up the check and avoid affecting accessories
            if (modifier.BoneLocation > BoneLocation.BodyTop) return;

            var boneName = modifier.BoneName;
#if KK || KKS || EC
            if (boneName.StartsWith("cf_d_sk_", StringComparison.Ordinal) ||
                boneName.StartsWith("cf_j_bust0", StringComparison.Ordinal) ||
                boneName.StartsWith("cf_d_siri01_", StringComparison.Ordinal) ||
                boneName.StartsWith("cf_j_siri_", StringComparison.Ordinal))
#elif AI || HS2
            if (boneName.StartsWith("cf_J_SiriDam", StringComparison.Ordinal) ||
                boneName.StartsWith("cf_J_Mune00", StringComparison.Ordinal))
#else
                todo fix
#endif
            {
                modifier.Reset();
                modifier.CollectBaseline();
            }
        }
    }

    /// <summary>
    /// Helper class for quickly getting a list of bones for specific parts of a character.
    /// Unlike FindAssist it handles accessories and parented characters (in Studio) by excluding them from found bones.
    /// </summary>
    public class BoneFinder
    {
        /// <summary>
        /// Create a dictionary of all bones and their names under the specified root object (itself included).
        /// Accessories and other characters are not included (the search stops at them).
        /// </summary>
        public Dictionary<string, GameObject> CreateBoneDic(GameObject rootObject)
        {
            KKABMX_Core.Logger.LogDebug($"Creating bone dictionary for char={_ctrl.name} rootObj={rootObject}");
            var d = new Dictionary<string, GameObject>();
            FindAll(rootObject.transform, d, _ctrl.objAccessory.Where(x => x != null).Select(x => x.transform).ToArray());
            return d;
        }

        private static void FindAll(Transform transform, Dictionary<string, GameObject> dictObjName, Transform[] excludeTransforms)
        {
            if (!dictObjName.ContainsKey(transform.name))
                dictObjName[transform.name] = transform.gameObject;

            for (var i = 0; i < transform.childCount; i++)
            {
                var childTransform = transform.GetChild(i);
                var trName = childTransform.name;
                // Exclude parented characters (in Studio) and accessories/other
                if (!trName.StartsWith("chaF_") && !trName.StartsWith("chaM_") && Array.IndexOf(excludeTransforms, childTransform) < 0)
                    FindAll(childTransform, dictObjName, excludeTransforms);
            }
        }

        private void PurgeDestroyed()
        {
            foreach (var nullGo in _lookup.Keys.Where(x => x == null).ToList()) _lookup.Remove(nullGo);
        }

        /// <summary>
        /// Find bone of a given name in the specified location.
        /// If location is Unknown, all locations will be searched and location value will be replaced with location where it was found (if it was found).
        /// </summary>
        /// <param name="name">Name of the bone to search for</param>
        /// <param name="location">Where to search for the bone. If Unknown, the value is replaced by the location the bone was found in if the bone was found.</param>
        public GameObject FindBone(string name, ref BoneLocation location)
        {
            if (location == BoneLocation.BodyTop)
            {
                return FindBone(name, _ctrl.objBodyBone);
            }

            if (location >= BoneLocation.Accessory)
            {
                var accId = location - BoneLocation.Accessory;
                var rootObj = _ctrl.objAccessory.SafeGet(accId);
                return rootObj != null ? FindBone(name, rootObj) : null;
            }

            // Handle unknown locations by looking everywhere. If the bone is found, update the location
            var bone = FindBone(name, _ctrl.objBodyBone);
            if (bone != null)
            {
                location = BoneLocation.BodyTop;
                return bone;
            }

            for (var index = 0; index < _ctrl.objAccessory.Length; index++)
            {
                var accObj = _ctrl.objAccessory[index];
                if (accObj != null)
                {
                    var accBone = FindBone(name, accObj, true);
                    if (accBone != null)
                    {
                        location = BoneLocation.Accessory + index;
                        return accBone;
                    }
                }
            }
            return null;
        }

        private GameObject FindBone(string name, GameObject rootObject, bool noRetry = false)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (rootObject == null) throw new ArgumentNullException(nameof(rootObject));

            var recreated = false;
            if (!_lookup.TryGetValue(rootObject, out var boneDic))
            {
                PurgeDestroyed();
                boneDic = CreateBoneDic(rootObject);
                recreated = true;
                _lookup[rootObject] = boneDic;
            }

            boneDic.TryGetValue(name, out var boneObj);
            if (boneObj == null && !recreated && !noRetry)
            {
                PurgeDestroyed();
                boneDic = CreateBoneDic(rootObject);
                _lookup[rootObject] = boneDic;
                boneDic.TryGetValue(name, out boneObj);
            }

            return boneObj;
        }

        /// <summary>
        /// Get a dictionary of all bones and their names in a given location.
        /// </summary>
        public IDictionary<string, GameObject> GetAllBones(BoneLocation location)
        {
            GameObject rootObject = null;
            if (location == BoneLocation.BodyTop)
            {
                rootObject = _ctrl.objBodyBone;
            }

            if (location >= BoneLocation.Accessory)
            {
                var accId = location - BoneLocation.Accessory;
                rootObject = _ctrl.objAccessory.SafeGet(accId);
            }

            if (rootObject == null) return null;

            if (!_lookup.TryGetValue(rootObject, out var boneDic))
            {
                PurgeDestroyed();
                boneDic = CreateBoneDic(rootObject);
                _lookup[rootObject] = boneDic;
            }

            return boneDic.ToReadOnlyDictionary();
        }

        /// <summary>
        /// Create a new instance for a given character.
        /// </summary>
        public BoneFinder(ChaControl ctrl)
        {
            _ctrl = ctrl;
            _lookup = new Dictionary<GameObject, Dictionary<string, GameObject>>();
        }

        private readonly Dictionary<GameObject, Dictionary<string, GameObject>> _lookup; //todo switching accs doesnt update?
        private readonly ChaControl _ctrl;

        /// <summary>
        /// Try to find and assign target bone to a bone modifier. Returns true if successful.
        /// </summary>
        public bool AssignBone(BoneModifier modifier)
        {
            var loc = modifier.BoneLocation;
            var bone = FindBone(modifier.BoneName, ref loc);
            var boneFound = bone != null;
            modifier.BoneTransform = boneFound ? bone.transform : null;
            modifier.BoneLocation = loc;
            return boneFound;
        }
    }
}
