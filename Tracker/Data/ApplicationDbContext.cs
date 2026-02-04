using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Composition;
using System.Security.Claims;
using Tracker.Models;
namespace Tracker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<MainTask> MainTasks { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }

        private string? CurrentUserId =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirstValue(ClaimTypes.NameIdentifier);

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TaskItem>()
                .HasQueryFilter(t => t.UserId == CurrentUserId);

            builder.Entity<MainTask>()
                .HasQueryFilter(m => m.UserId == CurrentUserId);
        }

    }
}
