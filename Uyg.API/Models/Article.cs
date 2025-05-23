namespace Uyg.API.Models
{
    public class Article : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string UserId { get; set; }
        public DateTime PublishDate { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public Category? Category { get; set; }
        public AppUser? User { get; set; }
    }
}
