using UnityEngine;

namespace MakerAPI
{
    public abstract class MakerGuiEntryBase
    {
        public static readonly string GuiApiNameAppendix = "(GUIAPI)";

        protected MakerGuiEntryBase(MakerCategory category)
        {
            Category = category;
        }

        public MakerCategory Category { get; }
        protected internal abstract void CreateControl(Transform subCategoryList);
    }
}