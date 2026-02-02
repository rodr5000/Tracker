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
        public IdentityUser User { get; set; }
        public int? MainTaskId { get; set; }
        [ValidateNever]
        public MainTask? MainTask { get; set; } 



        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; }= string.Empty;

        public long? TimeTakenTicks { get; set; }

        [NotMapped]
        public TimeSpan? TimeTaken
        {
            get => TimeTakenTicks.HasValue ? TimeSpan.FromTicks(TimeTakenTicks.Value) : null;
            set => TimeTakenTicks = value?.Ticks;
        }

        [Required]
        public Status Status { get; set; } = Status.Pending;

        [Required]
        public Priority Priority { get; set; } = Priority.Medium;

        public DateTime? StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }

        

        // Navigation
        //public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
    }
}
