namespace Tracker.Models.ViewModels
{
    public class AdminDashboardViewModel
    {

        public int TotalUsers { get; set; }
        public int TotalTasks { get; set; }

        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }

        public double TotalHoursWorked { get; set; }

        public Dictionary<string, int> TasksPerUser { get; set; } = new();
    }
}
