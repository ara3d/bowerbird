using System;
using System.Collections.Generic;
using System.Security;
using Ara3D.Collections;

namespace BIMOpenSchema;

public enum EntityIndex : long { }
public enum PointIndex : long { }
public enum LevelIndex : long { }
public enum DocumentIndex : long { }
public enum DescriptorIndex : long { }
public enum TypeIndex : long { }
public enum RoomIndex : long { }

public record Entity(
    string Id,
    DocumentIndex Document,
    string Name,
    string Category);

public record Type(
    EntityIndex Entity, 
    string Category);

public record Descriptor(
    string Name,
    string Units,
    string Group);

public record ParameterInt(
    EntityIndex Entity,
    DescriptorIndex Descriptor,
    int Value);

public record ParameterString(
    EntityIndex Entity,
    DescriptorIndex Descriptor,
    string Value);

public record ParameterDouble(
    EntityIndex Entity,
    DescriptorIndex Descriptor,
    double Value);

public record ParameterEntity(
    EntityIndex Entity,
    DescriptorIndex Descriptor,
    EntityIndex Value);

public record BoundsComponent(
    EntityIndex Entity,
    PointIndex Min,
    PointIndex Max);

public record LocationComponent(
    EntityIndex Entity,
    PointIndex Point);

public record Document(
    string Title,
    string PathName);

public record Level(
    EntityIndex LevelEntity,
    double Elevation);

public record LevelRelation(
    EntityIndex Entity,
    LevelIndex Level);

public record TypeRelation(
    EntityIndex Entity,
    TypeIndex Type);

public record Room(
    EntityIndex Entity,
    double BaseOffset,
    double LimitOffset,
    string RoomNumber);

public record Point(
    double X, 
    double Y, 
    double Z);

public class ParameterData
{
    public List<ParameterInt> Integers { get; set; } = [];
    public List<ParameterDouble> Doubles { get; set; } = [];
    public List<ParameterString> Strings { get; set; } = [];
    public List<ParameterEntity> Entities { get; set; } = [];
}

/// <summary>
/// Contains all the BIM Data for a federated model. 
/// We generally don't use this as-is except for serialization. 
/// </summary>
public class BIMData
{
    public ParameterData ParameterData { get; set; } = new();
    public List<Descriptor> Descriptors { get; set; } = [];
    public List<Point> Points { get; set; } = [];
    public List<Room> Rooms { get; set; } = [];
    public List<Level> Levels { get; set; } = [];
    public List<LevelRelation> LevelRelations { get; set; } = [];
    public List<Document> Documents { get; set; } = [];
    public List<Entity> Entities { get; set; } = [];
    public List<Type> Types { get; set; } = [];
    public List<BoundsComponent> Bounds { get; set; } = [];
    public List<LocationComponent> Locations { get; set; } = [];
}