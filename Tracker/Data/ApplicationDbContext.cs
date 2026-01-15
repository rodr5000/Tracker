using System.Composition;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tracker.Models;
namespace Tracker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<MainTask> MainTasks { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
