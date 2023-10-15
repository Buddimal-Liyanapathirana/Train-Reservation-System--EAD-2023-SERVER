namespace TrainReservationSystem.DTO
{
    public class UpdateUserDTO
    {
            public required string userName { get; set; }
            public required string userEmail { get; set; }
            public required string userPassword { get; set; }
            public required string userRole { get; set; }
    }
}
