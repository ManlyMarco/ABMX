using BepInEx;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "KKABMX Core", "1.0")]
    [BepInDependency("com.bepis.bepinex.extendedsave")]
    public class KKABMX_Core : BaseUnityPlugin
    {
        public const string GUID = "KKABMX.Core";

        public KKABMX_Core()
        {
            Hooks.InstallHook();
        }

        protected void Start()
        {
            BoneControllerMgr.Init();
        }
    }
}