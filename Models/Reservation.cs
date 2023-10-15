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
    public string startStation { get; set; }
    public string endStation { get; set; }
    public int EconomySeats { get; set; }
    public int LuxurySeats { get; set; }
    public double TotalFare { get; set; }
    public bool isCompleted { get; set; }
    public string completedTrain {  get; set; }
    public bool isRequested { get; set; }
    public string RequestedBy { get; set; }

}
