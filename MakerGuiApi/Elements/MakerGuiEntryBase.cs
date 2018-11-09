using System;
using UnityEngine;

namespace MakerAPI
{
    public abstract class MakerGuiEntryBase : IDisposable
    {
        public static readonly string GuiApiNameAppendix = "(GUIAPI)";

        protected MakerGuiEntryBase(MakerCategory category)
        {
            Category = category;
        }

        public MakerCategory Category { get; }
        protected internal abstract void CreateControl(Transform subCategoryList);

        public abstract void Dispose();
    }
}