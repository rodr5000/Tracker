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

namespace Tracker.Controllers
{
    public class TimeLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TimeLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TimeLogs
        public async Task<IActionResult> IndexTL()
        {
            var applicationDbContext = _context.TimeLogs.Include(t => t.TaskItem);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TimeLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timeLog = await _context.TimeLogs
                .Include(t => t.TaskItem)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (timeLog == null)
            {
                return NotFound();
            }

            return View(timeLog);
        }

        // GET: TimeLogs/Create
        public IActionResult Create()
        {
            ViewData["TaskItemId"] = new SelectList(_context.TaskItems, "Id", "Title");
            return View();
        }

        // POST: TimeLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TaskItemId,StartTime,EndTime,Note")] TimeLog timeLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(timeLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexTL));
            }
            ViewData["TaskItemId"] = new SelectList(_context.TaskItems, "Id", "Title", timeLog.TaskItemId);
            return View(timeLog);
        }

        // GET: TimeLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timeLog = await _context.TimeLogs.FindAsync(id);
            if (timeLog == null)
            {
                return NotFound();
            }
            ViewData["TaskItemId"] = new SelectList(_context.TaskItems, "Id", "Title", timeLog.TaskItemId);
            return View(timeLog);
        }

        // POST: TimeLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TaskItemId,StartTime,EndTime,Note")] TimeLog timeLog)
        {
            if (id != timeLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
              
                return RedirectToAction(nameof(IndexTL));
            }
            ViewData["TaskItemId"] = new SelectList(_context.TaskItems, "Id", "Title", timeLog.TaskItemId);
            return View(timeLog);
        }

        // GET: TimeLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timeLog = await _context.TimeLogs
                .Include(t => t.TaskItem)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (timeLog == null)
            {
                return NotFound();
            }

            return View(timeLog);
        }

        // POST: TimeLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var timeLog = await _context.TimeLogs.FindAsync(id);
            if (timeLog != null)
            {
                _context.TimeLogs.Remove(timeLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexTL));
        }


      


    }
}
