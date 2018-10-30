using System;
using System.IO;
using System.Reflection;
using ChaCustom;
using Harmony;
using Studio;
using UnityEngine;
using UnityEngine.UI;

namespace KKABMPlugin
{
    public static class Hook
    {
        public static void InstallHook()
        {
            HarmonyInstance.Create("KKABMPlugin.Hook").PatchAll(typeof(Hook));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), "Initialize", new[]
        {
            typeof(byte),
            typeof(bool),
            typeof(GameObject),
            typeof(int),
            typeof(int),
            typeof(ChaFileControl)
        }, null)]
        public static void DoInitializePostHook(byte _sex, bool _hiPoly, GameObject _objRoot, int _id, int _no,
            ChaFileControl _chaFile, ChaControl __instance)
        {
            if (!__instance.gameObject.GetComponent<BoneController>())
                __instance.gameObject.AddComponent<BoneController>();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaFileControl), "LoadFileLimited", new[]
        {
            typeof(string),
            typeof(byte),
            typeof(bool),
            typeof(bool),
            typeof(bool),
            typeof(bool),
            typeof(bool)
        }, null)]
        public static void DoLoadLimitedPostHook(string filename, byte sex, bool face, bool body, bool hair,
            bool parameter, bool coordinate, ChaFileControl __instance)
        {
            if ((face || body) && BoneControllerMgr.Instance)
                BoneControllerMgr.Instance.OnLoad(filename);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaFileControl), "LoadCharaFile", new[]
        {
            typeof(string),
            typeof(byte),
            typeof(bool),
            typeof(bool)
        }, null)]
        public static void DoLoadCharaFilePreHook(string filename, byte sex, bool noLoadPng, bool noLoadStatus,
            ChaFileControl __instance)
        {
            BoneControllerMgr.Instance.ClearExtDataAndBoneController(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaFileControl), "LoadCharaFile", new[]
        {
            typeof(string),
            typeof(byte),
            typeof(bool),
            typeof(bool)
        }, null)]
        public static void DoLoadCharaFilePostHook(string filename, byte sex, bool noLoadPng, bool noLoadStatus,
            ChaFileControl __instance)
        {
            BoneControllerMgr.Instance.LoadAsExtSaveData(filename, __instance, false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaFileControl), "LoadFromAssetBundle", new[]
        {
            typeof(string),
            typeof(string),
            typeof(bool),
            typeof(bool)
        }, null)]
        public static void DoLoadCharaFilePostHook(string assetBundleName, string assetName, bool noSetPNG,
            bool noLoadStatus, ChaFileControl __instance)
        {
            BoneControllerMgr.Instance.ClearExtDataAndBoneController(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OCIChar), "ChangeChara", new[]
        {
            typeof(string)
        }, null)]
        public static void DoStudioChangeCharaPostHook(string _path, OCIChar __instance)
        {
            var component = __instance.charInfo.gameObject.GetComponent<BoneController>();
            if (component != null)
                BoneControllerMgr.Instance.LoadFromPluginData(component, __instance.charInfo.chaFile);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaFileControl), "SaveCharaFile", new[]
        {
            typeof(BinaryWriter),
            typeof(bool)
        }, null)]
        public static void DoSavePreHook(BinaryWriter bw, bool savePng, ChaFileControl __instance)
        {
            if (BoneControllerMgr.Instance)
                BoneControllerMgr.Instance.OnPreSave(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaFileControl), "SaveCharaFile", new[]
        {
            typeof(string),
            typeof(byte),
            typeof(bool)
        }, null)]
        public static void DoSavePostHook(string filename, byte sex, bool newFile, ChaFileControl __instance)
        {
            if (BoneControllerMgr.Instance)
                BoneControllerMgr.Instance.OnSave(__instance.charaFileName);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaveData.CharaData), "Save", new[]
        {
            typeof(BinaryWriter)
        }, null)]
        public static void DoSaveDataSavePreHook(BinaryWriter w, SaveData.CharaData __instance)
        {
            if (BoneControllerMgr.Instance)
                BoneControllerMgr.Instance.OnPreCharaDataSave(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaFile), "CopyChaFile", new[]
        {
            typeof(ChaFile),
            typeof(ChaFile)
        }, null)]
        public static void DoCopyChaFilePostHook(ChaFile dst, ChaFile src)
        {
            if (dst is ChaFileControl && src is ChaFileControl)
                BoneControllerMgr.CloneBoneDataPluginData(src as ChaFileControl, dst as ChaFileControl);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomScene), "Start", new Type[]
        {
        }, null)]
        public static void OnCustomSceneStart()
        {
            BoneControllerMgr.Instance.EnterCustomScene();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomScene), "OnDestroy", new Type[]
        {
        }, null)]
        public static void OnCustomSceneDestroy()
        {
            BoneControllerMgr.Instance.ExitCustomScene();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CvsExit), "ExitSceneRestoreStatus", new[]
        {
            typeof(string)
        }, null)]
        public static void OnExitSceneRestoreStatus(string strInput, CvsExit __instance)
        {
            BoneControllerMgr.Instance.OnCustomSceneExitWithSave();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LiveCharaSelectSprite), "Start", new Type[]
        {
        }, null)]
        public static void LiveCharaSelectSprite_StartPostHook(LiveCharaSelectSprite __instance)
        {
            (typeof(LiveCharaSelectSprite)
                .GetField("btnIdolBack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(__instance) as Button).onClick.AddListener(delegate
            {
                BoneControllerMgr.Instance.SetNeedReload();
            });
        }
    }
}