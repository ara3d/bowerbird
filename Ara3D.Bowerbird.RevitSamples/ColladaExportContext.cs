 using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Ara3D.Utils;
using Autodesk.Revit.DB;

namespace RevitExporter
{
    class ColladaExportContext : IExportContext
    {
        private readonly Document exportedDocument = null;

        public uint CurrentPolymeshIndex { get; set; }

        public ElementId CurrentElementId =>
            (elementStack.Count > 0)
                ? elementStack.Peek()
                : ElementId.InvalidElementId;

        public Element CurrentElement =>
            exportedDocument.GetElement(
                CurrentElementId);

        public Transform CurrentTransform => transformationStack.Peek();

        private readonly bool isCancelled = false;
        private readonly Stack<ElementId> elementStack = new Stack<ElementId>();
        private readonly Stack<Transform> transformationStack = new Stack<Transform>();
        private ElementId currentMaterialId = ElementId.InvalidElementId;
        private StreamWriter streamWriter = null;

        readonly Dictionary<uint, ElementId> polymeshToMaterialId = new Dictionary<uint, ElementId>();

        public ColladaExportContext(Document document, StreamWriter writer)
        {
            streamWriter = writer;
            exportedDocument = document;
            transformationStack.Push(Transform.Identity);
        }

        public bool Start()
        {
            CurrentPolymeshIndex = 0;
            polymeshToMaterialId.Clear();

            WriteXmlColladaBegin();
            WriteXmlAsset();
            WriteXmlLibraryGeometriesBegin();

            return true;
        }

        public void Finish()
        {
            WriteXmlLibraryGeometriesEnd();

            WriteXmlLibraryMaterials();
            WriteXmlLibraryEffects();
            WriteXmlLibraryVisualScenes();
            WriteXmlColladaEnd();

            streamWriter.Close();
        }

        private void WriteXmlColladaBegin()
        {
            streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            streamWriter.Write("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\">\n");
        }

        private void WriteXmlColladaEnd()
        {
            streamWriter.Write("</COLLADA>\n");
        }

        private void WriteXmlAsset()
        {
            streamWriter.Write("<asset>\n");
            streamWriter.Write("<contributor>\n");
            streamWriter.Write("  <authoring_tool>Lumion - Revit COLLADA exporter</authoring_tool>\n");
            streamWriter.Write("</contributor>\n");
            streamWriter.Write($"<created>{DateTime.Now}</created>\n");

            //Units
            streamWriter.Write("<unit name=\"meter\" meter=\"1.00\"/>\n");
            streamWriter.Write("<up_axis>Z_UP</up_axis>\n");
            streamWriter.Write("</asset>\n");
        }

        private void WriteXmlLibraryGeometriesBegin()
        {
            streamWriter.Write("<library_geometries>\n");
        }

        private void WriteXmlLibraryGeometriesEnd()
        {
            streamWriter.Write("</library_geometries>\n");
        }

        public void OnPolymesh(PolymeshTopology polymesh)
        {
            CurrentPolymeshIndex++;

            WriteXmlGeometryBegin();
            WriteXmlGeometrySourcePositions(polymesh);
            WriteXmlGeometrySourceNormals(polymesh);
            if (polymesh.NumberOfUVs > 0)
                WriteXmlGeometrySourceMap(polymesh);

            WriteXmlGeometryVertices();

            if (polymesh.NumberOfUVs > 0)
                WriteXmlGeometryTrianglesWithMap(polymesh);
            else
                WriteXmlGeometryTrianglesWithoutMap(polymesh);

            WriteXmlGeometryEnd();

            polymeshToMaterialId.Add(CurrentPolymeshIndex, currentMaterialId);
        }

        private void WriteXmlGeometryBegin()
        {
            streamWriter.Write($"<geometry id=\"geom-{CurrentPolymeshIndex}\" name=\"{GetCurrentElementName()}\">\n");
            streamWriter.Write("<mesh>\n");
        }

