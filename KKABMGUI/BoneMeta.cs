namespace KKABMX.GUI
{
    public sealed class BoneMeta
    {
        public static BoneMeta Separator(string category, string subCategory)
        {
            return new BoneMeta(null, null, 0, 0, category, subCategory);
        }

        public bool IsSeparator => BoneName == null;

        public BoneMeta(string boneName, string displayName, float min, float max, string category, string subCategory, string rightBoneName = null)
        {
            BoneName = boneName;
            DisplayName = displayName;
            Min = min;
            Max = max;
            Category = category;
            SubCategory = subCategory;
            RightBoneName = rightBoneName;
        }

        public string BoneName { get; }
        public string RightBoneName { get; }
        public string DisplayName { get; }
        public float Min { get; }
        public float Max { get; }
        public string Category { get; }
        public string SubCategory { get; }
    }
}