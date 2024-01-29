using CQRS.Core.Commands;

namespace Post.Cmd.Api.Commands
{
    public class RemoveCommentCommand : BaseCommand
    {
        public Guid CommentId { get; set; } 
        public string Username { get; set; } // Nobody should be allowed to edit someone else's comment
    }
}
