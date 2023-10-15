using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

public class Route
{
    [BsonId]
    public string Name { get; set; }
    public  HashSet<string> Stations { get; set; }
}

