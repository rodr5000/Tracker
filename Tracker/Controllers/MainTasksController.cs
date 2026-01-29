using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Models;

namespace Tracker.Controllers
{
    public class MainTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MainTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MainTasks
        public async Task<IActionResult> Index()
        {
            var mainTasks = await _context.MainTasks
                              .Include(m => m.TaskItems) // <--- CRITICAL
                              .ToListAsync();
            return View(await _context.MainTasks.ToListAsync());
        }

        // GET: MainTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainTask = await _context.MainTasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mainTask == null)
            {
                return NotFound();
            }

            return View(mainTask);
        }

        // GET: MainTasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MainTasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("Id,Name,Description,CreatedAt,DueDate")] MainTask mainTask,
    int Hours,
    int Minutes)
        {
            mainTask.Duration = new TimeSpan(Hours, Minutes, 0);

            if (ModelState.IsValid)
            {
                _context.Add(mainTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mainTask);
        }


        // GET: MainTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainTask = await _context.MainTasks.FindAsync(id);
            if (mainTask == null)
            {
                return NotFound();
            }
            return View(mainTask);
        }

        // POST: MainTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt,DueDate")] MainTask mainTask ,int Hours,
    int Minutes)
        {
            mainTask.Duration = new TimeSpan(Hours, Minutes, 0);

            if (id != mainTask.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mainTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MainTaskExists(mainTask.Id))
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
            return View(mainTask);
        }

        // GET: MainTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainTask = await _context.MainTasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mainTask == null)
            {
                return NotFound();
            }

            return View(mainTask);
        }

        // POST: MainTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mainTask = await _context.MainTasks.FindAsync(id);
            if (mainTask != null)
            {
                _context.MainTasks.Remove(mainTask);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        private bool MainTaskExists(int id)
        {
            return _context.MainTasks.Any(e => e.Id == id);
        }
    }
}
