using System;
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

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            HarmonyInstance.Create(GUID).PatchAll(typeof(Hooks));
        }

        public static void Log(LogLevel logLevel, string text)
        {
            Logger.Log(logLevel, text);
        }
    }
}
