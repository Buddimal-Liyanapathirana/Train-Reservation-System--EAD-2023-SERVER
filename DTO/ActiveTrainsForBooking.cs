using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TrainReservationSystem.DTO
{
    public class ActiveTrainsForBooking
    {
            public string Id { get; set; }
            public string ScheduleId { get; set; }
            public string TrainName { get; set; }
            public HashSet<string> Route { get; set; }
            public HashSet<DayOfWeek> OperatingDays { get; set; }
            public int LuxurySeatCount { get; set; } = 50;
            public int EconomySeatCount { get; set; } = 50;
            public int AvailableLuxurySeats { get; set; } = 0;
            public int AvailableEconomySeats { get; set; } = 0;

        }
}
