using System.ComponentModel.DataAnnotations;

namespace Tracker.Models.Enums
{
    public enum Priority
    {
        [Display(Name = "Low Priority")]
        Low = 0,
        [Display(Name = "Med Priority")]
        Medium = 1,
        [Display(Name = "High Priority")]
        High = 2,
        [Display(Name = "Critical Priority")]
        Critical = 3
    }

}
