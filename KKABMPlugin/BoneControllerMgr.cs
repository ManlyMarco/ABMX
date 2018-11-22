using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using ChaCustom;
using ExtensibleSaveFormat;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

namespace KKABMX.Core
{
    public class BoneControllerMgr : MonoBehaviour
    {
        private const string BoneDataKey = "boneData";
        private const string ExtendedSaveId = "KKABMPlugin.ABMData";

        /// <summary>
        /// Extra bones to handle. Add extra bone names to handle before characters are created.
        /// </summary>
        public List<string> AdditionalBoneNames { get; } = new List<string>
        {
            "cf_j_shoulder_L",
            "cf_j_shoulder_R",
            "cf_j_arm00_L",
            "cf_j_arm00_R",
            "cf_j_forearm01_L",
            "cf_j_forearm01_R",
            "cf_j_hand_L",
            "cf_j_hand_R",
            "cf_j_waist01",
            "cf_j_waist02",
            "cf_j_thigh00_L",
            "cf_j_thigh00_R",
            "cf_j_leg01_L",
            "cf_j_leg01_R",
            "cf_j_leg03_L",
            "cf_j_leg03_R",
            "cf_j_foot_L",
            "cf_j_foot_R",
            "cf_j_ana",
            "cm_J_dan109_00",
            "cm_J_dan100_00",
            "cm_J_dan_f_L",
            "cm_J_dan_f_R",
            "cf_j_kokan",
            "cf_j_toes_L",
            "cf_j_toes_R",
            "cf_hit_head",
            "cf_j_index01_L" ,
            "cf_j_index02_L" ,
            "cf_j_index03_L" ,
            "cf_j_little01_L",
            "cf_j_little02_L",
            "cf_j_little03_L",
            "cf_j_middle01_L",
            "cf_j_middle02_L",
            "cf_j_middle03_L",
            "cf_j_ring01_L"  ,
            "cf_j_ring02_L"  ,
            "cf_j_ring03_L"  ,
            "cf_j_thumb01_L" ,
            "cf_j_thumb02_L" ,
            "cf_j_thumb03_L" ,
            "cf_j_index01_R" ,
            "cf_j_index02_R" ,
            "cf_j_index03_R" ,
            "cf_j_little01_R",
            "cf_j_little02_R",
            "cf_j_little03_R",
            "cf_j_middle01_R",
            "cf_j_middle02_R",
            "cf_j_middle03_R",
            "cf_j_ring01_R"  ,
            "cf_j_ring02_R"  ,
            "cf_j_ring03_R"  ,
            "cf_j_thumb01_R" ,
            "cf_j_thumb02_R" ,
            "cf_j_thumb03_R" ,
        };

        internal static readonly List<BoneController> BoneControllers = new List<BoneController>();

        public bool InsideMaker { get; private set; }
        public static BoneControllerMgr Instance { get; private set; }

        public string LastLoadedFile { get; private set; }

        public bool NeedReload { get; private set; }

        public static void ClearExtDataAndBoneController(ChaFileControl chaFile)
        {
            RemoveExtendedCharacterData(chaFile);

            foreach (var boneController in GetAllBoneControllers())
            {
                if (boneController != null &&
                    boneController.ChaControl != null &&
                    boneController.ChaControl.chaFile == chaFile)
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
                    data = { [BoneDataKey] = boneDataPluginData.data[BoneDataKey] }
                };
                SetExtendedCharacterData(dst, pluginData);
            }
        }

        public void EnterCustomScene()
        {
            InsideMaker = Singleton<CustomBase>.Instance != null;
            LastLoadedFile = null;
        }

        public void ExitCustomScene()
        {
            InsideMaker = false;
            LastLoadedFile = null;
        }

