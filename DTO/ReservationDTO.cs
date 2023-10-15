namespace TrainReservationSystem.DTO
{
    public class ReservationDTO
    {
        public required int luxurySeats { get; set; }
        public required int economySeats { get; set; }
        public required DateTime reservationDate { get; set; }
        public required string userNIC { get; set; }
        public required string trainId { get; set; }
        public required string startStation { get; set; }
        public required string endStation { get; set; }
    }
}
