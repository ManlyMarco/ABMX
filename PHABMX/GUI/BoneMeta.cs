using KKAPI.Maker;

namespace KKABMX.GUI
{
    public sealed class BoneMeta
    {
        private float? _lMax;
        private string _lDisplayName;
        private string _xDisplayName;
        private string _yDisplayName;
        private string _zDisplayName;

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
        public bool UniquePerCoordinate { get; set; } = false;

        public bool X { get; set; } = true;
        public bool Y { get; set; } = true;
        public bool Z { get; set; } = true;
        public bool L { get; set; } = false;

        public float LMax
        {
            get => _lMax ?? Max;
            set => _lMax = value;
        }

        public float LMin { get; set; } = 0.1f;

        public string LDisplayName
        {
            get => _lDisplayName ?? DisplayName + " Length";
            set => _lDisplayName = value;
        }

        public string XDisplayName
        {
            get => _xDisplayName ?? $"{DisplayName}{XYZPostfix} X";
            set => _xDisplayName = value;
        }

        public string YDisplayName
        {
            get => _yDisplayName ?? $"{DisplayName}{XYZPostfix} Y";
            set => _yDisplayName = value;
        }

        public string ZDisplayName
        {
            get => _zDisplayName ?? $"{DisplayName}{XYZPostfix} Z";
            set => _zDisplayName = value;
        }

        public string XYZPostfix { get; set; } = " Scale";
    }
}
