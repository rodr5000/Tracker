using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Models;
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
            ViewBag.MainTaskId = new SelectList(_context.mainTasks, "Id", "Name");
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,MainTaskId,Description,Status,Priority,StartTime,EndTime")] TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // 🔴 YOU MUST RELOAD THE DROPDOWN
            ViewBag.MainTaskId = new SelectList(
                _context.mainTasks,
                "Id",
                "Name",
                taskItem.MainTaskId
            );

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
               _context.mainTasks,
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,MainTaskId,Title,Description,Status,Priority,CreatedAt,DueDate")] TaskItem taskItem)
        {
            if (id != taskItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MainTaskId = new SelectList(
               _context.mainTasks,
                "Id",
                "Name",
                taskItem.MainTaskId
            );

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
        public async Task<IActionResult> UpdateStatus(int id, Status status)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
                return NotFound();

            task.Status = status;
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






        /*public IActionResult GetCalendarEvents()
        {
            var events = _context.TaskItems
                .Where(t => t.StartTime.HasValue)
                .Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    start = t.EndTime.Value.ToString("yyyy-MM-dd"),
                    allDay = true,
                    status = t.Status.ToString() // המרת ה-enum למחרוזת
                })
                .ToList();

            return Json(events);
        }
        public IActionResult Timer()
        {
            ViewBag.MainTaskId = new SelectList(_context.mainTasks, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Timer([Bind("Id,Title,MainTaskId,Description,Status,Priority,StartTime,EndTime")] TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }



            return View(taskItem);
        }*/


    }
}
