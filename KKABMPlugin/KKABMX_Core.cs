using BepInEx;

namespace KKABMX.Core
{
    [BepInPlugin(GUID, "KKABMX Core", Version)]
    [BepInDependency("com.bepis.bepinex.extendedsave")]
    public class KKABMX_Core : BaseUnityPlugin
    {
        internal const string Version = "2.3";
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