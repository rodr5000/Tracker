using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Models;
using Tracker.Models.Enums;
using Tracker.Models.ViewModels;

namespace Tracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;


        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> UserTasks(string userId)
        {
            var tasks = await _context.TaskItems
                .IgnoreQueryFilters()
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return View(tasks);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.TaskItems.IgnoreQueryFilters() 
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                return BadRequest("You cannot delete yourself");
            }
            else 
            {

                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("Users");
        }
        public async Task<IActionResult> TaskDetails(int id)
        {
            var task = await _context.TaskItems
                .IgnoreQueryFilters() 
                .Include(t => t.MainTask)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            return View(task);
        }
        public async Task<IActionResult> Dashboard()
        {
            var users = await _userManager.Users.ToListAsync();

            var tasks = await _context.TaskItems
                .IgnoreQueryFilters()
                .ToListAsync();

            var now = DateTime.Now;

            var model = new AdminDashboardViewModel
            {
                TotalUsers = users.Count,
                TotalTasks = tasks.Count,

                CompletedTasks = tasks.Count(t => t.Status == Status.Completed),
                InProgressTasks = tasks.Count(t => t.Status == Status.InProgress),
                PendingTasks = tasks.Count(t => t.Status == Status.Pending),

                OverdueTasks = tasks.Count(t =>
                    t.DueDate.HasValue &&
                    t.DueDate < now &&
                    t.Status != Status.Completed),

                TotalHoursWorked = tasks
                    .Where(t => t.TimeTaken.HasValue)
                    .Sum(t => t.TimeTaken.Value.TotalHours),

                TasksPerUser = tasks
                .GroupBy(t => t.UserId) 
                .ToDictionary(g => users
                .FirstOrDefault(u => u.Id == g.Key)?
                .Email ?? "Unknown",g => g.Count())
            };

            return View(model);
        }
    }
}
