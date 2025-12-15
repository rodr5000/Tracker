using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tracker.Models 
{
    public class TaskItem
    {

        public int Id { get; set; }

        public int? MainTaskId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; }= string.Empty;

        [Required]
        public Status Status { get; set; } = Status.Pending;

        [Required]
        public Priority Priority { get; set; } = Priority.Medium;

        public DateTime? StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }

        

        // Navigation
        public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
    }
}
