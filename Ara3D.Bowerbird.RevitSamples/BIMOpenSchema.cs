using System.Collections.Generic;

namespace BIMOpenSchema;

/// <summary>
/// Contains all the BIM Data for a discipline or federated model .
/// 
/// Optimized for efficient loading into analytical tools as a set of Parquet files, or a DuckDB database.
/// Provides a simple and efficient standardized way to interact with BIM data from different tools,
/// without having to go through APIs, or ad-hoc representations.
///
/// It is optimized for space and load-times, not ease of queries.
///
/// A typical workflow would be to ingest this  into a database then to use SQL to
/// create denormalized (wide) tables depending on an end-user's specific use-case
/// and what data they are interested in.  
/// 
/// This expresses the schema as an object model that is
/// independent of any specific serialization format,
/// whether it is JSON, Parquet, CSV, SQLite, or something else.
///
/// When exporting to a database, each list corresponds to a table.
/// When exporting to parquet, each list corresponds to a parquet file.
///
/// This data structure can also be used directly in C# code as am efficient
/// in-memory data structure for code-based workflows.
/// </summary>
public class BIMData
{
    public List<ParameterDescriptor> Descriptors { get; set; } = [];
    public List<ParameterInt> IntegerParameters { get; set; } = [];
    public List<ParameterDouble> DoubleParameters { get; set; } = [];
    public List<ParameterString> StringParameters { get; set; } = [];
    public List<ParameterEntity> EntityParameters { get; set; } = [];
    public List<ParameterPoint> PointParameters { get; set; } = [];
    public List<Document> Documents { get; set; } = [];
    public List<Entity> Entities { get; set; } = [];
    public List<string> Strings { get; set; } = [];
    public List<Point> Points { get; set; } = [];
    public List<EntityRelation> Relations { get; set; } = [];
}

//==
// Enumerations used for indexing tables. Provides type-safety and convenience in code
//
// The choice of long is to provide future proofing. 

public enum EntityIndex : long { }
public enum PointIndex : long { }
public enum DocumentIndex : long { }
public enum DescriptorIndex : long { }
public enum StringIndex : long { }

//==
// Main data type 

/// <summary>
/// Corresponds roughly to an element in the Revit file.
/// Some items are associated with entities that are not expressly derived from Element (e.g., Document, 
/// </summary>
public record Entity(
    // ElementID in Revit, and Step Line # in IFC
    long LocalId,

    // UniqueID in Revit, and GlobalID in IFC (not stored in string table, because it is never duplicated)
    string GlobalId, 

    // The index of the document this entity was found int
    DocumentIndex Document,

    // The name of the entity 
    StringIndex Name,

    // The category of the entity
    StringIndex Category);

/// <summary>
/// Corresponds with a specific Revit or IFC file 
/// </summary>
public record Document(
    StringIndex Title,
    StringIndex Path);

/// <summary>
/// Represents 3D location data.
/// </summary>
public record Point(
    double X,
    double Y,
    double Z);

/// <summary>
/// Meta-information for understanding a parameter 
/// </summary>
public record ParameterDescriptor(
    StringIndex Name,
    StringIndex Units,
    StringIndex Group);

//==
// Parameter data 
//
// All parameter data is arranged in one of a set of EAV (Entity Attribute Value) tables.
// Each one designed for a specific type. 

/// <summary>
/// A 32-bit integer parameter value
/// </summary>
public record ParameterInt(
    EntityIndex Entity,
    DescriptorIndex Descriptor,
    int Value);

/// <summary>
/// A parameter value representing text
/// </summary>
public record ParameterString(
    EntityIndex Entity,
    DescriptorIndex Descriptor,
    StringIndex Value);

/// <summary>
/// A 64-bit precision floating point (decimal) numeric parameter value
/// </summary>
public record ParameterDouble(
    EntityIndex Entity,
    DescriptorIndex Descriptor,
    double Value);

/// <summary>
/// A parameter value which references another entity 
/// </summary>
public record ParameterEntity(
    EntityIndex Entity,
    DescriptorIndex Descriptor,
    EntityIndex Value);

/// <summary>
/// A 32-bit integer parameter value
/// </summary>
public record ParameterPoint(
    EntityIndex Entity,
    DescriptorIndex Descriptor,
    PointIndex Value);

//==
// Relations data

/// <summary>
/// Expresses different kinds of relationships between entities 
/// </summary>
public record EntityRelation(
    EntityIndex EntityA,
    EntityIndex EntityB,
    RelationType RelationTypeIndex);

public enum RelationType : byte
{
    // For parts or members of a set. Represents both aggregation and composition 
    MemberOf = 0,

    // Represents spatial relationships. Like part of a level, or a room.  
    ContainedIn = 1,

    // Used to express family instance to family type relationship  
    InstanceOf = 2,

    // For parts or openings that occur within a host (such as windows or doorways). 
    HostedBy = 3,

    // Represents relationship of compound structures and their consitutents 
    HasLayer = 4,

    // Represents different kinds of material relationshipss
    HasMaterial = 5,

    // For parent-child relationships of categories 
    SubcategoryOf = 6,

    // NOTE: the following are currently not used 

    // Two-way connectivity relationship. Can assume that only one direction is stored in DB 
    ConnectsTo = 7,

    // MEP networks and connection manager
    HasConnector = 8,

}