        private string GetCurrentElementName()
        {
            var element = CurrentElement;
            if (element != null)
                return element.Name;

            return ""; //default name
        }

        private void WriteXmlGeometryEnd()
        {
            streamWriter.Write("</mesh>\n");
            streamWriter.Write("</geometry>\n");
        }

        private void WriteXmlGeometrySourcePositions(PolymeshTopology polymesh)
        {
            streamWriter.Write($"<source id=\"geom-{CurrentPolymeshIndex}-positions\">\n");
            streamWriter.Write(
                $"<float_array id=\"geom-{CurrentPolymeshIndex}-positions-array\" count=\"{(polymesh.NumberOfPoints * 3)}\">\n");

            XYZ point;
            var currentTransform = transformationStack.Peek();

            for (var iPoint = 0; iPoint < polymesh.NumberOfPoints; ++iPoint)
            {
                point = polymesh.GetPoint(iPoint);
                point = currentTransform.OfPoint(point);
                streamWriter.Write("{0:0.0000} {1:0.0000} {2:0.0000}\n", point.X, point.Y, point.Z);
            }

            streamWriter.Write("</float_array>\n");
            streamWriter.Write("<technique_common>\n");
            streamWriter.Write(
                $"<accessor source=\"#geom-{CurrentPolymeshIndex}-positions-array\" count=\"{polymesh.NumberOfPoints}\" stride=\"3\">\n");
            streamWriter.Write("<param name=\"X\" type=\"float\"/>\n");
            streamWriter.Write("<param name=\"Y\" type=\"float\"/>\n");
            streamWriter.Write("<param name=\"Z\" type=\"float\"/>\n");
            streamWriter.Write("</accessor>\n");
            streamWriter.Write("</technique_common>\n");
            streamWriter.Write("</source>\n");
        }

        private void WriteXmlGeometrySourceNormals(PolymeshTopology polymesh)
        {
            var nNormals = 0;

            switch (polymesh.DistributionOfNormals)
            {
                case DistributionOfNormals.AtEachPoint:
                    nNormals = polymesh.NumberOfPoints;
                    break;
                case DistributionOfNormals.OnePerFace:
                    nNormals = 1;
                    break;
                case DistributionOfNormals.OnEachFacet:
                    //TODO : DistributionOfNormals.OnEachFacet
                    nNormals = 1;
                    break;
            }

            streamWriter.Write($"<source id=\"geom-{CurrentPolymeshIndex}-normals\">\n");
            streamWriter.Write(
                $"<float_array id=\"geom-{CurrentPolymeshIndex}-normals-array\" count=\"{(nNormals * 3)}\">\n");

            XYZ point;
            var currentTransform = transformationStack.Peek();

            for (var iNormal = 0; iNormal < nNormals; ++iNormal)
            {
                point = polymesh.GetNormal(iNormal);
                point = currentTransform.OfVector(point);
                streamWriter.Write("{0:0.0000} {1:0.0000} {2:0.0000}\n", point.X, point.Y, point.Z);
            }

            streamWriter.Write("</float_array>\n");
            streamWriter.Write("<technique_common>\n");
            streamWriter.Write(
                $"<accessor source=\"#geom-{CurrentPolymeshIndex}-normals-array\" count=\"{nNormals}\" stride=\"3\">\n");
            streamWriter.Write("<param name=\"X\" type=\"float\"/>\n");
            streamWriter.Write("<param name=\"Y\" type=\"float\"/>\n");
            streamWriter.Write("<param name=\"Z\" type=\"float\"/>\n");
            streamWriter.Write("</accessor>\n");
            streamWriter.Write("</technique_common>\n");
            streamWriter.Write("</source>\n");
        }

