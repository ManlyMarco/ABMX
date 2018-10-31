using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx.Logging;
using ChaCustom;
using IllusionUtility.GetUtility;
using Studio;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace KKABMPlugin
{
    public class BoneController : MonoBehaviour
    {
        private const string EDIT_DEFAULT_FILE_NAME = "ill_default_female.png";
        private const float MISSING_BONE_CHECK_INTERVAL = 1.5f;

        private static readonly PropertyInfo f_sibBody = typeof(ChaControl).GetProperty("sibBody",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
        private static readonly PropertyInfo f_sibFace = typeof(ChaControl).GetProperty("sibFace",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
        private static readonly FieldInfo f_PvCopy_bone = typeof(PVCopy).GetField("bone",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo f_PvCopy_pv = typeof(PVCopy).GetField("pv",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private bool baseLineKnown;


        public ChaControl chaControl;


        public string fileToLoad;


        private bool isCustomScene;


        //private long lastLoadedFileTimestamp = -1L;


        //private string lastLoadedPath;


        private float missingBoneNextChecktime;


        public SortedDictionary<string, BoneModifierBody> modifiers = new SortedDictionary<string, BoneModifierBody>();


        private float[] sibBodyValues;


        private float[] sibFaceValues;

        private void Start()
        {
            chaControl = GetComponent<ChaControl>();
            StartCoroutine(InstallModifierCo());
            //StartCoroutine(WatchLoadedFileUpdateCo());
            isCustomScene = Singleton<CustomBase>.Instance != null;
            //BoneControllerMgr.Instance.RegisterBoneController(this);
        }

        private void OnDestroy()
        {
            //BoneControllerMgr.Instance.boneControllers.Remove(this);
        }

        public void ClearModifiers()
        {
            if (modifiers != null)
            {
                foreach (var boneModifierBody in modifiers.Values)
                {
                    boneModifierBody.Reset();
                    boneModifierBody.Clear();
                }
            }

            var sibBody = f_sibBody.GetValue(chaControl, null) as ShapeInfoBase;
            var sibFace = f_sibFace.GetValue(chaControl, null) as ShapeInfoBase;

            modifiers = BoneModifierBody.CreateListForBody(sibBody);
            BoneModifierBody.AddFaceBones(sibFace, modifiers);
            InsertAdditionalModifiers();

            baseLineKnown = false;
            sibBodyValues = null;
            sibFaceValues = null;
            //lastLoadedPath = null;
            //lastLoadedFileTimestamp = -1L;
        }

        private IEnumerator InstallModifierCo()
        {
            yield return new WaitUntil(() => chaControl != null && chaControl.loadEnd && chaControl.objBodyBone != null);
            try
            {
                ClearModifiers();
                if (IsExtDataExists())
                {
                    LoadFromFile();
                }
                else if (fileToLoad != null)
                {
                    if (File.Exists(fileToLoad))
                        LoadFromFile(fileToLoad);
                    fileToLoad = null;
                }
                else
                {
                    if (isCustomScene)
                    {
                        BoneControllerMgr.Instance.LoadFromPluginData(this, chaControl.chaFile);
                        //SaveToFile();
                    }
                    else
                    {
                        BoneControllerMgr.Instance.LoadFromPluginData(this, chaControl.chaFile);
                    }
                }
            }
            catch (Exception value)
            {
                Logger.Log(LogLevel.Error, "[ABM] Unxepected Error for " + chaControl.chaFile.parameter.fullname);
                Logger.Log(LogLevel.Error, value);
            }
        }

        private void InsertAdditionalModifiers()
        {
            // 0 = male, else female
            // TODO will they work for male too?
            if (chaControl.fileParam.sex != 0)
            {
                InsertAdditionalModifier("cf_j_shoulder_L");
                InsertAdditionalModifier("cf_j_shoulder_R");
                InsertAdditionalModifier("cf_j_arm00_L");
                InsertAdditionalModifier("cf_j_arm00_R");
                InsertAdditionalModifier("cf_j_forearm01_L");
                InsertAdditionalModifier("cf_j_forearm01_R");
                InsertAdditionalModifier("cf_j_hand_L");
                InsertAdditionalModifier("cf_j_hand_R");
                InsertAdditionalModifier("cf_j_waist01");
                InsertAdditionalModifier("cf_j_waist02");
                InsertAdditionalModifier("cf_j_thigh00_L");
                InsertAdditionalModifier("cf_j_thigh00_R");
                InsertAdditionalModifier("cf_j_leg01_L");
                InsertAdditionalModifier("cf_j_leg01_R");
                InsertAdditionalModifier("cf_j_leg03_L");
                InsertAdditionalModifier("cf_j_leg03_R");
                InsertAdditionalModifier("cf_j_foot_L");
                InsertAdditionalModifier("cf_j_foot_R");
                InsertAdditionalModifier("cf_j_ana");
            }
        }

        public BoneModifierBody InsertAdditionalModifier(string boneName)
        {
            var boneModifierBody = new BoneModifierBody(-1, null) { boneName = boneName };
            var loopGo = GetRootTransform().FindLoop(boneName);
            if (loopGo != null)
                boneModifierBody.manualTarget = loopGo.transform;
            else
                Console.WriteLine("Bone {0} not found but include forcefully.", boneName);
            modifiers.Add(boneModifierBody.boneName, boneModifierBody);
            return boneModifierBody;
        }

        public BoneModifierBody FindOrCreateModifierByBoneName(string boneName)
        {
            if (modifiers.ContainsKey(boneName))
                return modifiers[boneName];
            return InsertAdditionalModifier(boneName);
        }


        private string GetLastLoadedFile()
        {
            if (isCustomScene)
                return BoneControllerMgr.Instance.lastLoadedFile;
            return null;
        }

        public bool IsExtDataExists()
        {
            return isCustomScene && IsExtDataExists(BoneControllerMgr.Instance.lastLoadedFile);
        }

        public bool IsExtDataExists(string baseCharaFileName)
        {
            return File.Exists(GetExtDataFilePath(baseCharaFileName));
        }

        public string GetExtDataFilePath()
        {
            return GetExtDataFilePath(GetLastLoadedFile());
        }


        public string GetExtDataFilePath(string baseName)
        {
            if (GetLastLoadedFile() != null)
                return GetExtDataFilePath(baseName, chaControl.fileParam.sex);
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


        public void LoadFromFile()
        {
            ClearModifiers();
            if (IsExtDataExists())
            {
                LoadFromFile(GetExtDataFilePath());
                return;
            }
            if (isCustomScene && BoneControllerMgr.Instance.lastLoadedFile != null &&
                File.Exists(BoneControllerMgr.Instance.lastLoadedFile))
            {
                var chaFileControl = new ChaFileControl();
                chaFileControl.LoadCharaFile(BoneControllerMgr.Instance.lastLoadedFile, chaControl.fileParam.sex,
                    false, false);
                BoneControllerMgr.Instance.LoadFromPluginData(this, chaFileControl);
                Logger.Log(LogLevel.Error, "bonemod.txt not found. try to load from card data.");
            }
            //SaveToFile();
        }


        public void LoadFromFile(string path)
        {
            var lines = WriteSafeReadAllLines(path);
            if (modifiers.Count == 0)
                return;
            //lastLoadedFileTimestamp = -1L;
            ClearModifiers();
            var num = File.GetLastWriteTime(path).ToBinary();
            Console.WriteLine("Load from file: {0}, timestamp: {1}", path, num);
            if (ReadDataFromLines(lines))
            {
                //SaveToFile(path);
                //num = File.GetLastWriteTime(path).ToBinary();
            }
            //lastLoadedPath = path;
            //lastLoadedFileTimestamp = num;
        }


        public void LoadFromTextData(string textData)
        {
            var lines = ReadAllLinesFromReader(new StringReader(textData));
            if (modifiers.Count == 0)
                return;
            //lastLoadedFileTimestamp = -1L;
            //lastLoadedPath = null;
            ClearModifiers();
            ReadDataFromLines(lines);
        }


        private bool ReadDataFromLines(string[] lines)
        {
            var result = false;
            for (var i = 0; i < lines.Length; i++)
            {
                var text = lines[i].Trim();
                if (!string.IsNullOrEmpty(text))
                    try
                    {
                        var array = text.Split(',');
                        var text2 = array[1];
                        var enabled = bool.Parse(array[2]);
                        var x = float.Parse(array[3]);
                        var y = float.Parse(array[4]);
                        var z = float.Parse(array[5]);
                        var lenMod = 1f;
                        if (array.Length > 6)
                            lenMod = float.Parse(array[6]);
                        else
                            result = true;
                        BoneModifierBody boneModifierBody;
                        if (modifiers.ContainsKey(text2))
                        {
                            boneModifierBody = modifiers[text2];
                        }
                        else
                        {
                            int.Parse(array[0]);
                            boneModifierBody = FindOrCreateModifierByBoneName(text2);
                        }
                        boneModifierBody.enabled = enabled;
                        boneModifierBody.sclMod.x = x;
                        boneModifierBody.sclMod.y = y;
                        boneModifierBody.sclMod.z = z;
                        boneModifierBody.lenMod = lenMod;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("Parse error. Failed to load line : {0}", text);
                    }
            }
            return result;
        }


        /*public void SaveToFile()
        {
            if (GetLastLoadedFile() != null)
            {
                if (!GetLastLoadedFile().ToLower().EndsWith(EDIT_DEFAULT_FILE_NAME))
                {
                    var extDataFilePath = GetExtDataFilePath();
                    SaveToFile(extDataFilePath);
                }
            }
            else
            {
                Console.WriteLine("Cannot Save.");
            }
        }*/

        /// <summary>
        /// Get rid of the legacy config file
        /// </summary>
        public void DeleteFile()
        {
            var extDataFilePath = GetExtDataFilePath();
            File.Delete(extDataFilePath);
        }

        /*public void SaveToFile(string path)
        {
            Console.WriteLine("Save to file {0}", path);
            using (var streamWriter = File.CreateText(path))
            {
                SaveToWriter(streamWriter);
            }
            lastLoadedPath = path;
            lastLoadedFileTimestamp = File.GetLastWriteTime(path).ToBinary();
        }*/

        public string Serialize()
        {
            var sb = new StringBuilder();
            foreach (var key in modifiers.Keys)
            {
                var boneModifierBody = modifiers[key];

                if (boneModifierBody.sclMod.Equals(Vector3.one) && boneModifierBody.lenMod == 1f)
                    continue;

                sb.AppendLine(string.Join(",", new[]{
                    boneModifierBody.boneIndex.ToString(CultureInfo.InvariantCulture),
                    boneModifierBody.boneName,
                    boneModifierBody.enabled.ToString(CultureInfo.InvariantCulture),
                    boneModifierBody.sclMod.x.ToString(CultureInfo.InvariantCulture),
                    boneModifierBody.sclMod.y.ToString(CultureInfo.InvariantCulture),
                    boneModifierBody.sclMod.z.ToString(CultureInfo.InvariantCulture),
                    boneModifierBody.lenMod.ToString(CultureInfo.InvariantCulture)
                }));
            }
            return sb.ToString();
        }

        /*protected void Update()
        {
            try
            {
                if (Input.GetKeyDown((KeyCode)109) && (Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)303)) &&
                    (Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305)))
                {
                    disalbed = !disalbed;
                    Console.WriteLine("BoneController disabled: {0}", disalbed);
                    if (disalbed)
                    {
                        foreach (var boneModifierBody in modifiers.Values)
                            boneModifierBody.Reset();
                        chaControl.UpdateShapeBody();
                        chaControl.UpdateShapeFace();
                        StartCoroutine(CollectBaselineCo());
                    }
                }
                if (Input.GetKeyDown((KeyCode)108) && (Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)303)) &&
                    (Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305)))
                    LoadFromFile();
                if (baseLineKnown && IsBaselineChanged())
                    StartCoroutine(ForceBoneUpdateAndRebaseCo());
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
            }
        }
        
        private bool IsBaselineChanged()
        {
            if (sibBodyValues == null || sibFaceValues == null)
                return true;
            for (var i = 0; i < chaControl.fileBody.shapeValueBody.Length; i++)
                if (sibBodyValues[i] != chaControl.fileBody.shapeValueBody[i])
                    return true;
            for (var j = 0; j < chaControl.fileFace.shapeValueFace.Length; j++)
                if (sibFaceValues[j] != chaControl.fileFace.shapeValueFace[j])
                    return true;
            return false;
        }*/

        protected void LateUpdate()
        {
            try
            {
                if (BoneControllerMgr.Instance.needReload)
                    BoneControllerMgr.Instance.LoadFromPluginData(this, chaControl.chaFile);

                if (baseLineKnown && !BoneControllerMgr.Instance.needReload)
                {
                    if (missingBoneNextChecktime <= 0f)
                    {
                        foreach (var boneModifierBody in modifiers.Values)
                        {
                            if (!boneModifierBody.isNotManual && boneModifierBody.manualTarget == null)
                            {
                                var loopGo = GetRootTransform().FindLoop(boneModifierBody.boneName);
                                if (loopGo != null)
                                {
                                    boneModifierBody.manualTarget = loopGo.transform;
                                    boneModifierBody.CollectBaseline();
                                }
                            }
                        }

                        missingBoneNextChecktime = MISSING_BONE_CHECK_INTERVAL;
                    }
                    else
                    {
                        missingBoneNextChecktime -= Time.deltaTime;
                    }

                    ApplyAll();
                }
                else
                {
                    StartCoroutine(CollectBaselineCo());
                }
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
            }
        }

        public void ApplyAll()
        {
            foreach (var boneModifierBody in modifiers.Values)
                boneModifierBody.Apply();
        }


        private IEnumerator CollectBaselineCo()
        {
            if (chaControl.animBody == null) yield break;
            var pvCopy = chaControl.animBody.gameObject.GetComponent<PVCopy>();
            var currentPvCopy = new bool[4];
            if (pvCopy != null)
                for (var i = 0; i < 4; i++)
                {
                    currentPvCopy[i] = pvCopy[i];
                    pvCopy[i] = false;
                }
            yield return new WaitForEndOfFrame();
            foreach (var boneModifierBody in modifiers.Values)
                boneModifierBody.CollectBaseline();
            baseLineKnown = true;
            yield return new WaitForEndOfFrame();
            if (pvCopy != null)
            {
                var array = f_PvCopy_pv.GetValue(pvCopy) as GameObject[];
                var array2 = f_PvCopy_bone.GetValue(pvCopy) as GameObject[];
                for (var j = 0; j < 4; j++)
                    if (currentPvCopy[j] && array2[j] && array[j])
                    {
                        array[j].transform.localScale = array2[j].transform.localScale;
                        //array[j].transform.position - array2[j].transform.position;
                        array[j].transform.position = array2[j].transform.position;
                        array[j].transform.rotation = array2[j].transform.rotation;
                    }
            }
            yield return null;
            enabled = false;
            yield return null;
            enabled = true;
        }


        /*private IEnumerator ForceBoneUpdateAndRebaseCo()
        {
            yield return null;
            sibBodyValues = new float[chaControl.fileBody.shapeValueBody.Length];
            chaControl.fileBody.shapeValueBody.CopyTo(sibBodyValues, 0);
            sibFaceValues = new float[chaControl.fileFace.shapeValueFace.Length];
            chaControl.fileFace.shapeValueFace.CopyTo(sibFaceValues, 0);
            chaControl.updateShapeBody = true;
            chaControl.updateShapeFace = true;
            chaControl.UpdateShapeFace();
            chaControl.UpdateShapeBody();
            foreach (var boneModifierBody in modifiers.Values)
                if (boneModifierBody.boneIndex != -1 && boneModifierBody.isScaleBone)
                    boneModifierBody.CollectBaseline();
            baseLineKnown = true;
        }*/


        /*private IEnumerator WatchLoadedFileUpdateCo()
        {
            for (; ; )
            {
                yield return new WaitForSeconds(0.5f);
                if (lastLoadedFileTimestamp != -1L && lastLoadedPath != null)
                {
                    var num = File.GetLastWriteTime(lastLoadedPath).ToBinary();
                    if (File.Exists(lastLoadedPath) && num != lastLoadedFileTimestamp)
                    {
                        yield return new WaitForEndOfFrame();
                        Console.WriteLine("Timestamp changed. Reload the file {0}: ", lastLoadedPath);
                        LoadFromFile();
                    }
                }
            }
        }*/


        private static string[] WriteSafeReadAllLines(string path)
        {
            string[] result;
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    result = ReadAllLinesFromReader(streamReader);
                }
            }
            return result;
        }


        private static string[] ReadAllLinesFromReader(TextReader sr)
        {
            var list = new List<string>();
            for (; ; )
            {
                var text = sr.ReadLine();
                if (text == null)
                    break;
                list.Add(text);
            }
            return list.ToArray();
        }


        public ChaControl GetChaControl()
        {
            return chaControl;
        }

        private Transform GetRootTransform()
        {
            return chaControl.objBodyBone.transform;
        }
    }
}