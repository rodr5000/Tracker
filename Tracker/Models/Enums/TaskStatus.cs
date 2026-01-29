using System.ComponentModel.DataAnnotations;

namespace Tracker.Models.Enums
{
    public enum Status
    {
        Pending = 0,
        [Display(Name ="In Progress")]
        InProgress = 1,
        Completed = 2 ,
        Cancelled = 3
    }
}