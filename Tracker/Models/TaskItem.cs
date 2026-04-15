using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tracker.Models.Enums;

namespace Tracker.Models 
{
    public class TaskItem
    {

        public int Id { get; set; }

        [ValidateNever]
        public string UserId { get; set; }

        [ValidateNever]
        public ApplicationUser User { get; set; }
        public int? MainTaskId { get; set; }
        [ValidateNever]
        public MainTask? MainTask { get; set; } 



        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; }= string.Empty;

        public long? TimeTakenTicks { get; set; }

        public long? EstimatedTimeTicks { get; set; }

        [NotMapped]
        public TimeSpan? TimeTaken
        {
            get => TimeTakenTicks.HasValue
                ? TimeSpan.FromTicks(TimeTakenTicks.Value)
                : null;
            set => TimeTakenTicks = value?.Ticks;
        }

        [NotMapped]
        public TimeSpan? EstimatedTime
        {
            get => EstimatedTimeTicks.HasValue
                ? TimeSpan.FromTicks(EstimatedTimeTicks.Value)
                : null;
            set => EstimatedTimeTicks = value?.Ticks;
        }

        [Required]
        public Status Status { get; set; } = Status.Pending;

        [Required]
        public Priority Priority { get; set; } = Priority.Medium;

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }

        public DateTime? WorkStartTime { get; set; }
        public bool IsWorking { get; set; } = false;

        [NotMapped]
        public double ProgressPercentage
        {
            get
            {
                if (!EstimatedTime.HasValue || EstimatedTime.Value.TotalSeconds == 0)
                    return 0;

                return Math.Min(100,
                    (TimeTaken?.TotalSeconds ?? 0) /
                    EstimatedTime.Value.TotalSeconds * 100);
            }
        }

        // Navigation
        //public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
    }
}
