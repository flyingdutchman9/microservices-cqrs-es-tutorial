using CQRS.Core.Events;

namespace Post.Common.Events
{
    public class CommentRemovedEvent : BaseEvent
    {
        public CommentRemovedEvent() : base(type: nameof(CommentRemovedEvent))
        {
        }

        public Guid CommentId { get; set; }
    }
}
