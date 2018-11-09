﻿namespace MakerAPI
{
    public sealed class MakerCategory
    {
        private bool Equals(MakerCategory other)
        {
            return string.Equals(CategoryName, other.CategoryName) && string.Equals(SubCategoryName, other.SubCategoryName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MakerCategory && Equals((MakerCategory) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((CategoryName != null ? CategoryName.GetHashCode() : 0) * 397) ^ (SubCategoryName != null ? SubCategoryName.GetHashCode() : 0);
            }
        }

        public MakerCategory(string categoryName, string subCategoryName)
        {
            CategoryName = categoryName;
            SubCategoryName = subCategoryName;
        }

        public string CategoryName { get; }
        public string SubCategoryName { get; }

        public override string ToString()
        {
            return $"{CategoryName} / {SubCategoryName}";
        }
    }
}