namespace TrainReservationSystem.DTO
{
    public class ReservationRequestDTO
    {
        public required int luxurySeats { get; set; }
        public required int economySeats { get; set; }
        public required DateTime requestForDate { get; set; }
        public required string resuestedUserId { get; set; }
        public required string startStation { get; set; }
        public required string endStation { get; set; }

    }
}

