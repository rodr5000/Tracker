using System.ComponentModel.DataAnnotations;

namespace Tracker.Models.Enums
{
    public enum TaskCyclicality
    {

        repeating,
        [Display(Name = "not repeating")]
        notrepeat

    }
}
