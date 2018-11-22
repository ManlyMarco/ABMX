using System.Collections;
using System.IO;
using System.Reflection;
using ChaCustom;
using Harmony;
using Studio;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace KKABMX.Core
{
    internal static class Hooks
    {
        public static void InstallHook()
        {
            HarmonyInstance.Create("KKABMPlugin.Hook").PatchAll(typeof(Hooks));
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
        })]
        public static void ChaControl_InitializePostHook(byte _sex, bool _hiPoly, GameObject _objRoot, int _id, int _no,
            ChaFileControl _chaFile, ChaControl __instance)
        {
            if (!MakerListIsLoading)
                BoneControllerMgr.GetOrAttachBoneController(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaFile), "LoadFile", new[] { typeof(BinaryReader), typeof(bool), typeof(bool) })]
        public static void ChaFileLoadFilePreHook(ChaFile __instance, BinaryReader br, bool noLoadPNG, bool noLoadStatus)
        {
            if (!MakerListIsLoading && BoneControllerMgr.Instance != null && BoneControllerMgr.Instance.InsideMaker)
                lastLoadedChaFile = __instance;
            else
                lastLoadedChaFile = null;
        }

        private static ChaFile lastLoadedChaFile;
        private static readonly FieldInfo _idolBackButton = typeof(LiveCharaSelectSprite).GetField("btnIdolBack", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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
        })]
        public static void ChaFileControl_LoadLimitedPostHook(string filename, byte sex, bool face, bool body,
            bool hair,
            bool parameter, bool coordinate, ChaFileControl __instance)
        {
            if ((face || body) && BoneControllerMgr.Instance)
                BoneControllerMgr.Instance.OnLimitedLoad(filename, lastLoadedChaFile);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaFileControl), "LoadCharaFile", new[]
        {
            typeof(string),
            typeof(byte),
            typeof(bool),
            typeof(bool)
        })]
        public static void ChaFileControl_LoadCharaFilePreHook(string filename, byte sex, bool noLoadPng,
            bool noLoadStatus,
            ChaFileControl __instance)
        {
            if (!MakerListIsLoading)
                BoneControllerMgr.ClearExtDataAndBoneController(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaFileControl), "LoadCharaFile", new[]
        {
            typeof(string),
            typeof(byte),
            typeof(bool),
            typeof(bool)
        })]
        public static void ChaFileControl_LoadCharaFilePostHook(string filename, byte sex, bool noLoadPng,
            bool noLoadStatus,
            ChaFileControl __instance)
        {
            if (!MakerListIsLoading)
                BoneControllerMgr.Instance.LoadAsExtSaveData(filename, __instance, false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaFileControl), "LoadFromAssetBundle", new[]
        {
            typeof(string),
            typeof(string),
            typeof(bool),
            typeof(bool)
        })]
        public static void ChaFileControl_LoadCharaFilePostHook(string assetBundleName, string assetName, bool noSetPNG,
            bool noLoadStatus, ChaFileControl __instance)
        {
            if (!MakerListIsLoading)
                BoneControllerMgr.ClearExtDataAndBoneController(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OCIChar), "ChangeChara", new[]
        {
            typeof(string)
        })]
        public static void OCIChar_ChangeCharaPostHook(string _path, OCIChar __instance)
        {
            var component = __instance.charInfo.gameObject.GetComponent<BoneController>();
            if (component != null)
                BoneControllerMgr.Instance.StartCoroutine(DelayedLoad(component, __instance.charInfo.chaFile));
        }

        private static IEnumerator DelayedLoad(BoneController controller, ChaFileControl charInfoChaFile)
        {
            yield return null;
            BoneControllerMgr.LoadFromPluginData(controller, charInfoChaFile);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaFile), "CopyChaFile", new[]
        {
            typeof(ChaFile),
            typeof(ChaFile)
        })]
        public static void ChaFile_CopyChaFilePostHook(ChaFile dst, ChaFile src)
        {
            if (src is ChaFileControl srcCfc && dst is ChaFileControl dstCfc)
                BoneControllerMgr.CloneBoneDataPluginData(srcCfc, dstCfc);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomScene), "Start")]
        public static void CustomScene_Start()
        {
            BoneControllerMgr.Instance.EnterCustomScene();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomScene), "OnDestroy")]
        public static void CustomScene_Destroy()
        {
            BoneControllerMgr.Instance.ExitCustomScene();
            lastLoadedChaFile = null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CvsExit), "ExitSceneRestoreStatus", new[]
        {
            typeof(string)
        })]
        public static void CvsExit_ExitSceneRestoreStatus(string strInput, CvsExit __instance)
        {
            BoneControllerMgr.Instance.OnCustomSceneExitWithSave();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LiveCharaSelectSprite), "Start")]
        public static void LiveCharaSelectSprite_StartPostHook(LiveCharaSelectSprite __instance)
        {
            var button = _idolBackButton?.GetValue(__instance) as Button;
            button?.onClick.AddListener(BoneControllerMgr.Instance.SetNeedReload);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.NPCLoadAll))]
        public static void ActionScene_NPCLoadAllPreHook()
        {
            // Force reload when going to next day in school
            // It's needed because after 1st day since loading the characters are reset but not reloaded, and BoneController stops working
            BoneControllerMgr.Instance.SetNeedReload();
        }

        // Prevent reading ABM data when loading the list of characters
        [HarmonyPrefix, HarmonyPatch(typeof(CustomCharaFile), "Initialize")]
        public static void CustomScenePrefix()
        {
            MakerListIsLoading = true;
        }

        private static bool MakerListIsLoading;

        // Prevent reading ABM data when loading the list of characters
        [HarmonyPostfix, HarmonyPatch(typeof(CustomCharaFile), "Initialize")]
        public static void CustomScenePostfix()
        {
            MakerListIsLoading = false;
        }
    }
}