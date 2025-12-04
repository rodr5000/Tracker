using System.ComponentModel.DataAnnotations;

namespace Tracker.Models 
{
    public class TaskItem
    {
        public int TaskItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }

        // Navigation
        public ICollection<TimeLog> TimeLogs { get; set; }
    }
}
