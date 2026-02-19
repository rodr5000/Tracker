using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using Tracker.Models.Enums;

namespace Tracker.Models
{
    public class MainTask
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        [ValidateNever]
        public string UserId { get; set; }

        [ValidateNever]
        public ApplicationUser User { get; set; }

        public string Description { get; set; } = string.Empty;

        //public TimeSpan? Duration { get; set; }
        //public TaskCyclicality TaskCyclicality { get; set; } = TaskCyclicality.notrepeat;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public long? DurationTicks { get; set; }

        [NotMapped]
        public TimeSpan? Duration
        {
            get => DurationTicks.HasValue ? TimeSpan.FromTicks(DurationTicks.Value) : null;
            set => DurationTicks = value?.Ticks;
        }


        // Navigation
        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
    }
}
