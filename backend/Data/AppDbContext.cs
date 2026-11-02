using Microsoft.EntityFrameworkCore;

namespace Api;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
    {
        
    }

}
