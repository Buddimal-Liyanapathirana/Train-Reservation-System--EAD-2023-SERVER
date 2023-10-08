using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace MongoDotnetDemo.Models
{
    public class User
    {
        [BsonId]
        [RegularExpression("^[0-9]{12}$|^[0-9]{9}v$", ErrorMessage = "NIC must be either a 12-digit string or a 10-digit string with the last digit being 'v'.")]
        public string NIC { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }

        [EnumDataType(typeof(UserRole))]
        public string Role { get; set; }
        public bool? IsActive { get; set; } = true;

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string>? ReservationIds { get; set; }

    }

    public enum UserRole
    {
        BACK_OFFICER,
        TRAVEL_AGENT,
        TRAVELER
    }
}