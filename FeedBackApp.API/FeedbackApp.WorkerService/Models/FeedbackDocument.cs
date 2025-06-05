using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeedbackApp.WorkerService.Models
{
    public class FeedbackDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Name")]
        public string? Name { get; set; }

        [BsonElement("Email")]
        public string? Email { get; set; }

        [BsonElement("Message")]
        public string? Message { get; set; }

        [BsonElement("ReceivedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)] 
        public DateTime ReceivedAt { get; set; }

        [BsonElement("ProcessedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ProcessedAt { get; set; }
    }
}