        private static PluginData GetExtendedCharacterData(ChaFile chaFile)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));

            return ExtendedSave.GetExtendedDataById(chaFile, ExtendedSaveId);
        }

        public static BoneController GetOrAttachBoneController(ChaControl charInfo)
        {
            if (charInfo == null)
                return null;

            var existing = charInfo.gameObject.GetComponent<BoneController>();
            return existing ?? charInfo.gameObject.AddComponent<BoneController>();
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
                    data = { [BoneDataKey] = value }
                };
                SetExtendedCharacterData(chaFile, pluginData);
                SetNeedReload();
                return;
            }

            if (clearIfNotFound)
                RemoveExtendedCharacterData(chaFile);
        }

        public static void LoadFromPluginData(BoneController boneController, ChaFile chaFile)
        {
            var pluginData = GetExtendedCharacterData(chaFile);
            if (pluginData != null &&
                pluginData.data.TryGetValue(BoneDataKey, out var value) &&
                value is string textData &&
                !string.IsNullOrEmpty(textData))
            {
                Logger.Log(LogLevel.Info, $"[KKABMX] Loading embedded ABM data from card: {chaFile.parameter.fullname}");
                boneController.LoadFromTextData(textData);
            }
            else
                boneController.ResetModifiers();
        }

        public event EventHandler<BoneControllerEventArgs> MakerLimitedLoad;

        private void OnBeforeCardSave(ChaFile chaFile)
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
                    if (boneController != null && boneController.ChaControl != null &&
                        boneController.ChaControl.chaFile == chaFile)
                    {
                        var pluginData2 = SerializeBoneController(boneController);
                        SetExtendedCharacterData(chaFile, pluginData2);
                        break;
                    }
                }
            }
        }

        public void OnCustomSceneExitWithSave()
        {
            if (InsideMaker)
                OnBeforeCardSave(Singleton<CustomBase>.Instance.chaCtrl.chaFile);
        }

        public void OnLimitedLoad(string path, ChaFile chaFile)
        {
            if (InsideMaker)
            {
                LastLoadedFile = path;

                var makerController = FindObjectOfType<BoneController>();
                LoadFromPluginData(makerController, chaFile);

                MakerLimitedLoad?.Invoke(this, new BoneControllerEventArgs(makerController));
            }
        }

        private static void RemoveExtendedCharacterData(ChaFileControl chaFile)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));

            ExtendedSave.GetAllExtendedData(chaFile)?.Remove(ExtendedSaveId);
        }

        private static void SetExtendedCharacterData(ChaFile chaFile, PluginData pluginData)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));
            if (pluginData == null) throw new ArgumentNullException(nameof(pluginData));

            Logger.Log(LogLevel.Info, "[KKABMX] Saving embedded ABM data to character card: " +
                (chaFile.charaFileName ?? chaFile.parameter.fullname ?? "[Unnamed]"));
            ExtendedSave.SetExtendedDataById(chaFile, ExtendedSaveId, pluginData);
        }

        public void SetNeedReload()
        {
            if (!NeedReload)
            {
                NeedReload = true;
                StartCoroutine(ClearReloadFlagCo());
            }
        }

        internal static void Init()
        {
            if (!Instance)
            {
                Instance = new GameObject("BoneControllerMgr").AddComponent<BoneControllerMgr>();
                DontDestroyOnLoad(Instance.gameObject);
                ExtendedSave.CardBeingSaved += Instance.OnBeforeCardSave;
                SceneManager.sceneLoaded += (sc, mode) => {
                    Instance.InsideMaker = Singleton<CustomBase>.Instance != null;
                    Instance.LastLoadedFile = null;
                };
            }
        }

        private IEnumerator ClearReloadFlagCo()
        {
            yield return new WaitForEndOfFrame();
            NeedReload = false;
        }

        private static IEnumerable<BoneController> GetAllBoneControllers()
        {
            return BoneControllers.Where(x => x != null);
            //return FindObjectsOfType<BoneController>();
        }

        private static PluginData SerializeBoneController(BoneController boneController)
        {
            var pluginData = new PluginData
            {
                version = 1,
                data = { [BoneDataKey] = boneController.Serialize() }
            };

            return pluginData;
        }

        public sealed class BoneControllerEventArgs : EventArgs
        {
            public BoneControllerEventArgs(BoneController controller)
            {
                Controller = controller;
            }

            public BoneController Controller { get; }
        }
    }
}
