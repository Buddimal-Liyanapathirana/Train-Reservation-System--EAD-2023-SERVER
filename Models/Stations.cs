namespace TrainReservationSystem.Models
{
    public class Stations
    {
        public static HashSet<string> SOUTH = new HashSet<string>
        {
            "Maradana",
            "Fort",
            "Kalutara",
            "Benthota",
            "Ambalangoda",
            "Galle",
            "Matara",
            "Hambanthota"
        };

        public static HashSet<string> NORTH = new HashSet<string>
        {
            "Colombo",
            "Negombo",
            "Chilaw",
            "Puttlam",
            "Anuradapura",
            "Medawachchiya",
            "Kilinochi",
            "Jaffna"
        };

        public static HashSet<string> UPCOUNTRY = new HashSet<string>
        {
            "Colombo",
            "Kaduwela",
            "Avissawella",
            "Kithulgala",
            "Thalawakale",
            "Nuwara Eliya"
        };

    }
}
