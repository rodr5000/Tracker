using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public enum Status
    {
        Pending,
        [Display(Name ="In Progress")]
        InProgress,
        Completed,
        Cancelled
    }
}