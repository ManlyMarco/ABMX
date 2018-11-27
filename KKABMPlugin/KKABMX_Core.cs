using BepInEx;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "KKABMX Core", Version)]
    [BepInDependency("com.bepis.bepinex.extendedsave")]
    public class KKABMX_Core : BaseUnityPlugin
    {
        public const string Version = "2.1";
        public const string GUID = "KKABMX.Core";

        public KKABMX_Core()
        {
            Hooks.InstallHook();
        }

        protected void Start()
        {
            BoneControllerMgr.Init();
            BoneControllerMgr.Instance.transform.SetParent(transform);
        }
    }
}