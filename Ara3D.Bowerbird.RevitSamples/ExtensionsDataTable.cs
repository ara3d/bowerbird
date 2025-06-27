using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Ara3D.Bowerbird.RevitSamples
{
    public static class ExtensionsDataTable
    {
        public static Type GetUnderlyingType(this Type type)
            => Nullable.GetUnderlyingType(type) ?? type;

        public static Type GetValueType(this MemberInfo mi)
        {
            if (mi is FieldInfo f) return f.FieldType.GetUnderlyingType();
            if (mi is PropertyInfo p) return p.PropertyType.GetUnderlyingType();
            if (mi is MethodInfo m) return m.ReturnType.GetUnderlyingType();
            throw new Exception("Not invokable member");
        }

        public static object GetValue(this MemberInfo mi, object obj)
        {
            if (mi is FieldInfo f) return f.GetValue(obj);
            if (mi is PropertyInfo p) return p.GetValue(obj);
            if (mi is MethodInfo m) return m.Invoke(obj, Array.Empty<object>());
            throw new Exception("Not invokable member");
        }

        public static bool CanGetValue(this MemberInfo mi)
            => mi is FieldInfo 
               || (mi is PropertyInfo pi && pi.CanRead && pi.GetIndexParameters().Length == 0) 
               || (mi is MethodInfo method && method.GetParameters().Length == 0);

        public static DataTableBuilder BuildDataTable(this Type self, DataTableBuilder.DataTableBuilderOptions options = null)
            => new DataTableBuilder(self, options);

        public static System.Data.DataTable ToDataTable<T>(this IEnumerable<T> self, DataTableBuilder.DataTableBuilderOptions options = null)
            => BuildDataTable(typeof(T), options).AddRows(self).DataTable;
    }
}