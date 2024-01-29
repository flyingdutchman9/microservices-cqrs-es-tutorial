using CQRS.Core.Messages;

namespace CQRS.Core.Events
{
    // Inherit Message because we also need Unique identifier
    public abstract class BaseEvent : Message
    {
        protected BaseEvent(string type)
        {
            Type = type;
        }

        /// <summary>
        /// Used to replay latest data aggregate 
        /// </summary>
        public int Version { get; set; }


        /// <summary>
        /// Descriminator property - will be used for polymorphic data when deserializing event objects..
        /// Will be explained in Kafka consumer
        /// </summary>
        public string Type { get; set; }
    }
}
