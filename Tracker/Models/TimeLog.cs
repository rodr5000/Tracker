using System.ComponentModel.DataAnnotations;

namespace Tracker.Models

{
    public class TimeLog
    {
        public int TimeLogId { get; set; }

        [Required]
        public int TaskItemId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [StringLength(300)]
        public string Note { get; set; }

        // Navigation
        public TaskItem TaskItem { get; set; }
    }
}