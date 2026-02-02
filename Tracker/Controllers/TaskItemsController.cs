using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tracker.Data;
using Tracker.Models;
using Tracker.Models.Enums;
using Tracker.Models.ViewModels;



namespace Tracker.Controllers
{
    [Authorize]
    public class TaskItemsController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TaskItemsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var tasks = await _context.TaskItems
                .Where(t => t.UserId == userId)
                .Include(t => t.MainTask)
                .ToListAsync();

            return View(await _context.TaskItems.ToListAsync());
            
        }

        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            var taskItem = await _context.TaskItems.Include(t => t.MainTask).FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }
            
            return View(taskItem);
        }

        // GET: TaskItems/Create
        public IActionResult Create()
        {
            ViewBag.MainTaskId = new SelectList(_context.MainTasks, "Id", "Name");

            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("Id,Title,MainTaskId,Description,Status,Priority,StartTime,EndTime,UserId")]
    Models.TaskItem taskItem)
        {
            var userId = _userManager.GetUserId(User);
            taskItem.UserId = userId;

            // TimeTaken
            if (taskItem.StartTime.HasValue && taskItem.EndTime.HasValue)
            {
                taskItem.TimeTaken = taskItem.EndTime.Value - taskItem.StartTime.Value;
            }

            // Ensure MainTask
            if (!taskItem.MainTaskId.HasValue || taskItem.MainTaskId == 0)
            {
                var generalTask = await _context.MainTasks
                    .FirstOrDefaultAsync(m => m.Name == "General" && m.UserId == userId);

                if (generalTask == null)
                {
                    generalTask = new MainTask
                    {
                        Name = "General",
                        UserId = userId,
                        DurationTicks = 0
                    };

                    _context.MainTasks.Add(generalTask);
                    await _context.SaveChangesAsync();
                }

                taskItem.MainTaskId = generalTask.Id;
            }
            // Reduce MainTask.Duration if task is completed
            if (taskItem.Status == Status.Completed && taskItem.MainTaskId != null && taskItem.TimeTaken.HasValue)
            {
                var mainTask = await _context.MainTasks.FindAsync(taskItem.MainTaskId);
                if (mainTask != null)
                {
                    mainTask.Duration ??= TimeSpan.Zero;
                    mainTask.Duration -= taskItem.TimeTaken.Value;
                    if (mainTask.Duration < TimeSpan.Zero)
                        mainTask.Duration = TimeSpan.Zero;
                }
            }

            try
            {
                _context.TaskItems.Add(taskItem);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 SAVE FAILED:");
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                throw;
            }   

            return RedirectToAction(nameof(Index));
        }




        // GET: TaskItems/Edit/5
        // GET: TaskItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            // Security: Only find the task item if it belongs to a MainTask owned by this user
            var taskItem = await _context.TaskItems
                .Include(t => t.MainTask)
                .FirstOrDefaultAsync(t => t.Id == id && t.MainTask.UserId == userId);

            if (taskItem == null) return NotFound();

            // Only show the user's own MainTasks in the dropdown
            ViewBag.MainTaskId = new SelectList(
                _context.MainTasks.Where(m => m.UserId == userId),
                "Id", "Name", taskItem.MainTaskId);

            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MainTaskId,UserId,Title,Description,Status,Priority,StartTime,EndTime")] Models.TaskItem taskItem)
        {
            taskItem.UserId = _userManager.GetUserId(User);

            if (id != taskItem.Id) return NotFound();

            // 1. Manually remove validation for properties NOT in the form
            ModelState.Remove("MainTask");
            // If your TaskItem model has a "User" or other linked models, remove them here too:
            // ModelState.Remove("User");

            // 2. Perform your Time Calculation Logic
            if (taskItem.StartTime.HasValue && taskItem.EndTime.HasValue)
            {
                taskItem.TimeTaken = taskItem.EndTime.Value - taskItem.StartTime.Value;
                if (taskItem.TimeTaken < TimeSpan.Zero)
                {
                    ModelState.AddModelError("", "End time must be after start time.");
                }
            }


            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the record
                    _context.Update(taskItem);

                    // Handle the Duration logic for the parent MainTask here if needed...

                    await _context.SaveChangesAsync();

                    // Redirect to MainTasks Index so you can see the change immediately
                    return RedirectToAction("Index", "MainTasks");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TaskItems.Any(e => e.Id == taskItem.Id)) return NotFound();
                    else throw;
                }
            }

            // If we reach here, validation failed. 
            // Check 'ModelState' in debugger to see why!
            var userId = _userManager.GetUserId(User);
            ViewBag.MainTaskId = new SelectList(_context.MainTasks.Where(m => m.UserId == userId), "Id", "Name", taskItem.MainTaskId);
            return View(taskItem);
        }




        // GET: TaskItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem != null)
            {
                _context.TaskItems.Remove(taskItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Calendar()
        {
            return View();
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.Id == id);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, Status status)
        {
            var task = await _context.TaskItems
                .Include(t => t.MainTask)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            task.Status = status;

            if (status == Status.Completed && task.TimeTaken.HasValue && task.MainTask != null)
            {
                var mainTask = task.MainTask;

                // If MainTask has no duration yet, initialize it
                if (!mainTask.Duration.HasValue)
                    mainTask.Duration = TimeSpan.Zero;

                // Reduce duration by task time
                mainTask.Duration -= task.TimeTaken;

                // Optional safety: don’t go below zero
                if (mainTask.Duration < TimeSpan.Zero)
                    mainTask.Duration = TimeSpan.Zero;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        public IActionResult Report()
        {
            var now = DateTime.Now;

            var tasks = _context.TaskItems
                .Include(t => t.MainTask)
                .ToList();

            var model = new TaskReportViewModel
            {
                TotalTasks = tasks.Count,
                PendingCount = tasks.Count(t => t.Status == Status.Pending),
                InProgressCount = tasks.Count(t => t.Status == Status.InProgress),
                CompletedCount = tasks.Count(t => t.Status == Status.Completed),
                CancelledCount = tasks.Count(t => t.Status == Status.Cancelled),

                OverdueCount = tasks.Count(t =>
                    t.EndTime.HasValue &&
                    t.EndTime.Value < now &&
                    t.Status != Status.Completed
                )
            };

            model.TasksPerMainTask = tasks
                .GroupBy(t => t.MainTask?.Name ?? "No Main Task")
                .ToDictionary(g => g.Key, g => g.Count());

            return View(model);
        }



        public IActionResult GetCalendarEvents()
        {
            var tasks = _context.TaskItems
                .Where(t => t.StartTime != null && t.EndTime != null)
                .Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    start = t.StartTime,
                    end = t.EndTime,
                    backgroundColor = t.Status == Status.Completed ? "#4CAF50" :
                                      t.Status == Status.InProgress ? "#2196F3" :
                                      "#FFC107"
               })
                .ToList();

            return Json(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTime([FromBody] UpdateTimeDto dto)
        {
            var task = await _context.TaskItems.FindAsync(dto.Id);
            if (task == null) return NotFound();

            task.StartTime = dto.StartTime;
            task.EndTime = dto.EndTime;
            task.TimeTaken = dto.EndTime - dto.StartTime;

            await _context.SaveChangesAsync();
            return Ok();
        }


        public class UpdateTimeDto
        {
            public int Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }





    }
}
