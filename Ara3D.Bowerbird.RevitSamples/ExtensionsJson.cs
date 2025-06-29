using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ara3D.Utils;
using Parameter = Autodesk.Revit.DB.Parameter;

namespace Ara3D.Bowerbird.RevitSamples
{
    public static class ExtensionsJson
    {
        public static void WriteSchedulesAsJson(this Document doc, DirectoryPath outputDir)
        {
            var baseName = Path.GetFileNameWithoutExtension(doc.PathName);
            foreach (var schedule in doc.GetSchedules())
            {
                var fileName = $"{baseName}-{schedule.Name}.json".ToValidFileName();
                schedule.WriteAsJson(outputDir.RelativeFile(fileName));
            }
        }

        public static Utils.FilePath WriteAsJson(this ViewSchedule schedule, Utils.FilePath outputFile)
            => outputFile.WriteJson(schedule.GetScheduleData());

        public static void SerializeAllElementsAsJson(this Document doc, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                SerializeAllElementsAsJson(doc, stream);
            }
        }

        public static void SerializeAllElementsAsJson(this Document doc, Stream stream)
        {
            var js = new JsonSerializer();
            using (var writer = new StreamWriter(stream))
            {
                js.Serialize(writer, GetAllElementsAsJson(doc));
            }
        }

        public static JObject ToJson(this Element elem)
        {
            // Basic element data
            var name = elem.Name;
            var elementId = elem.Id.Value;
            var categoryName = elem.Category?.Name;

            // Extract parameters
            var parametersObject = new JObject();
            foreach (Parameter param in elem.Parameters)
            {
                if (param != null && param.HasValue && param.Definition != null)
                {
                    var paramName = param.Definition.Name;
                    var paramValue = GetParameterValueAsString(param);
                    parametersObject[paramName] = paramValue;
                }
            }

            // Construct the JSON object for the element
            var r = new JObject
            {
                ["name"] = name,
                ["element_id"] = elementId,
                ["category_name"] = categoryName,
            };

            var loc = elem.Location;
            if (loc is LocationPoint lp)
            {
                var p = lp.Point;
                var positionObject = new JObject
                {
                    ["x"] = p.X,
                    ["y"] = p.Y,
                    ["z"] = p.Z,
                };
                r["position"] = positionObject;
            }

            // Add parameters if we have any
            if (parametersObject.Count > 0)
            {
                r["parameters"] = parametersObject;
            }

            return r;
        }

        public static Utils.FilePath WriteJson<T>(this Utils.FilePath path, T data)
            => path.WriteAllText(JsonConvert.SerializeObject(data, Formatting.Indented));

        public static JArray ToJson(this IEnumerable<Element> elements)
            => new JArray(elements.Select(ToJson));

        public static JArray GetAllElementsAsJson(this Document doc)
            => doc.GetElements().ToJson();

        /// <summary>
        /// Helper method to convert a Revit parameter value into a string.
        /// You can customize this method further based on parameter storage type.
        /// </summary>
        private static string GetParameterValueAsString(Parameter param)
        {
            try
            {
                switch (param.StorageType)
                {
                    case StorageType.String:
                        return param.AsString();
                    case StorageType.Integer:
                        return param.AsInteger().ToString();
                    case StorageType.Double:
                        // Convert from internal units to display units if desired.
                        // For simplicity, we just return the raw double.
                        return param.AsDouble().ToString();
                    case StorageType.ElementId:
                        var id = param.AsElementId();
                        return id.Value.ToString();
                    case StorageType.None:
                    default:
                        return param.AsValueString();
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
