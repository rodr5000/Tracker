using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Tracker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Tracker.Models.TaskItem> TaskItems { get; set; }
        public DbSet<Tracker.Models.TimeLog> TimeLogs { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
