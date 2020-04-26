using Microsoft.EntityFrameworkCore;

namespace BookListRazor.Model
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<Book> Books { get; set; }
    }
}
