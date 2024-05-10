using Microsoft.EntityFrameworkCore;

namespace pswebrunner.Models
{
    public class ScriptDbContext : DbContext
    {
        public DbSet<ScriptPath> ScriptPaths { get; set; }

        public ScriptDbContext(DbContextOptions<ScriptDbContext> options)
            : base(options)
        {
        }
    }

    public class ScriptPath
    {
        public int Id { get; set; }
        public string Path { get; set; }
    }
}
