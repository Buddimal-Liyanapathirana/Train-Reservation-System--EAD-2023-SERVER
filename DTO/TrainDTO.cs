namespace TrainReservationSystem.DTO
{
    public class TrainDTO
    {
        public required string trainName { get; set; }
        public required int luxurySeatCount { get; set; }
        public required int economySeatCount { get; set; }
    }
}
