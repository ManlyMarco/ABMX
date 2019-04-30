using System;
using BepInEx;
using BepInEx.Harmony;
using BepInEx.Logging;
using EC.Core.ExtensibleSaveFormat;
using KKAPI.Chara;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "KKABMX Core", Version)]
    [BepInDependency(ExtendedSave.GUID)]
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = Metadata.Version;
        public const string GUID = Metadata.GUID;
        public const string ExtDataGUID = Metadata.ExtDataGUID;

        private void Start()
        {
            _logsource = Logger;

            if (!KKAPI.KoikatuAPI.CheckRequiredPlugin(this, KKAPI.KoikatuAPI.GUID, new Version(KKAPI.KoikatuAPI.VersionConst)))
                return;

            CharacterApi.RegisterExtraBehaviour<BoneController>(ExtDataGUID);

            HarmonyWrapper.PatchAll(typeof(Hooks));
        }

        private static ManualLogSource _logsource;

        public static void Log(LogLevel level, string text)
        {
            _logsource.Log(level, text);
        }
    }
}
