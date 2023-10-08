﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

public class Train
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string TrainName { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? Schedule { get; set; }
    public bool IsActive { get; set; } = true;
    public int LuxurySeatCount { get; set; }
    public int EconomySeatCount { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string>? Reservations { get; set; }
}
