using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Ara3D.Bowerbird.RevitSamples;

public static class ObjectSpreader
{
    /// <summary>
    /// Merges each source into one ExpandoObject, copying:
    /// 1) IDictionary<string,object> entries (covers ExpandoObject & plain dictionaries)
    /// 2) public instance properties (covers anonymous types, POCOs, etc.)
    /// Later sources overwrite earlier ones on name conflict.
    /// </summary>
    public static ExpandoObject Spread(params object[] sources)
    {
        var result = new ExpandoObject();
        var dict = (IDictionary<string, object>)result;

        foreach (var src in sources)
        {
            if (src == null) continue;

            // 1) If it’s a dictionary, copy every kvp:
            if (src is IDictionary<string, object> srcDict)
            {
                foreach (var kvp in srcDict)
                    dict[kvp.Key] = kvp.Value;
            }
            else
            {
                // 2) Otherwise reflect over its public readable properties:
                var props = src.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.CanRead);

                foreach (var prop in props)
                    dict[prop.Name] = prop.GetValue(src);
            }
        }

        return result;
    }
}