using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using Harmony;
using KKAPI.Chara;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "KKABMX Core", Version)]
    [BepInDependency("com.bepis.bepinex.extendedsave")]
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = Metadata.Version;
        public const string GUID = Metadata.GUID;
        public const string ExtDataGUID = Metadata.ExtDataGUID;

        private void Start()
        {
            if(!KKAPI.KoikatuAPI.CheckRequiredPlugin(this, KKAPI.KoikatuAPI.GUID, new Version(KKAPI.KoikatuAPI.VersionConst)))
                return;

            if (File.Exists(Path.Combine(Paths.PluginPath, "KKABMPlugin.dll")) || File.Exists(Path.Combine(Paths.PluginPath, "KKABMGUI.dll")))
            {
                Log(LogLevel.Message | LogLevel.Error, "Old version of ABM found! Remove KKABMPlugin.dll and KKABMGUI.dll and restart the game.");
                return;
            }

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            Hooks.Init();
        }

        public static void Log(LogLevel logLevel, string text)
        {
            Logger.Log(logLevel, text);
        }
    }
}
