using UnityEngine;

namespace MakerAPI
{
    public class MakerSeparator : MakerGuiEntryBase
    {
        private static Transform _sourceSeparator;
        
        protected internal override void CreateControl(Transform subCategoryList)
        {
            var s = Object.Instantiate(SourceSeparator, subCategoryList, false);
            s.name = "Separate" + GuiApiNameAppendix;
        }

        public override void Dispose()
        {
        }

        private static Transform SourceSeparator
        {
            get
            {
                if (_sourceSeparator == null)
                {
                    // Exists in male and female maker
                    // CustomScene /CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglAll/AllTop/Separate
                    _sourceSeparator = GameObject.Find("00_FaceTop").transform.Find("tglAll").Find("AllTop").Find("Separate");
                }

                return _sourceSeparator;
            }
        }

        public MakerSeparator(MakerCategory category) : base(category)
        {
        }
    }
}