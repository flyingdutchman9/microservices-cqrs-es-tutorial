using CQRS.Core.Domain;
using Post.Cmd.Domain.Models;
using Post.Common.Events;

namespace Post.Cmd.Domain.Aggregates
{
    public class PostAggregate : AggregateRoot
    {
        public bool Active { get => _active; set => _active = value; }

        private string _author;
        private bool _active;
        // Isključio sam ideju Tuplea jer je preapstraktan u korištenju -> pretvoreno u Domain model PostComment
        //private readonly Dictionary<Guid, Tuple<string, string>> _comments = new();
        private readonly Dictionary<Guid, PostComment> _comments = new();

        public PostAggregate() { }

        #region Post Message (through constructor)
        public PostAggregate(Guid id, string author, string message)
        {
            RaiseEvent(new PostCreatedEvent
            {
                Id = id,
                Author = author,
                Message = message,
                DatePosted = DateTime.UtcNow,
            });
        }

        // Ova metoda će biti pozvana nakon RaiseEvent poziva (sjetimo se..Reflectiona u AggregateRoot)
        public void Apply(PostCreatedEvent @event)
        {
            _id = @event.Id;
            _active = true;
            _author = @event.Author;
        }
        #endregion

        #region Edit Message
        public void EditMessage(string message)
        {
            if (!_active) { throw new InvalidOperationException("You cannot edit the message of inactive post"); }
            if (string.IsNullOrWhiteSpace(message)) { throw new InvalidOperationException($"The value of {nameof(message)} cannot be null or empty"); }

            RaiseEvent(new MessageUpdatedEvent
            {
                Id = Id,
                Message = message,
            });
        }

        public void Apply(MessageUpdatedEvent @event)
        {
            _id = @event.Id;
        }
        #endregion

        #region Like Post
        public void LikePost()
        {
            if (!Active) { throw new InvalidOperationException("You cannot like inactive post"); }

            RaiseEvent(new PostLikedEvent { Id = Id });
        }

        public void Apply(PostLikedEvent @event)
        {
            _id = @event.Id;
        }
        #endregion

        #region Add Comment
        public void AddComment(string comment, string username)
        {
            if (!Active) { throw new InvalidOperationException("You cannot add a comment to an inactive post"); }
            if (string.IsNullOrWhiteSpace(comment)) { throw new InvalidOperationException($"The value of {nameof(comment)} cannot be empty"); }

            RaiseEvent(new CommentAddedEvent
            {
                Id = base.Id,
                CommentId = Guid.NewGuid(),
                Comment = comment,
                Username = username,
                CommentDate = DateTime.UtcNow,
            });
        }

        public void Apply(CommentAddedEvent @event)
        {
            _id = @event.Id;
            //_comments.Add(@event.CommentId, new Tuple<string, string>(@event.Comment, @event.Username));
            _comments.Add(@event.CommentId, new PostComment { CommentText = @event.Comment, Username = @event.Username });
        }
        #endregion

        #region Edit Comment
        public void EditComment(Guid commentId, string comment, string username)
        {
            if (!Active) { throw new InvalidOperationException("You cannot edit a comment of an inactive post"); }

            //if (!_comments[commentId].Item2.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            //{
            //}
            if (!_comments[commentId].Username.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("You are not allowed to edit comment made by another user");
            }

            RaiseEvent(new CommentUpdatedEvent
            {
                Id = Id,
                CommentId = commentId,
                Username = username,
                CommentText = comment,
                EditDate = DateTime.UtcNow,
            });
        }

        public void Apply(CommentUpdatedEvent @event)
        {
            _id = @event.Id;
            _comments[@event.Id] = new PostComment { Username = @event.Username };
        }
        #endregion

        #region Remove Comment
        public void RemoveComment(Guid commentId, string username)
        {
            if (!Active) { throw new InvalidOperationException("You cannot remove a comment of an inactive post"); }
            if (!_comments[commentId].Username.Equals(username, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("You are not allowed to delete a comment made by another user");
            }

            RaiseEvent(new CommentRemovedEvent
            {
                Id = Id,
                CommentId = commentId,
            });
        }

        public void Apply(CommentRemovedEvent @event)
        {
            _id = @event.Id;
            _comments.Remove(@event.Id);
        }
        #endregion

        #region Delete Post
        public void DeletePost(string username)
        {
            if (!Active) { throw new InvalidOperationException("The post has already been removed"); }
            if (!_author.Equals(username, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException("You are not allowed to delete a post made by another user");
            }

            RaiseEvent(new PostRemovedEvent
            {
                Id = Id
            });
        }

        public void Apply(PostRemovedEvent @event)
        {
            _id = @event.Id;
            Active = false;
        }
        #endregion
    }
}
