using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TrainReservationSystem.Models
{
    public class UserActivationRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string userNic { get; set; }
    }
}
