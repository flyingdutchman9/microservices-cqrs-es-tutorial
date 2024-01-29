using CQRS.Core.Events;

namespace Post.Common.Events
{
    public class PostRemovedEvent : BaseEvent
    {
        public PostRemovedEvent() : base(type: nameof(PostRemovedEvent))
        {
        }
        public Guid CommentId { get; set; }
    }
}
