using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Uyg.API.Models
{
    public class Comment : BaseEntity
    {
        public string Content { get; set; }
        public int ArticleId { get; set; }
        public string UserId { get; set; }
        
        // Navigation properties
        [ForeignKey("ArticleId")]
        public Article? Article { get; set; }
        
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
    }
} 