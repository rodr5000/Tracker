namespace Tracker.Models
{
    public class Tag
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<TaskItemTag> TaskItemTags { get; set; }
            = new List<TaskItemTag>();
    }
}
