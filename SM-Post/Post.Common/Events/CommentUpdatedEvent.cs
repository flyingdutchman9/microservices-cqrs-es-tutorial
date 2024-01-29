using CQRS.Core.Events;

namespace Post.Common.Events
{
    public class CommentUpdatedEvent : BaseEvent
    {
        public CommentUpdatedEvent() : base(type: nameof(CommentUpdatedEvent))
        {
        }

        public Guid CommentId { get; set; }
        public string CommentText { get; set; }
        public string Username { get; set; }
        public DateTime EditDate { get; set; }
    }
}
