using System.ComponentModel.DataAnnotations;

namespace Tracker.Models 
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; }= string.Empty;

        [Required]
        public Status Status { get; set; } = Status.Pending;

        [Required]
        public Priority Priority { get; set; } = Priority.Medium;

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }

        // Navigation
        public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
    }
}
