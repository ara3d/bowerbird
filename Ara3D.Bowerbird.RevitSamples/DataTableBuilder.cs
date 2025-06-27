using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using Type = System.Type;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class DataTableBuilder
    {
        public class DataTableBuilderOptions
        {
            public bool IncludeFields = true;
            public bool IncludeProps = true;
            public bool IncludeMethods = false;
            public bool PublicOnly = true;
            public bool DeclaredOnly = false;
        }

        public readonly Type Type;
        public readonly IReadOnlyList<MemberInfo> Members;
        public readonly DataTableBuilderOptions Options;
        public readonly System.Data.DataTable DataTable;

        public DataTableBuilder(Type type, DataTableBuilderOptions options = null)
        {
            Options = options ?? new DataTableBuilderOptions();
            Type = type;
            DataTable = new System.Data.DataTable(Type.Name);

            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (!Options.PublicOnly)
                bindingFlags |= BindingFlags.NonPublic;
            if (Options.DeclaredOnly)
                bindingFlags |= BindingFlags.DeclaredOnly;

            var properties = Options.IncludeProps ? Type.GetProperties(bindingFlags).Where(ExtensionsDataTable.CanGetValue).ToArray() : Array.Empty<PropertyInfo>();
            foreach (var property in properties)
                DataTable.Columns.Add(property.Name, property.GetValueType());

            var fields = Options.IncludeFields ? Type.GetFields(bindingFlags).ToArray() : Array.Empty<FieldInfo>();
            foreach (var field in fields)
                DataTable.Columns.Add(field.Name, field.GetValueType());

            var methods = Options.IncludeMethods ? Type.GetMethods(bindingFlags).Where(ExtensionsDataTable.CanGetValue).ToArray() : Array.Empty<MethodInfo>();
            foreach (var method in methods)
                DataTable.Columns.Add(method.Name, method.GetValueType());

            Members = properties.Cast<MemberInfo>().Concat(fields).Concat(methods).ToArray();
        }

        public DataTableBuilder AddRows(IEnumerable items)
        {
            foreach (var item in items)
            {
                var row = DataTable.NewRow();
                foreach (var member in Members)
                {
                    try
                    {
                        row[member.Name] = member.GetValue(item) ?? DBNull.Value;
                    }
                    catch (Exception e)
                    {
                        row.SetColumnError(member.Name, $"{member.Name}: {e.Message}");
                    }
                }
                DataTable.Rows.Add(row);
            }
            return this;
        }
    }

    // TODO: consider maybe making an Ara3D.Utils.Revit for these functions and more. 
}
