using Microsoft.AspNetCore.Identity;

namespace Tracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<MainTask> MainTasks { get; set; } = new List<MainTask>();
        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
    }
}
