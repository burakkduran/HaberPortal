namespace Uyg.API.DTOs
{
    public class CommentDto : BaseDto
    {
        public string Content { get; set; }
        public int ArticleId { get; set; }
        public string UserId { get; set; }
        public UserDto? User { get; set; }
    }
} 