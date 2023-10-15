namespace TrainReservationSystem.DTO
{
    public class ScheduleDTO
    {
        public required string scheduleRoute { get; set; }
        public required int scheduleLuxuryFare { get; set; }
        public required int scheduleEconomyFare { get; set; }
        public HashSet<DayOfWeek> scheduleOperatingDays { get; set; }
        public DateTime scheduleDepartureTime { get; set; }
        public DateTime scheduleArrivalTime { get; set; }
    }
}
