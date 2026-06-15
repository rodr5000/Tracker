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


namespace Tracker.Controllers
{
    
    [Authorize]
    public class MainTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MainTasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MainTasks
        public async Task<IActionResult> Index()
        {
            // Get the current user ID
            var userId = _userManager.GetUserId(User);

            // .Include is the magic that fills the item.TaskItems list
            var tasks = await _context.MainTasks
                .Include(m => m.TaskItems)
                .Where(m => m.UserId == userId)
                .ToListAsync();

            return View(tasks);
        }

        // GET: MainTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainTask = await _context.MainTasks
                .Include(m => m.TaskItems)
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
    [Bind("Id,Name,Description,CreatedAt,DueDate,Duration")] MainTask mainTask, // Removed User/UserId from Bind
    int Hours,
    int Minutes)
        {
            var userId = _userManager.GetUserId(User);

            // Assign values manually
            mainTask.UserId = userId;
            Console.WriteLine($" \n ================Hours: {Hours}, Minutes: {Minutes}=======================");
            mainTask.Duration = new TimeSpan(Hours, Minutes, 0);

            // REMOVE these from validation since they aren't coming from the form
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                _context.Add(mainTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we got here, check the Errors collection in your debugger to see what else failed
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt,DueDate,Duration")] MainTask mainTask, int Hours, int Minutes)
        {
            if (id != mainTask.Id)
            {
                return NotFound();
            }

            // 1. Re-assign the UserId so it doesn't get overwritten with NULL
            var userId = _userManager.GetUserId(User);
            mainTask.UserId = userId;

            // 2. Set the Duration from the extra inputs
            Console.WriteLine($"Hours: {Hours}, Minutes: {Minutes}");
            mainTask.Duration = new TimeSpan(Hours, Minutes, 0);

            // 3. Clear validation for properties we handle manually
            ModelState.Remove("UserId");
            ModelState.Remove("User");

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
