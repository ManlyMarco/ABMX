using MakerAPI;

namespace KKABMX.GUI
{
    public sealed class BoneMeta
    {
        public static BoneMeta Separator(MakerCategory category)
        {
            return new BoneMeta(null, null, 0, 0, category);
        }

        public bool IsSeparator => BoneName == null;

        public BoneMeta(string boneName, string displayName, float min, float max, MakerCategory category, string rightBoneName = null)
        {
            BoneName = boneName;
            DisplayName = displayName;
            Min = min;
            Max = max;
            Category = category;
            RightBoneName = rightBoneName;
        }

        public string BoneName { get; }
        public string RightBoneName { get; }
        public string DisplayName { get; }
        public float Min { get; }
        public float Max { get; }
        public MakerCategory Category { get; }
    }
}