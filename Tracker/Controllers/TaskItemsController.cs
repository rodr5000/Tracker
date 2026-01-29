using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Utilities;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Models;
using Tracker.Models.Enums;
using Tracker.Models.ViewModels;

namespace Tracker.Controllers
{
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
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
    [Bind("Id,Title,MainTaskId,Description,Status,Priority,StartTime,EndTime")]
    Models.TaskItem taskItem)
        {
            // Always calculate TimeTaken from times
            if (taskItem.StartTime.HasValue && taskItem.EndTime.HasValue)
            {
                taskItem.TimeTaken = taskItem.EndTime.Value - taskItem.StartTime.Value;

                if (taskItem.TimeTaken < TimeSpan.Zero)
                {
                    ModelState.AddModelError("", "End time must be after start time.");
                }
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

            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MainTaskId = new SelectList(_context.MainTasks, "Id", "Name", taskItem.MainTaskId);
            return View(taskItem);
        }




        // GET: TaskItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
           
            if (id == null)
            {
                return NotFound();
            }


            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            ViewBag.MainTaskId = new SelectList(
               _context.MainTasks,
                "Id",
                "Name",
                taskItem.MainTaskId
            );
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,MainTaskId,Title,Description,Status,Priority,StartTime,EndTime")]
    Models.TaskItem taskItem)
        {
            var existingTask = await _context.TaskItems
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTask == null)
                return NotFound();

            // Always calculate TimeTaken from times
            if (taskItem.StartTime.HasValue && taskItem.EndTime.HasValue)
            {
                taskItem.TimeTaken = taskItem.EndTime.Value - taskItem.StartTime.Value;

                if (taskItem.TimeTaken < TimeSpan.Zero)
                {
                    ModelState.AddModelError("", "End time must be after start time.");
                }
            }
            else
            {
                taskItem.TimeTaken = null;
            }

            // Adjust MainTask.Duration
            var delta = TimeSpan.Zero;

            if (existingTask.Status == Status.Completed)
                delta += existingTask.TimeTaken ?? TimeSpan.Zero;

            if (taskItem.Status == Status.Completed)
                delta -= taskItem.TimeTaken ?? TimeSpan.Zero;

            if (taskItem.MainTaskId != null && delta != TimeSpan.Zero)
            {
                var mainTask = await _context.MainTasks.FindAsync(taskItem.MainTaskId);
                if (mainTask != null)
                {
                    mainTask.Duration ??= TimeSpan.Zero;
                    mainTask.Duration += delta;

                    if (mainTask.Duration < TimeSpan.Zero)
                        mainTask.Duration = TimeSpan.Zero;
                }
            }

            if (ModelState.IsValid)
            {
                _context.Update(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.MainTaskId = new SelectList(_context.MainTasks, "Id", "Name", taskItem.MainTaskId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
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