        private void WriteXmlGeometrySourceMap(PolymeshTopology polymesh)
        {
            streamWriter.Write($"<source id=\"geom-{CurrentPolymeshIndex}-map\">\n");
            streamWriter.Write(
                $"<float_array id=\"geom-{CurrentPolymeshIndex}-map-array\" count=\"{(polymesh.NumberOfUVs * 2)}\">\n");

            UV uv;

            for (var iUv = 0; iUv < polymesh.NumberOfUVs; ++iUv)
            {
                uv = polymesh.GetUV(iUv);
                streamWriter.Write("{0:0.0000} {1:0.0000}\n", uv.U, uv.V);
            }

            streamWriter.Write("</float_array>\n");
            streamWriter.Write("<technique_common>\n");
            streamWriter.Write(
                $"<accessor source=\"#geom-{CurrentPolymeshIndex}-map-array\" count=\"{polymesh.NumberOfPoints}\" stride=\"2\">\n");
            streamWriter.Write("<param name=\"S\" type=\"float\"/>\n");
            streamWriter.Write("<param name=\"T\" type=\"float\"/>\n");
            streamWriter.Write("</accessor>\n");
            streamWriter.Write("</technique_common>\n");
            streamWriter.Write("</source>\n");
        }

        private void WriteXmlGeometryVertices()
        {
            streamWriter.Write($"<vertices id=\"geom-{CurrentPolymeshIndex}-vertices\">\n");
            streamWriter.Write($"<input semantic=\"POSITION\" source=\"#geom-{CurrentPolymeshIndex}-positions\"/>\n");
            streamWriter.Write("</vertices>\n");
        }

        private void WriteXmlGeometryTrianglesWithoutMap(PolymeshTopology polymesh)
        {
            streamWriter.Write($"<triangles count=\"{polymesh.NumberOfFacets}\"");
            if (IsMaterialValid(currentMaterialId))
                streamWriter.Write($" material=\"material-{currentMaterialId}\"");
            streamWriter.Write(">\n");
            streamWriter.Write(
                $"<input offset=\"0\" semantic=\"VERTEX\" source=\"#geom-{CurrentPolymeshIndex}-vertices\"/>\n");
            streamWriter.Write(
                $"<input offset=\"1\" semantic=\"NORMAL\" source=\"#geom-{CurrentPolymeshIndex}-normals\"/>\n");
            streamWriter.Write("<p>\n");
            PolymeshFacet facet;

            switch (polymesh.DistributionOfNormals)
            {
                case DistributionOfNormals.AtEachPoint:
                    for (var i = 0; i < polymesh.NumberOfFacets; ++i)
                    {
                        facet = polymesh.GetFacet(i);
                        streamWriter.Write($"{facet.V1} {facet.V1} {facet.V2} {facet.V2} {facet.V3} {facet.V3} \n");
                    }

                    break;

                case DistributionOfNormals.OnEachFacet:
                //TODO : DistributionOfNormals.OnEachFacet
                case DistributionOfNormals.OnePerFace:
                    for (var i = 0; i < polymesh.NumberOfFacets; ++i)
                    {
                        facet = polymesh.GetFacet(i);
                        streamWriter.Write($"{facet.V1} 0 {facet.V2} 0 {facet.V3} 0 \n");
                    }

                    break;

            }

            streamWriter.Write("</p>\n");
            streamWriter.Write("</triangles>\n");
        }

