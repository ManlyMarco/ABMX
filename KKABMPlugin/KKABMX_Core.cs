using BepInEx;

namespace KKABMPlugin
{
    [BepInPlugin(GUID, "KKABMX", "1.0")]
    [BepInDependency("com.bepis.bepinex.extendedsave")]
    public class KKABMX_Core : BaseUnityPlugin
    {
        public const string GUID = "KKABMX.Core";
        public KKABMX_Core()
        {
            Hook.InstallHook();
        }

        protected void Start()
        {
            BoneControllerMgr.Init();
        }
    }
}