using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MongoDotnetDemo.Models
{
    public class Schedule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string DepartureStation { get; set; }
        public string ArrivalStation { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        // Days of operation
        public HashSet<DayOfWeek> OperatingDays { get; set; }

    }
}
