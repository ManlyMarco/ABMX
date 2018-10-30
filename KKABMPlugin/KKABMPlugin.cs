using BepInEx;

namespace KKABMPlugin
{
    [BepInPlugin("48DBE560-CFBA-45E4-B348-F5246F475D04", "KKABMPlugin", "0.7.0")]
    [BepInDependency("com.bepis.bepinex.extendedsave")]
    public class KKABMPlugin : BaseUnityPlugin
    {
        public KKABMPlugin()
        {
            Hook.InstallHook();
        }

        protected void Start()
        {
            BoneControllerMgr.Init();
        }
    }
}