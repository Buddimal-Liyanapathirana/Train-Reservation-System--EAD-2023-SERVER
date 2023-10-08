namespace MongoDotnetDemo.Models
{
    public class DatabaseSettings
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public string? UsersCollectionName { get; set; }
        public string? TrainsCollectionName { get; set; }
        public string? ReservationsCollectionName { get; set; }
        public string? SchedulesCollectionName { get; set; }
    }
}
