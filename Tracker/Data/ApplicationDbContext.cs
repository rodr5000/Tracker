using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Composition;
using System.Reflection.Emit;
using System.Security.Claims;
using Tracker.Models;
namespace Tracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<MainTask> MainTasks { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<TaskItemTag> TaskItemTags { get; set; }

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

            builder.Entity<MainTask>()
                .HasOne(m => m.User)
                .WithMany(u => u.MainTasks)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TaskItem>()
                .HasOne(t => t.User)
                .WithMany(u => u.TaskItems)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TaskItem>()
                .HasOne(t => t.MainTask)
                .WithMany(m => m.TaskItems)
                .HasForeignKey(t => t.MainTaskId)
                .OnDelete(DeleteBehavior.Cascade);



            builder.Entity<TaskItemTag>()
            .HasKey(tt => new { tt.TaskItemId, tt.TagId });

            builder.Entity<TaskItemTag>()
                .HasOne(tt => tt.TaskItem)
                .WithMany(t => t.TaskItemTags)
                .HasForeignKey(tt => tt.TaskItemId);

            builder.Entity<TaskItemTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TaskItemTags)
                .HasForeignKey(tt => tt.TagId);
        }



    }
}
