using System.Collections.Generic;
using Ara3D.Utils;
using Plato.DoublePrecision;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class SMComponent
    {
        // Where is this going to be documented I wonder. 
        public string ComponentType;

        // What is the format of this? I suggest a URI for it. 
        public string AuthorIdentifier = "";
        public string Context = ""; // NOTE : This is a filepath 
        public string Function = "instance"; // ??
        public string Includes = ""; // ??

        // What is the time-zone? I recommend: ISO 8601 format.
        // https://en.wikipedia.org/wiki/ISO_8601
        public string DateCreated = "";
        public string LastModified = "20240219143423";

        public string Name = "CUP";

        public string ComponentClassification; 
        public string EntityGUID;
        public string ComponentGUID;
        public string ComponentVersionGUID;

        // What are the valid options here? It would be nice if there was a registry somewhere of known payload data types.
        // The identification system, would make sense as some kind of URI. 
        public string PayloadDataType;

        // What does this mean? It doesn't seem necessary 
        public string ResponseToComponent;

        // Do we really want to support multiple hash definitions? 
        // A programmer would have to know what the valid options are, and write algorithms for them all.
        public string HashDefinition = "MD5";

        // Even knowing the algorithm, if the data is encoded as JSON, whitespace should not change meaning, but it will change the hash.
        // I don't think I would hash the payload as part of the header. 
        public string PayloadHash;
        public string ComponentHash = "";
    }

    // Which of these fields are optional. 
    public class GeoJson : SMComponent
    {
        public GeoPayload PayloadGeoJson = new GeoPayload();
        public GeoPayload Doors = new GeoPayload();
        public GeoPayload Openings = new GeoPayload();

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
        public List<List<List<double>>> Coordinates = new List<List<List<double>>>();
    }

    //==

    public class BuildingLayout
    {
        public Dictionary<int, RoomLayout> Rooms = new Dictionary<int, RoomLayout>();
        public Dictionary<int, DoorLayout> Doors = new Dictionary<int, DoorLayout>();
        public Dictionary<int, LevelLayout> Levels = new Dictionary<int, LevelLayout>();
    }

    public class LevelLayout
    {
        public int Id;
        public string Name;
        public double Elevation;
        public override string ToString() => $"{Name} - {Id}";
    }

    public class RoomLayout
    {
        public int Id;
        public string Name;
        public int Level;
        public Bounds3D Bounds;
        public override string ToString() => $"{Name} - {Id}";
    }

    public class DoorLayout
    {
        public int Id;
        public string Name;
        public int Level;
        public int FromRoom;
        public int ToRoom;
        public Bounds3D Bounds;
        public override string ToString() => $"{Name} - {Id}";
    }
}