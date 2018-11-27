using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using IllusionUtility.GetUtility;
using Studio;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace KKABMX.Core
{
    public class BoneController : MonoBehaviour
    {
        //private const float MissingBoneCheckInterval = 1.5f;
        //private float _missingBoneNextChecktime;

        private bool _baseLineKnown;
        private float[] _sibBodyValues;
        private float[] _sibFaceValues;

        public ChaControl ChaControl { get; private set; }
        public SortedDictionary<string, BoneModifierBody> Modifiers { get; private set; } = new SortedDictionary<string, BoneModifierBody>();

        private static readonly int CoordinateCount = Enum.GetValues(typeof(ChaFileDefine.CoordinateType)).Length;
        private readonly Dictionary<string, ModifierData[]> _coordinateModifierData = new Dictionary<string, ModifierData[]>();

        public ChaFileDefine.CoordinateType CurrentCoordinate => (ChaFileDefine.CoordinateType)_currentCoordinate;
        private int _currentCoordinate;

        public event EventHandler CurrentCoordinateChanged;

        private void ApplyAll()
        {
            foreach (var boneModifierBody in Modifiers.Values)
                boneModifierBody.Apply();
        }

        /// <summary>
        /// Get rid of the legacy config file
        /// </summary>
        public void DeleteFile()
        {
            var extDataFilePath = GetMakerExtDataFilePath();
            if (File.Exists(extDataFilePath))
                File.Delete(extDataFilePath);
        }

        private BoneModifierBody FindOrCreateModifierByBoneName(string boneName)
        {
            return Modifiers.TryGetValue(boneName, out var result) ? result : InsertAdditionalModifier(boneName);
        }

        private string GetExtDataFilePath(string baseName)
        {
            if (GetLastLoadedFile() != null)
                return GetExtDataFilePath(baseName, ChaControl.fileParam.sex);
            return "";
        }

        public static string GetExtDataFilePath(string path, int sex)
        {
            var text = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);
            if (text == string.Empty)
                text = UserData.Path + (sex != 0 ? "chara/female/" : "chara/male/");
            if (File.Exists(text + "/" + fileName))
                return text + "/" + fileName + ".bonemod.txt";
            return "";
        }

        private string GetMakerExtDataFilePath()
        {
            return GetExtDataFilePath(GetLastLoadedFile());
        }

        public BoneModifierBody InsertAdditionalModifier(string boneName)
        {
            var loopGo = GetRootTransform().FindLoop(boneName);
            if (loopGo == null)
            {
                Logger.Log(LogLevel.Debug, $"[KKABMX] Manually included bone {boneName} was not found");
                return null;
            }
            var boneModifierBody = new BoneModifierBody(Utilities.ManualBoneId, null, boneName);
            boneModifierBody.ManualTarget = loopGo.transform;
            Modifiers.Add(boneModifierBody.BoneName, boneModifierBody);
            return boneModifierBody;
        }

        private bool IsExtDataExists(string baseCharaFileName)
        {
            return File.Exists(GetExtDataFilePath(baseCharaFileName));
        }

        private bool IsMakerExtDataExists()
        {
            return IsCustomScene() && IsExtDataExists(BoneControllerMgr.Instance.LastLoadedFile);
        }

        private void LoadFromFile(string path)
        {
            ResetModifiers();

            if (Modifiers.Count == 0)
                return;

            Logger.Log(LogLevel.Info, $"[KKABMX] Load from file: {path}");

            var lines = File.ReadAllLines(path);
            DeserializeToModifiers(lines);
        }

        public void LoadFromTextData(string textData)
        {
            ResetModifiers();

            if (Modifiers.Count == 0)
                return;

            DeserializeToModifiers(textData.Split());
        }

        public void ResetModifiers()
        {
            foreach (var boneModifierBody in Modifiers.Values)
            {
                boneModifierBody.Reset();
                boneModifierBody.Clear();
            }

            var sibBody = ChaControl.GetSibBody();
            var sibFace = ChaControl.GetSibFace();

            Modifiers = BoneModifierBody.CreateListForBody(sibBody);
            BoneModifierBody.AddFaceBones(sibFace, Modifiers);
            InsertAdditionalModifiers();

            foreach (var coordinateBoneName in BoneConfiguration.CoordinateBoneNames)
                _coordinateModifierData[coordinateBoneName] = Enumerable.Range(0, CoordinateCount).Select(_ => new ModifierData()).ToArray();

            _currentCoordinate = 0;
            _baseLineKnown = false;
            _sibBodyValues = null;
            _sibFaceValues = null;
        }

        public string Serialize()
        {
            var sb = new StringBuilder();
            foreach (var key in Modifiers.Keys)
            {
                var boneModifierBody = Modifiers[key];

                void SerializeHelper(string boneName, Vector3 sclMod, float lenMod, bool forceWrite)
                {
                    if (!forceWrite && sclMod.Equals(Vector3.one) && boneModifierBody.LenMod.Equals(1f))
                        return;

                    var serializedModifier = string.Join(",", new[]
                        {
                            boneModifierBody.BoneIndex.ToString(CultureInfo.InvariantCulture),
                            boneName,
                            boneModifierBody.Enabled.ToString(CultureInfo.InvariantCulture),
                            sclMod.x.ToString(CultureInfo.InvariantCulture),
                            sclMod.y.ToString(CultureInfo.InvariantCulture),
                            sclMod.z.ToString(CultureInfo.InvariantCulture),
                            lenMod.ToString(CultureInfo.InvariantCulture)
                        });
                    sb.AppendLine(serializedModifier);
                }

                // Save legacy data even if we save coord-specific, keep it first in order
                SerializeHelper(boneModifierBody.BoneName, boneModifierBody.SclMod, boneModifierBody.LenMod, false);

                if (_coordinateModifierData.TryGetValue(key, out var data))
                {
                    for (var i = 0; i < data.Length; i++)
                    {
                        var sclMod = _currentCoordinate == i ? boneModifierBody.SclMod : data[i].SclMod;
                        var lenMod = _currentCoordinate == i ? boneModifierBody.LenMod : data[i].LenMod;
                        // Need to force write these so we don't assume we are converting from old bonemod and set something other than Vector3.one
                        SerializeHelper($"{boneModifierBody.BoneName}__{i}", sclMod, lenMod, true);
                    }
                }
            }
            return sb.ToString();
        }

        protected void LateUpdate()
        {
            try
            {
                if (BoneControllerMgr.Instance.NeedReload)
                    BoneControllerMgr.LoadFromPluginData(this, ChaControl.chaFile);

                if (!_baseLineKnown || BoneControllerMgr.Instance.NeedReload)
                {
                    StartCoroutine(CollectBaselineCo());
                    return;
                }

                /*if (_missingBoneNextChecktime <= 0f)
                {
                    foreach (var boneModifierBody in Modifiers.Values)
                    {
                        if (boneModifierBody.IsNotManual || boneModifierBody.ManualTarget != null)
                            continue;

                        var sw = Stopwatch.StartNew();
                        var loopGo = GetRootTransform().FindLoop(boneModifierBody.BoneName);
                        if (loopGo != null)
                        {
                            Logger.Log(LogLevel.Info, "Found missing bone " + boneModifierBody.BoneName);
                            boneModifierBody.ManualTarget = loopGo.transform;
                            boneModifierBody.CollectBaseline();
                        }
                        sw.Stop();

                        Logger.Log(LogLevel.Info, "Checking for missing bones " + sw.Elapsed);
                    }

                    _missingBoneNextChecktime = MissingBoneCheckInterval;
                }
                else
                {
                    _missingBoneNextChecktime -= Time.deltaTime;
                }*/

                ApplyAll();
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
            }
        }

        private IEnumerator CollectBaselineCo()
        {
            if (ChaControl.animBody == null) yield break;

            var pvCopy = ChaControl.animBody.gameObject.GetComponent<PVCopy>();
            var currentPvCopy = new bool[4];
            if (pvCopy != null)
            {
                for (var i = 0; i < 4; i++)
                {
                    currentPvCopy[i] = pvCopy[i];
                    pvCopy[i] = false;
                }
            }

            yield return new WaitForEndOfFrame();

            foreach (var boneModifierBody in Modifiers.Values)
                boneModifierBody.CollectBaseline();

            _baseLineKnown = true;

            yield return new WaitForEndOfFrame();

            if (pvCopy != null)
            {
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

            yield return null;
            enabled = false;
            yield return null;
            enabled = true;
        }

        private void DeserializeToModifiers(IEnumerable<string> lines)
        {
            const float emptyMarker = float.MinValue;
            foreach (var coordDatas in _coordinateModifierData)
            {
                for (var i = 1; i < coordDatas.Value.Length; i++)
                {
                    var modifierData = coordDatas.Value[i];
                    modifierData.SclMod.x = emptyMarker;
                    modifierData.LenMod = emptyMarker;
                }
            }

            foreach (var lineText in lines)
            {
                var trimmedText = lineText?.Trim();
                if (string.IsNullOrEmpty(trimmedText)) continue;
                try
                {
                    var splitValues = trimmedText.Split(',');

                    var boneName = splitValues[1];
                    var isEnabled = bool.Parse(splitValues[2]);
                    var x = float.Parse(splitValues[3]);
                    var y = float.Parse(splitValues[4]);
                    var z = float.Parse(splitValues[5]);

                    var lenMod = splitValues.Length > 6 ? float.Parse(splitValues[6]) : 1f;

                    // Load non-coord data as 1st coord data if that's missing
                    var coordinateId = 0;
                    if (boneName[boneName.Length - 2] == '_' && boneName[boneName.Length - 3] == '_')
                    {
                        coordinateId = boneName[boneName.Length - 1] - '0';
                        boneName = boneName.Substring(0, boneName.Length - 3);
                    }

                    var boneModifierBody = FindOrCreateModifierByBoneName(boneName);
                    if (boneModifierBody == null)
                        continue;

                    if (_coordinateModifierData.TryGetValue(boneName, out var data))
                    {
                        data[coordinateId].SclMod = new Vector3(x, y, z);
                        if (splitValues.Length > 6)
                            data[coordinateId].LenMod = lenMod;
                    }

                    // Non-coord data is always 1st, so it gets overwritten by coord data if it exists
                    if (coordinateId == 0 || coordinateId == _currentCoordinate)
                    {
                        boneModifierBody.Enabled = isEnabled;
                        boneModifierBody.SclMod.x = x;
                        boneModifierBody.SclMod.y = y;
                        boneModifierBody.SclMod.z = z;
                        boneModifierBody.LenMod = lenMod;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"[KKABMX] Failed to load line \"{trimmedText}\" - {ex.Message}");
                    Logger.Log(LogLevel.Debug, "Error details: " + ex);
                }
            }

            foreach (var coordDatas in _coordinateModifierData)
            {
                for (var i = 1; i < coordDatas.Value.Length; i++)
                {
                    if (coordDatas.Value[i].SclMod.x.Equals(emptyMarker))
                        coordDatas.Value[i] = coordDatas.Value[0];
                    if (coordDatas.Value[i].LenMod.Equals(emptyMarker))
                        coordDatas.Value[i].LenMod = coordDatas.Value[0].LenMod;
                }
            }
        }

        private IEnumerator ForceBoneUpdateAndRebaseCo()
        {
            yield return null;

            _sibBodyValues = new float[ChaControl.fileBody.shapeValueBody.Length];
            ChaControl.fileBody.shapeValueBody.CopyTo(_sibBodyValues, 0);
            _sibFaceValues = new float[ChaControl.fileFace.shapeValueFace.Length];
            ChaControl.fileFace.shapeValueFace.CopyTo(_sibFaceValues, 0);

            ChaControl.updateShapeBody = true;
            ChaControl.updateShapeFace = true;
            ChaControl.UpdateShapeFace();
            ChaControl.UpdateShapeBody();

            foreach (var boneModifierBody in Modifiers.Values)
            {
                if (boneModifierBody.BoneIndex != -1 && boneModifierBody.ScaleBone)
                    boneModifierBody.CollectBaseline();
            }

            _baseLineKnown = true;
        }

        private static string GetLastLoadedFile()
        {
            if (IsCustomScene())
                return BoneControllerMgr.Instance.LastLoadedFile;
            return null;
        }

        private Transform GetRootTransform()
        {
            return ChaControl.objBodyBone.transform;
        }

        private void InsertAdditionalModifiers()
        {
            // 0 = male, else female
            //if (chaControl.fileParam.sex != 0)

            foreach (var boneName in BoneConfiguration.AdditionalBoneNames)
                InsertAdditionalModifier(boneName);
        }

        private IEnumerator InstallModifierCo()
        {
            yield return new WaitUntil(() => ChaControl != null && ChaControl.loadEnd && ChaControl.objBodyBone != null);
            try
            {
                if (IsMakerExtDataExists())
                    LoadFromFile(GetMakerExtDataFilePath());
                else
                    BoneControllerMgr.LoadFromPluginData(this, ChaControl.chaFile);
            }
            catch (Exception value)
            {
                Logger.Log(LogLevel.Error, "[KKABMX] Unxepected error setting up character: " + ChaControl.chaFile.parameter.fullname);
                Logger.Log(LogLevel.Error, value);
            }
        }

        private bool IsBaselineChanged()
        {
            if (_sibBodyValues == null || _sibFaceValues == null)
                return true;

            var bodyLen = ChaControl.fileBody.shapeValueBody.Length;
            if (bodyLen != _sibBodyValues.Length) return true;

            for (var i = 0; i < bodyLen; i++)
            {
                if (!_sibBodyValues[i].Equals(ChaControl.fileBody.shapeValueBody[i]))
                    return true;
            }

            var faceLen = ChaControl.fileFace.shapeValueFace.Length;
            if (faceLen != _sibFaceValues.Length) return true;

            for (var j = 0; j < faceLen; j++)
            {
                if (!_sibFaceValues[j].Equals(ChaControl.fileFace.shapeValueFace[j]))
                    return true;
            }

            return false;
        }

        private static bool IsCustomScene()
        {
            return BoneControllerMgr.Instance.InsideMaker;
        }

        private void Awake()
        {
            BoneControllerMgr.BoneControllers.Add(this);
        }

        private void Start()
        {
            ChaControl = GetComponent<ChaControl>();
            StartCoroutine(InstallModifierCo());
        }

        private void Update()
        {
            if (ChaControl.fileStatus.coordinateType != _currentCoordinate)
            {
                var previousId = _currentCoordinate;
                _currentCoordinate = ChaControl.fileStatus.coordinateType;
                OnCurrentCoordinateChanged(_currentCoordinate, previousId);
            }

            if (_baseLineKnown && IsBaselineChanged())
                StartCoroutine(ForceBoneUpdateAndRebaseCo());
        }

        private void OnCurrentCoordinateChanged(int newId, int previousId)
        {
            foreach (var coordData in _coordinateModifierData)
            {
                var modifier = FindOrCreateModifierByBoneName(coordData.Key);
                if (modifier != null)
                {
                    var previousModifierData = coordData.Value[previousId];
                    previousModifierData.SclMod = modifier.SclMod;
                    previousModifierData.LenMod = modifier.LenMod;

                    var newModifierData = coordData.Value[newId];
                    modifier.SclMod = newModifierData.SclMod;
                    modifier.LenMod = newModifierData.LenMod;
                }
            }

            CurrentCoordinateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDestroy()
        {
            BoneControllerMgr.BoneControllers.Remove(this);
        }

        private sealed class ModifierData
        {
            public Vector3 SclMod;
            public float LenMod;

            public ModifierData() : this(Vector3.one, 1) { }

            public ModifierData(Vector3 sclMod, float lenMod)
            {
                SclMod = sclMod;
                LenMod = lenMod;
            }
        }
    }
}
