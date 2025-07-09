using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Autodesk.Revit.DB;

namespace Ara3D.Bowerbird.RevitSamples;

public static class CategoryExtensions
{
    public static IReadOnlyList<Category> GetAllCategories(
        this Document rootDoc,
        bool includeSubCategories = true)
    {
        if (rootDoc == null) throw new ArgumentNullException(nameof(rootDoc));

        var result = new List<Category>();

        void AddCategoryTree(Category cat)
        {
            if (cat == null) return;
            result.Add(cat);

            if (!includeSubCategories) return;

            foreach (Category sub in cat.SubCategories ?? [])
                AddCategoryTree(sub);
        }

        foreach (Category cat in rootDoc.Settings.Categories)
            AddCategoryTree(cat);
        return result;
    }
}