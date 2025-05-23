using Uyg.API.Models;

namespace Uyg.API.Repositories
{
    public class ArticleRepository : GenericRepository<Article>
    {
        public ArticleRepository(AppDbContext context) : base(context)
        {
        }
    }
}
