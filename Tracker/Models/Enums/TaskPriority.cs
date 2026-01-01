using System.ComponentModel.DataAnnotations;

namespace Tracker.Models.Enums
{
    public enum Priority
    {
        [Display(Name = "Low Priority")]
        Low,
        [Display(Name = "Med Priority")]
        Medium,
        [Display(Name = "High Priority")]
        High,
        [Display(Name = "Critical Priority")]
        Critical
    }

}
