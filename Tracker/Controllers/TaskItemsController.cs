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
using Tracker.Services;

using static System.Net.Mime.MediaTypeNames;



namespace Tracker.Controllers
{
    
    [Authorize]
    public class TaskItemsController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private GenericServiceClient _serviceClient;

        public TaskItemsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            // Create the service client
            _serviceClient = new GenericServiceClient();
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var tasks = await _context.TaskItems
                .Include(t => t.MainTask)
                .Include(t => t.TaskItemTags)
                .ThenInclude(tt => tt.Tag)
                .Where(t => t.UserId == userId)
                .ToListAsync();

            // ✅ מחזירים את הרשימה המקושרת והמסוננת
            return View(tasks);
        }

        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.Tags = _context.Tags.ToList();
            var taskItem = await _context.TaskItems
       .Include(t => t.MainTask)
       .Include(t => t.TaskItemTags)
           .ThenInclude(tt => tt.Tag)
       .FirstOrDefaultAsync(m => m.Id == id);
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

            ViewBag.Tags = _context.Tags.ToList();

            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     [Bind("Id,Title,MainTaskId,Description,Status,Priority,StartDate,DueDate,EstimatedTime")]
    Models.TaskItem taskItem,
     int[] selectedTags)
        {
            var userId = _userManager.GetUserId(User);

            // ✅ חובה לפני השמירה
            taskItem.UserId = userId;

            // זמן משוער
            if (Request.Form["EstimatedTime"].Count > 0)
            {
                double hours = double.Parse(Request.Form["EstimatedTime"]);
                taskItem.EstimatedTime = TimeSpan.FromHours(hours);
            }

            // בדיקות זמן
            if (taskItem.StartDate.HasValue && taskItem.DueDate.HasValue)
            {
                if (taskItem.StartDate >= taskItem.DueDate)
                {
                    ModelState.AddModelError("", "End time must be after start time.");
                }

                var maxDuration = TimeSpan.FromDays(30);

                if (taskItem.DueDate.Value - taskItem.StartDate.Value > maxDuration)
                {
                    ModelState.AddModelError("", "Task duration cannot be longer than 30 days.");
                }
            }

            // MainTask ברירת מחדל
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

            if (!ModelState.IsValid)
            {
                ViewBag.MainTaskId =
                    new SelectList(_context.MainTasks, "Id", "Name");

                ViewBag.Tags = _context.Tags.ToList();

                return View(taskItem);
            }

            // ✅ שמירת Task
            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            // ✅ שמירת Tags
            foreach (var tagId in selectedTags)
            {
                _context.TaskItemTags.Add(new TaskItemTag
                {
                    TaskItemId = taskItem.Id,
                    TagId = tagId
                });
            }

            await _context.SaveChangesAsync();

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
                    .Include(t => t.TaskItemTags)
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (taskItem == null) return NotFound();

            // Only show the user's own MainTasks in the dropdown
            ViewBag.MainTaskId = new SelectList(
                _context.MainTasks.Where(m => m.UserId == userId),
                "Id", "Name", taskItem.MainTaskId);

            ViewBag.Tags = await _context.Tags.ToListAsync();


            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
     int id,
     [Bind("Id,MainTaskId,UserId,Title,Description,Status,Priority,StartDate,DueDate,EstimatedTime")] Models.TaskItem taskItem,
     int[] selectedTags) // 💡 1. הוספנו את מערך ה-IDs של התגיות שנבחרו
        {
            taskItem.UserId = _userManager.GetUserId(User);

            if (id != taskItem.Id) return NotFound();

            // 1. Manually remove validation for properties NOT in the form
            ModelState.Remove("MainTask");
            ModelState.Remove("TaskItemTags"); // 💡 2. מונע מה-ModelState להיכשל בגלל קשרי הניווט של התגיות

            // time check
            if (taskItem.StartDate.HasValue && taskItem.DueDate.HasValue)
            {
                if (taskItem.StartDate >= taskItem.DueDate)
                {
                    ModelState.AddModelError("", "End time must be after start time.");
                }

                var maxDuration = TimeSpan.FromDays(30);
                if (taskItem.DueDate.Value - taskItem.StartDate.Value > maxDuration)
                {
                    ModelState.AddModelError("", "Task duration cannot be longer than 30 days.");
                }
            }

            // הדפסת שגיאות ל-Debug (הקוד המקורי שלך)
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
            }

            var estimatedHours = Request.Form["EstimatedTimeHours"];
            if (!string.IsNullOrEmpty(estimatedHours))
            {
                taskItem.EstimatedTime = TimeSpan.FromHours(double.Parse(estimatedHours));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // עדכון פרטי המשימה הבסיסיים
                    _context.Update(taskItem);
                    await _context.SaveChangesAsync();

                    // 💡 3. עדכון התגיות: קודם כל מוחקים את הקישורים הישנים של המשימה הזו
                    var oldTags = _context.TaskItemTags.Where(tt => tt.TaskItemId == taskItem.Id);
                    _context.TaskItemTags.RemoveRange(oldTags);
                    await _context.SaveChangesAsync();

                    // 💡 4. שומרים את התגיות החדשות שסומנו בטופס
                    if (selectedTags != null && selectedTags.Length > 0)
                    {
                        foreach (var tagId in selectedTags)
                        {
                            _context.TaskItemTags.Add(new TaskItemTag
                            {
                                TaskItemId = taskItem.Id,
                                TagId = tagId
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    // הפנייה חזרה (הקוד המקורי שלך מפנה ל-MainTasks, תשאיר לפי הנוחות שלך)
                    return RedirectToAction("Index", "MainTasks");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TaskItems.Any(e => e.Id == taskItem.Id)) return NotFound();
                    else throw;
                }
            }

            // אם הגענו לכאן - ה-Validation נכשל. מחזירים את ה-View וממלאים מחדש את הנתונים
            var userId = _userManager.GetUserId(User);
            ViewBag.MainTaskId = new SelectList(_context.MainTasks.Where(m => m.UserId == userId), "Id", "Name", taskItem.MainTaskId);

            // 💡 5. חובה לטעון מחדש את כל התגיות כדי שהצ'קבוקסים לא ייעלמו מהמסך אם חזרנו עם שגיאה
            ViewBag.Tags = _context.Tags.ToList();

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
                    t.DueDate.HasValue &&
                    t.DueDate.Value < now &&
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
                .Where(t => t.StartDate != null && t.DueDate != null)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.StartDate,
                    t.DueDate ,
                    t.Priority,
                    t.Status
                    
                })
                .ToList();

            var events = tasks.SelectMany(t => new[]
            {
        new {
            id = t.Id + "_start",
            taskId = t.Id,
            title = (t.Status == Status.Completed ? "✔ bg-text " :
                  t.Status == Status.Cancelled ? "❌ bg-text " :
                  "⏳ bg-text " )
                  + t.Title,
            start = t.StartDate,

              className = t.Status == Status.Completed ? "bg-low" :
                      t.Status == Status.Cancelled ? "bg-high" :
                      t.Priority == Priority.Critical ? "bg-high":
                      t.Priority == Priority.High ? "bg-high" :
                      "bg-medium"
        },
        new {
            id = t.Id + "_due",
            taskId = t.Id,
            title = (t.Status == Status.Completed ? "✔ " :
                  t.Status == Status.Cancelled ? "❌  " :
                  "⏳ ")
                  + t.Title,
            start = t.DueDate,
            className = t.Status == Status.Completed ? "bg-low" :
                      t.Status == Status.Cancelled ? "bg-high" :
                      t.Priority == Priority.Critical ? "bg-high":
                      t.Priority == Priority.High ? "bg-high" :
                      "bg-medium"
        }
    });

            return Json(events); 
        }


        
        [HttpPost]
        public async Task<IActionResult> UpdateTime([FromBody] UpdateTimeDto dto)
        {
            var task = await _context.TaskItems.FindAsync(dto.Id);
            if (task == null) return NotFound();

            task.StartDate = dto.StartDate;
            task.DueDate = dto.DueDate;
            task.TimeTaken = dto.DueDate - dto.DueDate;

            await _context.SaveChangesAsync();
            return Ok();
        }


        public class UpdateTimeDto
        {
            public int Id { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime DueDate { get; set; }
        }


        private int CalculateFocusScore(Models.TaskItem task, DateTime today)
        {
            int score = 0;

            // 🔥 1️⃣ Priority
            score += task.Priority switch
            {
                Priority.Critical => 80,
                Priority.High => 60,
                Priority.Medium => 40,
                Priority.Low => 20,
                _ => 0
            };

            // ⏳ 2️⃣ Due date urgency
            if (task.DueDate.HasValue)
            {
                var daysLeft = (task.DueDate.Value.Date - today).Days;

                if (daysLeft < 0)
                    score += 120; // 🚨 overdue
                else if (daysLeft == 0)
                    score += 80;  // today
                else if (daysLeft <= 2)
                    score += 50;
                else if (daysLeft <= 5)
                    score += 25;
            }

            // 🚀 3️⃣ Started but not finished
            if (task.TimeTaken.HasValue && task.TimeTaken > TimeSpan.Zero)
            {
                score += 20;
            }

            // 🧠 4️⃣ Estimated vs Actual (VERY IMPORTANT)
            if (task.EstimatedTime.HasValue)
            {
                var estimated = task.EstimatedTime.Value;
                var actual = task.TimeTaken ?? TimeSpan.Zero;

                if (actual > estimated)
                {
                    score += 100; // 🔥 you're behind
                }
                else
                {
                    var progress = actual.TotalSeconds / estimated.TotalSeconds;

                    if (progress > 0.7)
                        score += 40; // almost done → finish it
                    else if (progress > 0.3)
                        score += 20;
                }
            }

            // 📅 5️⃣ Starts today
            if (task.StartDate.HasValue && task.StartDate.Value.Date == today)
            {
                score += 30;
            }

            if (task.TaskItemTags != null && task.TaskItemTags.Any())
            {
                foreach (var itemTag in task.TaskItemTags)
                {
                    // מוודאים שהתגית עצמה נטענה ולא Null
                    if (itemTag.Tag != null && !string.IsNullOrEmpty(itemTag.Tag.Name))
                    {
                        // המרת השם לטקסט קטן כדי למנוע בעיות של capital letters (למשל urgent לעומת Urgent)
                        var tagName = itemTag.Tag.Name.ToLower().Trim();

                        score += tagName switch
                        {
                            "urgent" => 50,         // תגית חירום מקפיצה משמעותית
                            "work" => 25,           // משימות עבודה מקבלות עדיפות בשוטף
                            "study" => 20,          // משימות לימודים
                            "entertainment" => -15, // משימות פנאי מורידות ניקוד (כדי שלא יקפצו בטעות לראש הפוקוס)
                            _ => 0                  // תגיות אחרות לא משפיעות על הניקוד
                        };



                        
                    }
                }
            }
            return score;
        }


        public async Task<IActionResult> FocusToday()
        {
            var today = DateTime.Today;

            var tasks = await _context.TaskItems
                .Where(t => t.Status != Status.Completed &&
                            t.Status != Status.Cancelled)
                .ToListAsync();

            var scoredTasks = tasks
                .Select(t => new
                {
                    Task = t,
                    Score = CalculateFocusScore(t, today)
                })
                .OrderByDescending(t => t.Score)
                .ToList();

            return View(scoredTasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartWork(int id)
        {
            var userId = _userManager.GetUserId(User);

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null) return NotFound();

            if (!task.IsWorking)
            {
                task.WorkStartTime = DateTime.Now;
                task.IsWorking = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StopWork(int id)
        {
            var userId = _userManager.GetUserId(User);

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null) return NotFound();

            if (task.IsWorking && task.WorkStartTime.HasValue)
            {
                var sessionTime = DateTime.Now - task.WorkStartTime.Value;

                task.TimeTaken ??= TimeSpan.Zero;
                task.TimeTaken += sessionTime;

                task.WorkStartTime = null;
                task.IsWorking = false;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(
     [Bind("Id,Title,MainTaskId,Description,Status,Priority,StartDate,DueDate,EstimatedTime")]
    Models.TaskItem taskItem,
     int[] selectedTags)
        {
            var userId = _userManager.GetUserId(User);

            // ✅ חובה לפני השמירה
            taskItem.UserId = userId;

            // ***************** API *****************

            var inventory =await _serviceClient.CheckInventoryAsync(taskItem.Title);

            if (inventory == null)
            {
                ModelState.AddModelError("", "Item not found in inventory.");

                ViewBag.MainTaskId =
                    new SelectList(_context.MainTasks, "Id", "Name");

                ViewBag.Tags = _context.Tags.ToList();

                return View(taskItem);
            }

            taskItem.Title = inventory.ItemNumber;

            taskItem.Description =
                $"Item: {inventory.Item}\n" +
                $"Item Number: {inventory.ItemNumber}";

            taskItem.EstimatedTime =
                TimeSpan.FromHours(inventory.Quantity);

            taskItem.Status = Status.Pending;

            taskItem.Priority = Priority.Low;





            // ***************** API *****************


            // זמן משוער

            // MainTask ברירת מחדל
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

            if (!ModelState.IsValid)
            {
                ViewBag.MainTaskId =
                    new SelectList(_context.MainTasks, "Id", "Name");

                ViewBag.Tags = _context.Tags.ToList();

                return View(taskItem);
            }

            // ✅ שמירת Task
            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

       
            

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



    }


}

