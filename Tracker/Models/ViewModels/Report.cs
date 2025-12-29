namespace Tracker.Models.ViewModels
{
    public class TaskReportViewModel
    {
        public int TotalTasks { get; set; }

        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }

        public int OverdueCount { get; set; }

        public Dictionary<string, int> TasksPerMainTask { get; set; } = new Dictionary<string, int>();
    }
}