        private void WriteXmlGeometryTrianglesWithMap(PolymeshTopology polymesh)
        {
            streamWriter.Write($"<triangles count=\"{polymesh.NumberOfFacets}\"");
            if (IsMaterialValid(currentMaterialId))
                streamWriter.Write($" material=\"material-{currentMaterialId}\"");
            streamWriter.Write(">\n");
            streamWriter.Write(
                $"<input offset=\"0\" semantic=\"VERTEX\" source=\"#geom-{CurrentPolymeshIndex}-vertices\"/>\n");
            streamWriter.Write(
                $"<input offset=\"1\" semantic=\"NORMAL\" source=\"#geom-{CurrentPolymeshIndex}-normals\"/>\n");
            streamWriter.Write(
                $"<input offset=\"2\" semantic=\"TEXCOORD\" source=\"#geom-{CurrentPolymeshIndex}-map\" set=\"0\"/>\n");
            streamWriter.Write("<p>\n");
            PolymeshFacet facet;

            switch (polymesh.DistributionOfNormals)
            {
                case DistributionOfNormals.AtEachPoint:
                    for (var i = 0; i < polymesh.NumberOfFacets; ++i)
                    {
                        facet = polymesh.GetFacet(i);
                        streamWriter.Write(
                            $"{facet.V1} {facet.V1} {facet.V1} {facet.V2} {facet.V2} {facet.V2} {facet.V3} {facet.V3} {facet.V3} \n");
                    }

                    break;

                case DistributionOfNormals.OnEachFacet:
                //TODO : DistributionOfNormals.OnEachFacet
                case DistributionOfNormals.OnePerFace:
                    for (var i = 0; i < polymesh.NumberOfFacets; ++i)
                    {
                        facet = polymesh.GetFacet(i);
                        streamWriter.Write(
                            $"{facet.V1} 0 {facet.V1} {facet.V2} 0 {facet.V2} {facet.V3} 0 {facet.V3} \n");
                    }

                    break;
            }

            streamWriter.Write("</p>\n");
            streamWriter.Write("</triangles>\n");
        }

        public void OnMaterial(MaterialNode node)
        {
            // OnMaterial method can be invoked for every single out-coming mesh
            // even when the material has not actually changed. Thus it is usually
            // beneficial to store the current material and only get its attributes
            // when the material actually changes.

            currentMaterialId = node.MaterialId;
        }

        private void WriteXmlLibraryMaterials()
        {
            streamWriter.Write("<library_materials>\n");

            foreach (var materialId in polymeshToMaterialId.Values.Distinct())
            {
                if (IsMaterialValid(materialId) == false)
                    continue;

                streamWriter.Write($"<material id=\"material-{materialId}\" name=\"{GetMaterialName(materialId)}\">\n");
                streamWriter.Write($"<instance_effect url=\"#effect-{materialId}\" />\n");
                streamWriter.Write("</material>\n");
            }

            streamWriter.Write("</library_materials>\n");
        }

        private string GetMaterialName(ElementId materialId)
        {
            var material = exportedDocument.GetElement(materialId) as Material;
            if (material != null)
                return material.Name;

            return ""; //default material name
        }

        private bool IsMaterialValid(ElementId materialId)
        {
            var material = exportedDocument.GetElement(materialId) as Material;
            if (material != null)
                return true;

            return false;
        }

        private void WriteXmlLibraryEffects()
        {
            streamWriter.Write("<library_effects>\n");

            foreach (var materialId in polymeshToMaterialId.Values.Distinct())
            {
                if (IsMaterialValid(materialId) == false)
                    continue;

                var material = exportedDocument.GetElement(materialId) as Material;

                streamWriter.Write($"<effect id=\"effect-{materialId}\" name=\"{GetMaterialName(materialId)}\">\n");
                streamWriter.Write("<profile_COMMON>\n");

                streamWriter.Write("<technique sid=\"common\">\n");
                streamWriter.Write("<phong>\n");
                streamWriter.Write("<ambient>\n");
                streamWriter.Write("<color>0.1 0.1 0.1 1.000000</color>\n");
                streamWriter.Write("</ambient>\n");


                //diffuse
                streamWriter.Write("<diffuse>\n");
                streamWriter.Write(
                    $"<color>{material.Color.Red} {material.Color.Green} {material.Color.Blue} 1.0</color>\n");
                streamWriter.Write("</diffuse>\n");


                streamWriter.Write("<specular>\n");
                streamWriter.Write("<color>1.000000 1.000000 1.000000 1.000000</color>\n");
                streamWriter.Write("</specular>\n");

                streamWriter.Write("<shininess>\n");
                streamWriter.Write($"<float>{material.Shininess}</float>\n");
                streamWriter.Write("</shininess>\n");

                streamWriter.Write("<reflective>\n");
                streamWriter.Write("<color>0 0 0 1.000000</color>\n");
                streamWriter.Write("</reflective>\n");
                streamWriter.Write("<reflectivity>\n");
                streamWriter.Write("<float>1.000000</float>\n");
                streamWriter.Write("</reflectivity>\n");

                streamWriter.Write("<transparent opaque=\"RGB_ZERO\">\n");
                streamWriter.Write("<color>1.000000 1.000000 1.000000 1.000000</color>\n");
                streamWriter.Write("</transparent>\n");

                streamWriter.Write("<transparency>\n");
                streamWriter.Write($"<float>{material.Transparency}</float>\n");
                streamWriter.Write("</transparency>\n");

                streamWriter.Write("</phong>\n");
                streamWriter.Write("</technique>\n");


                streamWriter.Write("</profile_COMMON>\n");
                streamWriter.Write("</effect>\n");
            }

            streamWriter.Write("</library_effects>\n");
        }

