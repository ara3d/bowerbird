using System.Collections.Generic;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class SMComponent
    {
        // Where is this going to be documented I wonder. 
        public string ComponentType;

        // What is the hash algorithm of the component? How is it computed? Why is it necessary? 
        public string ComponentHash = "";

        // What is the format of this? I suggest a URI for it. 
        public string AuthorIdentifier = "";

        public string Context = ""; // NOTE : This is a filepath 
        public string Function = "instance"; // ??
        public string Includes = ""; // ??

        // What is the time-zone? I recommend: ISO 8601 format.
        // https://en.wikipedia.org/wiki/ISO_8601
        public string DateCreated = "";
        public string LastModified = "20240219143423";

        // What does this mean? 
        public string Name = "CUP";

        public string ComponentClassification; 
        public string EntityGUID;
        public string ComponentGUID;
        public string ComponentVersionGUID;

        // What are the valid options here? It would be nice if there was a registry somewhere of known payload data types.
        // THe identification system, would make sense as some kind of URI. 
        public string PayloadDataType;

        // What does this mean? 
        public string ResponseToComponent;

        // Do we really want to support multiple hash definitions? 
        // A programmer would have to know what the valid options are, and write algorithms for them all.
        public string HashDefinition = "MD5";

        // Even knowing the algorithm, if the data is encoded as JSON, whitespace should not change meaning, but it will change the hash.
        public string PayloadHash;
    }

    // Which of these fields are optional. 
    public class GeoJson : SMComponent
    {
        // This name would vary depending on the 
        public GeoPayload PayloadGeoJson;

        public GeoJson()
        {
            ComponentClassification = "hok.room.geometry";
            PayloadDataType = "geojson";
            ComponentType = "geojson.room.net";
        }
    }

    public class GeoPayload
    {
        public string Type = "Polygon";

        // Technically this looks like a list of polygons. 
        // This level of nesting is very error-prone.
        // I recommend defining a polygon type, etc. 
        public List<List<List<double>>> Coordinates;
    }

}