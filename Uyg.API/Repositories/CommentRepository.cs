using Microsoft.EntityFrameworkCore;
using Uyg.API.Models;

namespace Uyg.API.Repositories
{
    public class CommentRepository : GenericRepository<Comment>
    {
        public CommentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Comment>> GetByArticleIdAsync(int articleId)
        {
            return await _context.Comments
                .Where(c => c.ArticleId == articleId && c.IsActive)
                .Include(c => c.User)
                .OrderByDescending(c => c.Created)
                .ToListAsync();
        }
    }
} 