using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Reservation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string UserNIC{ get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string TrainId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ReservedOn { get; set; }
    public int EconomySeats { get; set; }
    public int LuxurySeats { get; set; }
    public double TotalFare { get; set; }

}
