using System.ComponentModel.DataAnnotations;

namespace RisingStarsData.Entities
{// Student Model
    public class Comment
    {
        public int CommentId { get; set; }
        [Required] public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public string StudentId { get; set; }
        public Student Student { get; set; }
    }
}
