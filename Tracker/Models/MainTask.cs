using Tracker.Models.Enums;

namespace Tracker.Models
{
    public class MainTask
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public TaskCyclicality TaskCyclicality { get; set; } = TaskCyclicality.notrepeat;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }


        // Navigation
        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
    }
}
