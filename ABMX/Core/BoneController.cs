using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Chara;
using MessagePack;
using UniRx;
using UnityEngine;
using Logger = KKABMX.Core.KKABMX_Core;

#if KK
using ExtensibleSaveFormat;
using CoordinateType = ChaFileDefine.CoordinateType;
#elif EC
using EC.Core.ExtensibleSaveFormat;
using CoordinateType = KoikatsuCharaFile.ChaFileDefine.CoordinateType;
#endif

namespace KKABMX.Core
{
    // Run the LateUpdate after everything else
    [DefaultExecutionOrder(10000)]
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

        public event EventHandler NewDataLoaded;

#if EC
        public BehaviorSubject<CoordinateType> CurrentCoordinate = new BehaviorSubject<CoordinateType>(CoordinateType.School01);
#endif

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

            var newEffects = effect.GetAffectedBones(this).Except(Modifiers.Select(x => x.BoneName)).ToList();
            if (newEffects.Any())
            {
                foreach (var boneName in newEffects)
                    Modifiers.Add(new BoneModifier(boneName));
                ModifiersFillInTransforms();
                NeedsBaselineUpdate = true;
            }
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
                    Logger.Log(LogLevel.Error, "[KKABMX] Failed to load coordinate extended data - " + ex);
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
        }

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

            if (!maintainState)
            {
                Modifiers = null;

                var data = GetExtendedData();
                if (data != null)
                {
                    try
                    {
                        switch (data.version)
                        {
                            case 2:
                                Modifiers = LZ4MessagePackSerializer.Deserialize<List<BoneModifier>>((byte[])data.data[ExtDataBoneDataKey]);
                                break;

                            case 1:
                                Logger.Log(LogLevel.Debug, $"[KKABMX] Loading legacy embedded ABM data from card: {ChaFileControl.parameter?.fullname}");
                                Modifiers = OldDataConverter.MigrateOldExtData(data);
                                break;

                            default:
                                throw new NotSupportedException($"Save version {data.version} is not supported");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "[KKABMX] Failed to load extended data - " + ex);
                    }
                }
            }

            if (Modifiers == null)
                Modifiers = new List<BoneModifier>();

            StartCoroutine(OnDataChangedCo());
        }

        protected override void Start()
        {
            base.Start();
            CurrentCoordinate.Subscribe(_ => StartCoroutine(OnDataChangedCo()));
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

                foreach (var modifier in Modifiers)
                {
                    var additionalModifiers = _additionalBoneEffects
                        .Select(x => x.GetEffect(modifier.BoneName, this, CurrentCoordinate.Value))
                        .Where(x => x != null)
                        .ToList();

                    modifier.Apply(CurrentCoordinate.Value, additionalModifiers);
                }
            }
            else if (_baselineKnown == false)
            {
                _baselineKnown = null;
                CollectBaseline();
            }

            NeedsBaselineUpdate = false;
        }

        private IEnumerator OnDataChangedCo()
        {
            foreach (var modifier in Modifiers.Where(x => x.IsEmpty()).ToList())
            {
                modifier.Reset();
                Modifiers.Remove(modifier);
            }

            // Add any modifiers that will be used by the AdditionalBoneEffects if they don't already exist
            foreach (var boneName in _additionalBoneEffects.SelectMany(x => x.GetAffectedBones(this)).Except(Modifiers.Select(x => x.BoneName)))
            {
                Modifiers.Add(new BoneModifier(boneName));
            }

            // Needed to let accessories load in
            yield return new WaitForEndOfFrame();

            ModifiersFillInTransforms();

            NeedsBaselineUpdate = true;

            NewDataLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void CollectBaseline()
        {
            if (ChaControl.animBody == null) return;

            foreach (var modifier in Modifiers)
                modifier.CollectBaseline();

            _baselineKnown = true;

#if KK
            StartCoroutine(CollectBaselineCo());
#endif
        }

#if KK
        private IEnumerator CollectBaselineCo()
        {
            var pvCopy = ChaControl.animBody.gameObject.GetComponent<Studio.PVCopy>();
            var currentPvCopy = new bool[4];
            if (pvCopy != null)
            {
                for (var i = 0; i < 4; i++)
                {
                    currentPvCopy[i] = pvCopy[i];
                    pvCopy[i] = false;
                }

                yield return new WaitForEndOfFrame();

                var array = pvCopy.GetPvArray();
                var array2 = pvCopy.GetBoneArray();
                for (var j = 0; j < 4; j++)
                {
                    if (currentPvCopy[j] && array2[j] && array[j])
                    {
                        array[j].transform.localScale = array2[j].transform.localScale;
                        array[j].transform.position = array2[j].transform.position;
                        array[j].transform.rotation = array2[j].transform.rotation;
                    }
                }
            }
        }
#endif

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
