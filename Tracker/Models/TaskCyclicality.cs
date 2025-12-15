using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public enum TaskCyclicality
    {

        repeating,
        [Display(Name = "not repeating")]
        notrepeat

    }
}
