using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Post.Query.Domain.Entities
{
    [Table("Post")]
    public class PostEntity
    {
        [Key]
        [Column("PostId")]
        public Guid PostId { get; set; }
        public string Author { get; set; }
        public DateTime DatePosted { get; set; }
        public string Message { get; set; }
        public int Likes { get; set; }

        [JsonIgnore] // izbjegavamo circular reference kod serijalizacije
        public virtual ICollection<CommentEntity> Comments { get; set; }
    }
}
