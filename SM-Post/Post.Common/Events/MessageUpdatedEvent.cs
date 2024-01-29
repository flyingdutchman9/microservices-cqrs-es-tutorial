using CQRS.Core.Events;

namespace Post.Common.Events
{
    public class MessageUpdatedEvent : BaseEvent
    {
        public MessageUpdatedEvent() : base(type: nameof(PostCreatedEvent))
        {
        }

        public string Message { get; set; }
    }
}
