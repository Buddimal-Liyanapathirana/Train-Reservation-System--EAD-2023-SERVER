namespace TrainReservationSystem.DTO
{
    public class EditReservationDTO
    {

            public required int luxurySeats { get; set; }
            public required int economySeats { get; set; }
            public required DateTime reservationDate { get; set; }
            public required string startStation { get; set; }
            public required string endStation { get; set; }

    }
}
