using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace MongoDotnetDemo.Models
{
    public class Schedule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string DepartureStation { get; set; }
        public string ArrivalStation { get; set; }
        public string Route { get; set; }
        public HashSet<string> stopStations { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public double LuxuryFare { get; set; } = 500;
        public double EconomyFare { get; set; } = 250;
        // Days of operation
        public HashSet<DayOfWeek> OperatingDays { get; set; }

    }
}
