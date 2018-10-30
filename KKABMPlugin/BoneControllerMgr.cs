using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using ChaCustom;
using ExtensibleSaveFormat;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace KKABMPlugin
{
    internal class BoneControllerMgr : MonoBehaviour
    {
        public const string EXTENDED_SAVE_ID = "KKABMPlugin.ABMData";


        public const string BONE_DATA_KEY = "boneData";


        public List<BoneController> boneControllers = new List<BoneController>();


        private CustomBase customBase;


        public string lastLoadedFile;


        public bool needReload;

        // (get) Token: 0x06000032 RID: 50 RVA: 0x0000332B File Offset: 0x0000152B
        public static BoneControllerMgr Instance { get; private set; }


        public static void Init()
        {
            if (!Instance)
            {
                Instance = new GameObject("BoneControllerMgr").AddComponent<BoneControllerMgr>();
                DontDestroyOnLoad(Instance.gameObject);
            }
        }


        protected void Start()
        {
        }


        private IEnumerator ClearReloadFlagCo()
        {
            yield return new WaitForEndOfFrame();
            needReload = false;
        }


        public void SetNeedReload()
        {
            if (!needReload)
            {
                needReload = true;
                StartCoroutine(ClearReloadFlagCo());
            }
        }


        public void SetCustomLastLoadedFile(string path)
        {
            if (customBase != null)
                lastLoadedFile = path;
        }


        protected void OnLevelWasLoaded(int level)
        {
            customBase = Singleton<CustomBase>.Instance;
            lastLoadedFile = null;
        }


        public void EnterCustomScene()
        {
            customBase = Singleton<CustomBase>.Instance;
            lastLoadedFile = null;
        }


        public void ExitCustomScene()
        {
            customBase = null;
            lastLoadedFile = null;
        }


        public void OnCustomSceneExitWithSave()
        {
            if (customBase != null)
                OnPreSave(customBase.chaCtrl.chaFile);
        }


        private string GetPath(Transform root)
        {
            var text = root.name;
            var parent = root.parent;
            while (parent != null)
            {
                text = parent.name + "/" + text;
                parent = parent.parent;
            }
            return text;
        }


        public void OnLoad(string path)
        {
            if (customBase != null)
            {
                lastLoadedFile = path;
                ReloadABM(path);
            }
        }


        public static PluginData GetBoneDataPluginData(ChaFileControl chaFile)
        {
            //if (ExtendedSave.GetAllExtendedData(chaFile) != null)
                return ExtendedSave.GetExtendedDataById(chaFile, "KKABMPlugin.ABMData");
            //return null;
        }


        public void LoadAsExtSaveData(string charCardPath, ChaFileControl chaFile, bool clearIfNotFound = true)
        {
            var extDataFilePath = BoneController.GetExtDataFilePath(charCardPath, chaFile.parameter.sex);
            if (!string.IsNullOrEmpty(extDataFilePath) && File.Exists(extDataFilePath))
            {
                Logger.Log(LogLevel.Error, "[ABM] .bonemod.txt file found: " + extDataFilePath + ".");
                using (var fileStream =
                    new FileStream(extDataFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        var value = streamReader.ReadToEnd();
                        var pluginData = new PluginData();
                        pluginData.version = 1;
                        pluginData.data["boneData"] = value;
                        ExtendedSave.SetExtendedDataById(chaFile, "KKABMPlugin.ABMData", pluginData);
                    }
                }
                SetNeedReload();
                return;
            }
            if (clearIfNotFound && GetBoneDataPluginData(chaFile) != null)
                ExtendedSave.GetAllExtendedData(chaFile).Remove("KKABMPlugin.ABMData");
        }


        public void ClearExtDataAndBoneController(ChaFileControl chaFile)
        {
            if (GetBoneDataPluginData(chaFile) != null)
                ExtendedSave.GetAllExtendedData(chaFile).Remove("KKABMPlugin.ABMData");
            foreach (var boneController in boneControllers)
                if (boneController != null && boneController.chaControl != null &&
                    boneController.chaControl.chaFile == chaFile)
                {
                    boneController.ClearModifiers();
                    break;
                }
        }


        public static void CloneBoneDataPluginData(ChaFileControl src, ChaFileControl dst)
        {
            var boneDataPluginData = GetBoneDataPluginData(src);
            if (boneDataPluginData != null)
            {
                var pluginData = new PluginData();
                pluginData.version = boneDataPluginData.version;
                pluginData.data["boneData"] = boneDataPluginData.data["boneData"];
                ExtendedSave.SetExtendedDataById(dst, "KKABMPlugin.ABMData", pluginData);
            }
        }


        public void LoadFromPluginData(BoneController boneController, ChaFileControl chaFile)
        {
            if (ExtendedSave.GetAllExtendedData(chaFile) != null)
            {
                var extendedDataById = ExtendedSave.GetExtendedDataById(chaFile, "KKABMPlugin.ABMData");
                if (extendedDataById != null)
                {
                    if (extendedDataById.data.ContainsKey("boneData"))
                    {
                        var textData = (string) extendedDataById.data["boneData"];
                        Logger.Log(LogLevel.Info,
                            "[ABM] ExtensibleSaveFormat data found for " + chaFile.parameter.fullname
                        );
                        boneController.LoadFromTextData(textData);
                        return;
                    }
                    boneController.ClearModifiers();
                }
            }
            else
            {
                boneController.ClearModifiers();
            }
        }


        public void OnPreCharaDataSave(SaveData.CharaData saveCharaData)
        {
            if (GetBoneDataPluginData(saveCharaData.charFile) != null)
                Logger.Log(LogLevel.Info,"[Save] Save bone data as ExtensibleSaveFormat: " + saveCharaData.charFile.parameter.fullname);
        }


        public void OnPreSave(ChaFileControl chaFile)
        {
            if (customBase != null)
            {
                var component = customBase.chaCtrl.gameObject.GetComponent<BoneController>();
                if (component)
                {
                    var pluginData = SaveAsPluginData(component);
                    ExtendedSave.SetExtendedDataById(chaFile, "KKABMPlugin.ABMData", pluginData);
                }
            }
            else
            {
                foreach (var boneController in boneControllers)
                    if (boneController != null && boneController.chaControl != null &&
                        boneController.chaControl.chaFile == chaFile)
                    {
                        var pluginData2 = SaveAsPluginData(boneController);
                        ExtendedSave.SetExtendedDataById(chaFile, "KKABMPlugin.ABMData", pluginData2);
                        break;
                    }
            }
        }


        private PluginData SaveAsPluginData(BoneController boneController)
        {
            var pluginData = new PluginData();
            pluginData.version = 1;
            var stringWriter = new StringWriter();
            boneController.SaveToWriter(stringWriter);
            var value = stringWriter.ToString();
            pluginData.data["boneData"] = value;
            return pluginData;
        }


        public void OnSave(string path)
        {
            if (customBase != null)
            {
                lastLoadedFile = path;
                SaveABM();
            }
        }


        public void ReloadABM(string path)
        {
            var array = FindObjectsOfType<BoneController>();
            for (var i = 0; i < array.Length; i++)
                array[i].LoadFromFile();
        }


        public void SaveABM()
        {
            var array = FindObjectsOfType<BoneController>();
            for (var i = 0; i < array.Length; i++)
                array[i].SaveToFile();
        }


        public void RegisterBoneController(BoneController controller)
        {
            boneControllers.Add(controller);
        }


        protected void OnLoadClick()
        {
        }


        protected void OnSaveClick()
        {
        }


        private T FindObject<T>(Transform transform)
        {
            if (transform.gameObject.GetComponent<T>() != null)
                return transform.gameObject.GetComponent<T>();
            for (var i = 0; i < transform.childCount; i++)
            {
                var t = FindObject<T>(transform.GetChild(i));
                if (t != null)
                    return t;
            }
            return default(T);
        }


        private BoneController InstallBoneController(ChaControl charInfo)
        {
            if (charInfo == null)
                return null;
            var component = charInfo.gameObject.GetComponent<BoneController>();
            if (component == null)
                return charInfo.gameObject.AddComponent<BoneController>();
            return component;
        }
    }
}