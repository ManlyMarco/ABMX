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
        private const string ExtDataBoneDataKey = "boneData";
        private const string ExtDataSaveId = "KKABMPlugin.ABMData";

        internal static readonly List<BoneController> BoneControllers = new List<BoneController>();

        internal static bool InsideMaker => MakerAPI.MakerAPI.Instance.InsideMaker;
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
                    data = { [ExtDataBoneDataKey] = boneDataPluginData.data[ExtDataBoneDataKey] }
                };
                SetExtendedCharacterData(dst, pluginData);
            }
        }

        private static PluginData GetExtendedCharacterData(ChaFile chaFile)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));

            return ExtendedSave.GetExtendedDataById(chaFile, ExtDataSaveId);
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
            if (!String.IsNullOrEmpty(extDataFilePath) && File.Exists(extDataFilePath))
            {
                Logger.Log(LogLevel.Info, $"[KKABMX] Importing external ABM data from file: {extDataFilePath}");

                var value = File.ReadAllText(extDataFilePath);
                var pluginData = new PluginData
                {
                    version = 1,
                    data = { [ExtDataBoneDataKey] = value }
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
                pluginData.data.TryGetValue(ExtDataBoneDataKey, out var value) &&
                value is string textData &&
                !String.IsNullOrEmpty(textData))
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

            ExtendedSave.GetAllExtendedData(chaFile)?.Remove(ExtDataSaveId);
        }

        private static void SetExtendedCharacterData(ChaFile chaFile, PluginData pluginData)
        {
            if (chaFile == null) throw new ArgumentNullException(nameof(chaFile));
            if (pluginData == null) throw new ArgumentNullException(nameof(pluginData));

            Logger.Log(LogLevel.Info, "[KKABMX] Saving embedded ABM data to character card: " +
                (chaFile.charaFileName ?? chaFile.parameter.fullname ?? "[Unnamed]"));
            ExtendedSave.SetExtendedDataById(chaFile, ExtDataSaveId, pluginData);
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

                SceneManager.sceneLoaded += (sc, mode) => Instance.LastLoadedFile = null;
                MakerAPI.MakerAPI.Instance.InsideMakerChanged += (sender, args) => Instance.LastLoadedFile = null;

                MakerAPI.MakerAPI.Instance.CharacterChanged += (sender, args) =>
                {
                    if (args.Face || args.Body)
                        Instance.OnLimitedLoad(args.Filename, args.LoadedChaFile);
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
                data = { [ExtDataBoneDataKey] = boneController.Serialize() }
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
