using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using ChaCustom;
using ExtensibleSaveFormat;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace KKABMX.Core
{
    public class BoneControllerMgr : MonoBehaviour
    {
        private const string EXTENDED_SAVE_ID = "KKABMPlugin.ABMData";
        private const string BONE_DATA_KEY = "boneData";
        //public List<BoneController> boneControllers = new List<BoneController>();

        public bool InsideMaker { get; private set; }
        public static BoneControllerMgr Instance { get; private set; }

        public string lastLoadedFile;

        public bool needReload;

        public List<string> AdditionalBoneNames { get; } = new List<string>
        {
            "cf_j_shoulder_L" ,
            "cf_j_shoulder_R" ,
            "cf_j_arm00_L"    ,
            "cf_j_arm00_R"    ,
            "cf_j_forearm01_L",
            "cf_j_forearm01_R",
            "cf_j_hand_L"     ,
            "cf_j_hand_R"     ,
            "cf_j_waist01"    ,
            "cf_j_waist02"    ,
            "cf_j_thigh00_L"  ,
            "cf_j_thigh00_R"  ,
            "cf_j_leg01_L"    ,
            "cf_j_leg01_R"    ,
            "cf_j_leg03_L"    ,
            "cf_j_leg03_R"    ,
            "cf_j_foot_L"     ,
            "cf_j_foot_R"     ,
            "cf_j_ana"        ,
            "cm_J_dan109_00"  ,
            "cm_J_dan100_00"  ,
            "cm_J_dan_f_L"    ,
            "cm_J_dan_f_R"    ,
            "cf_j_kokan"
        };

        public static void Init()
        {
            if (!Instance)
            {
                Instance = new GameObject("BoneControllerMgr").AddComponent<BoneControllerMgr>();
                DontDestroyOnLoad(Instance.gameObject);
                ExtendedSave.CardBeingSaved += Instance.OnBeforeCardSave;
            }
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
            if (InsideMaker)
                lastLoadedFile = path;
        }


        protected void OnLevelWasLoaded(int level)
        {
            InsideMaker = Singleton<CustomBase>.Instance != null;
            lastLoadedFile = null;
        }


        public void EnterCustomScene()
        {
            InsideMaker = Singleton<CustomBase>.Instance != null;
            lastLoadedFile = null;
        }


        public void ExitCustomScene()
        {
            InsideMaker = false;
            lastLoadedFile = null;
        }

        public void OnCustomSceneExitWithSave()
        {
            if (InsideMaker)
                OnBeforeCardSave(Singleton<CustomBase>.Instance.chaCtrl.chaFile);
        }

        /*private string GetPath(Transform root)
        {
            var text = root.name;
            var parent = root.parent;
            while (parent != null)
            {
                text = parent.name + "/" + text;
                parent = parent.parent;
            }
            return text;
        }*/

        public void OnLimitedLoad(string path, ChaFile chaFile)
        {
            if (InsideMaker)
            {
                lastLoadedFile = path;

                var makerController = FindObjectOfType<BoneController>();
                LoadFromPluginData(makerController, chaFile);

                MakerLimitedLoad?.Invoke(this, new BoneControllerEventArgs(makerController));
            }
        }

        public event EventHandler<BoneControllerEventArgs> MakerLimitedLoad;

        public sealed class BoneControllerEventArgs : EventArgs
        {
            public BoneControllerEventArgs(BoneController controller)
            {
                Controller = controller;
            }

            public BoneController Controller { get; }
        }

        public static PluginData GetExtendedCharacterData(ChaFile chaFile)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));

            return ExtendedSave.GetExtendedDataById(chaFile, EXTENDED_SAVE_ID);
        }

        public static void SetExtendedCharacterData(ChaFile chaFile, PluginData pluginData)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));
            if (pluginData == null) throw new ArgumentNullException(nameof(pluginData));

            Logger.Log(LogLevel.Info, "[KKABMX] Saving embedded ABM data to character card: " + (chaFile.charaFileName ?? chaFile.parameter.fullname));
            ExtendedSave.SetExtendedDataById(chaFile, EXTENDED_SAVE_ID, pluginData);
        }

        public static void RemoveExtendedCharacterData(ChaFileControl chaFile)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));
            //if (GetCharacterData(chaFile) != null)
            ExtendedSave.GetAllExtendedData(chaFile)?.Remove(EXTENDED_SAVE_ID);
        }

        public void LoadAsExtSaveData(string charCardPath, ChaFileControl chaFile, bool clearIfNotFound = true)
        {
            var extDataFilePath = BoneController.GetExtDataFilePath(charCardPath, chaFile.parameter.sex);
            if (!string.IsNullOrEmpty(extDataFilePath) && File.Exists(extDataFilePath))
            {
                Logger.Log(LogLevel.Info, $"[KKABMX] Importing external ABM data from file: {extDataFilePath}");

                var value = File.ReadAllText(extDataFilePath);
                var pluginData = new PluginData
                {
                    version = 1,
                    data = { [BONE_DATA_KEY] = value }
                };
                SetExtendedCharacterData(chaFile, pluginData);
                SetNeedReload();
                return;
            }

            if (clearIfNotFound)
                RemoveExtendedCharacterData(chaFile);
        }

        public void ClearExtDataAndBoneController(ChaFileControl chaFile)
        {
            RemoveExtendedCharacterData(chaFile);

            foreach (var boneController in GetAllBoneControllers())
            {
                if (boneController != null &&
                    boneController.chaControl != null &&
                    boneController.chaControl.chaFile == chaFile)
                {
                    boneController.ResetModifiers();
                    break;
                }
            }
        }

        public static void CloneBoneDataPluginData(ChaFileControl src, ChaFile dst)
        {
            var boneDataPluginData = GetExtendedCharacterData(src);
            if (boneDataPluginData != null)
            {
                var pluginData = new PluginData
                {
                    version = boneDataPluginData.version,
                    data = { [BONE_DATA_KEY] = boneDataPluginData.data[BONE_DATA_KEY] }
                };
                SetExtendedCharacterData(dst, pluginData);
            }
        }

        public void LoadFromPluginData(BoneController boneController, ChaFile chaFile)
        {
            var pluginData = GetExtendedCharacterData(chaFile);
            if (pluginData != null &&
                pluginData.data.TryGetValue(BONE_DATA_KEY, out var value) &&
                value is string textData &&
                !string.IsNullOrEmpty(textData))
            {
                Logger.Log(LogLevel.Info, "[KKABMX] Loading embedded ABM data from card: " + chaFile.parameter.fullname);
                boneController.LoadFromTextData(textData);
            }
            else
            {
                boneController.ResetModifiers();
            }
        }

        /*public void OnPreCharaDataSave(SaveData.CharaData saveCharaData)
        {
            //if (GetCharacterData(saveCharaData.charFile) != null)
            //    Logger.Log(LogLevel.Info, "[Save] Save bone data as ExtensibleSaveFormat: " + saveCharaData.charFile.parameter.fullname);
        }*/


        public void OnBeforeCardSave(ChaFile chaFile)
        {
            if (InsideMaker)
            {
                var boneController = Singleton<CustomBase>.Instance.chaCtrl.gameObject.GetComponent<BoneController>();
                if (boneController)
                {
                    var pluginData = SerializeBoneController(boneController);
                    SetExtendedCharacterData(chaFile, pluginData);

                    // Get rid of the text file since we already loaded it and will now save changes to the card
                    boneController.DeleteFile();
                }
            }
            else
            {
                foreach (var boneController in GetAllBoneControllers())
                {
                    if (boneController != null && boneController.chaControl != null &&
                        boneController.chaControl.chaFile == chaFile)
                    {
                        var pluginData2 = SerializeBoneController(boneController);
                        SetExtendedCharacterData(chaFile, pluginData2);
                        break;
                    }
                }
            }
        }

        private static PluginData SerializeBoneController(BoneController boneController)
        {
            var pluginData = new PluginData
            {
                version = 1,
                data = { [BONE_DATA_KEY] = boneController.Serialize() }
            };

            return pluginData;
        }

        /*public void OnSave(string path)
        {
            if (InsideMaker)
            {
                lastLoadedFile = path;
                SaveAllControllers();
            }
        }*/

        public void ReloadAllControllers()
        {
            var controllers = GetAllBoneControllers();
            foreach (var bc in controllers)
                LoadFromPluginData(bc, bc.chaControl.chaFile);
        }

        /*public void SaveAllControllers()
        {
            var controllers = GetAllBoneControllers();
            foreach (var bc in controllers)
                bc.SaveToFile();
        }*/

        private static BoneController[] GetAllBoneControllers()
        {
            return FindObjectsOfType<BoneController>();
        }

        /*public void RegisterBoneController(BoneController controller)
        {
            boneControllers.Add(controller);
        }*/

        /*private T FindObject<T>(Transform transform)
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
        }*/


        public static BoneController AttachBoneController(ChaControl charInfo)
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