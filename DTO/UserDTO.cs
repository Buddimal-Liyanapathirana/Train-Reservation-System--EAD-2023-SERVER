﻿namespace TrainReservationSystem.DTO
{
    public class UserDTO
    {
            public required string userNIC { get; set; }
            public required string userName { get; set; }
            public required string userEmail { get; set; }
            public required string userPassword { get; set; }
            public required string userRole { get; set; }
    }
}
