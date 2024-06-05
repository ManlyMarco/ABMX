using BepInEx.Configuration;

namespace KKABMX
{
    internal static class Metadata
    {
        public const string Version = "5.2.2";
#if AI
        public const string Name = "AIABMX (BonemodX)";
#elif HS2
        public const string Name = "HS2ABMX (BonemodX)";
#elif KK
        public const string Name = "KKABMX (BonemodX)";
#elif EC
        public const string Name = "ECABMX (BonemodX)";
#elif KKS
        public const string Name = "KKSABMX (BonemodX)";
#endif
        public const string GUID = "KKABMX.Core";
        public const string ExtDataGUID = "KKABMPlugin.ABMData";
        
        public const string ShowSlidersName = "Add sliders to maker";
        public const string ShowSlidersDesc = "Expand maker customization options with new yellow sliders, including new categories.\n\n" +
                                              "This setting doesn't affect loading and using cards that use ABMX sliders. Advanced window can still be used as normal.\n\n" +
                                              "The setting takes effect after maker restart. Has no effect on advanced window.";

        public const string XyzModeName = "Use XYZ scale sliders";
        public const string XyzModeDesc = "When enabled, all yellow scale sliders in maker are split into XYZ sliders (one for each direction). " +
                                          "Cards made with this option will automatically enable it for relevant sliders.\n\n" +
                                          "Note that using uneven scaling on some parts can skew the model in some animations, especially in H mode.\n\n" +
                                          "The setting takes effect immediately while inside maker. Has no effect on advanced window.";

        public const string RaiseLimitsName = "Increase slider limits 2x";
        public const string RaiseLimitsDesc = "Maximum values of all yellow sliders in maker are increased twofold. Can cause even more horrifying results. " +
                                              "Recommended to only enable when working on furries and superdeformed charas.\n\n" +
                                              "The setting takes effect after maker restart. Has no effect on advanced window.";

        public const string ResetToLastLoadedName = "Reset button restores saved card value";
        public const string ResetToLastLoadedDesc = "Affects yellow bonemod sliders in maker. If false, the Reset button sets the value to 1 (old behaviour). If true, the Reset button restores the value that is saved in the loaded character card.";

        public const string OpenEditorKeyName = "Open bonemod editor";
        public const string OpenEditorKeyDesc = "Opens advanced bonemod window if there is a character that can be edited. Can be used at (almost) any time.";
    }
}
