namespace KKABMX
{
    internal static class Metadata
    {
        public const string Version = "4.1";
        public const string Name = "PHABMX (BonemodX)";
        public const string GUID = "KKABMX.Core";
        public const string ExtDataGUID = "KKABMPlugin.ABMData";

        public const string XyzModeName = "Use XYZ scale sliders";
        public const string XyzModeDesc = "When enabled, all scale sliders in maker are split into XYZ sliders (one for each direction). " +
                                               "Cards made with this option will automatically enable it for relevant sliders.\n\n" +
                                               "Note that using uneven scaling on some parts can skew the model in some animations, especially in H mode.\n\n" +
                                               "The setting takes effect immediately while inside maker.";

        public const string RaiseLimitsName = "Increase slider limits 2x";
        public const string RaiseLimitsDesc = "Maximum values of all sliders in maker are increased twofold. Can cause even more horrifying results. " +
                                              "Recommended to only enable when working on furries and superdeformed charas.\n\n" +
                                              "The setting takes effect after maker restart.";

        public const string AdvTransparencyName = "Make advanced window transparent";
        public const string AdvTransparencyDesc = "If false, the window has a solid background, else it's see-through";
    }
}