        public void WriteXmlLibraryVisualScenes()
        {
            streamWriter.Write("<library_visual_scenes>\n");
            streamWriter.Write("<visual_scene id=\"Revit_project\">\n");

            foreach (var pair in polymeshToMaterialId)
            {
                streamWriter.Write($"<node id=\"node-{pair.Key}\" name=\"elementName\">\n");
                streamWriter.Write($"<instance_geometry url=\"#geom-{pair.Key}\">\n");
                if (IsMaterialValid(pair.Value))
                {
                    streamWriter.Write("<bind_material>\n");
                    streamWriter.Write("<technique_common>\n");
                    streamWriter.Write(
                        $"<instance_material target=\"#material-{pair.Value}\" symbol=\"material-{pair.Value}\" >\n");
                    streamWriter.Write("</instance_material>\n");
                    streamWriter.Write("</technique_common>\n");
                    streamWriter.Write("</bind_material>\n");
                }

                streamWriter.Write("</instance_geometry>\n");
                streamWriter.Write("</node>\n");
            }

            streamWriter.Write("</visual_scene>\n");
            streamWriter.Write("</library_visual_scenes>\n");

            streamWriter.Write("<scene>\n");
            streamWriter.Write("<instance_visual_scene url=\"#Revit_project\"/>\n");
            streamWriter.Write("</scene>\n");
        }

        public bool IsCanceled()
        {
            // This method is invoked many times during the export process.
            return isCancelled;
        }

        public void OnRPC(RPCNode node)
        {
        }

        public RenderNodeAction OnViewBegin(ViewNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnViewEnd(ElementId elementId)
        {
            // Note: This method is invoked even for a view that was skipped.
        }

        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            elementStack.Push(elementId);

            return RenderNodeAction.Proceed;
        }

        public void OnElementEnd(ElementId elementId)
        {
            // Note: this method is invoked even for elements that were skipped.
            elementStack.Pop();
        }

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            // This method is invoked only if the custom exporter was set to include faces.
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node)
        {
            // This method is invoked only if the custom exporter was set to include faces.
            // Note: This method is invoked even for faces that were skipped.
        }

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            // This method marks the start of processing a family instance
            transformationStack.Push(transformationStack.Peek().Multiply(node.GetTransform()));

            // We can either skip this instance or proceed with rendering it.
            return RenderNodeAction.Proceed;
        }

        public void OnInstanceEnd(InstanceNode node)
        {
            // Note: This method is invoked even for instances that were skipped.
            transformationStack.Pop();
        }

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            transformationStack.Push(transformationStack.Peek().Multiply(node.GetTransform()));
            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node)
        {
            // Note: This method is invoked even for instances that were skipped.
            transformationStack.Pop();
        }

        public void OnLight(LightNode node)
        {
        }
    }
}
