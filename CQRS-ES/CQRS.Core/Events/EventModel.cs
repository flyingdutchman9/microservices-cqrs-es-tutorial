using MongoDB.Bson.Serialization.Attributes;

namespace CQRS.Core.Events
{
    //Event Store -> Event Model poglavlje
    public class EventModel
    {
        // MongoDb koristi Guid _id  i za to trebamo
        // dodati atribute za primary key:
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public Guid AggregateIdentifier { get; set; }
        public string AggregateType { get; set; }
        public int Version { get; set; }
        public string EventType { get; set; }
        public BaseEvent EventData { get; set; }
    }
}
