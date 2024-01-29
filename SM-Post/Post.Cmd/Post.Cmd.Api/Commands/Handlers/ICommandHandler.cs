using Post.Cmd.Api.Commands.Restore;

namespace Post.Cmd.Api.Commands.Handlers
{
    public interface ICommandHandler
    {
        Task HandleAsync(NewPostCommand command);
        Task HandleAsync(DeletePostCommand command);
        Task HandleAsync(LikePostCommand command);
        Task HandleAsync(AddCommentCommand command);
        Task HandleAsync(EditCommentCommand command);
        Task HandleAsync(RemoveCommentCommand command);
        Task HandleAsync(EditMessageCommand command);
        Task HandleAsync(RestoreReadDbCommand command);
    }
}
