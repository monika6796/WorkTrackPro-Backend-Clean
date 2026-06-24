using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;   //--//
using WorkTrackPro.API.Data;                //--//
using WorkTrackPro.API.Models;             //--//   

namespace WorkTrackPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]            //--//
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;                  //--//

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        // ➕ Add Task
        [HttpPost]
        public IActionResult AddTask(TaskItem task)
        {
            _context.Tasks.Add(task);
            _context.SaveChanges();

            return Ok(task);
        }

        // 📋 Get All Tasks
        [HttpGet]
        public IActionResult GetTasks()
        {
            var data = _context.Tasks.ToList();
            return Ok(data);
        }

        // 🔄 Update Status
        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, TaskItem task)
        {
            var data = _context.Tasks.Find(id);

            if (data == null)
            {
                return NotFound(new
                {
                    message = "Task not found"
                });
            }

            data.Title = task.Title;
            data.Description = task.Description;
            data.AssignedTo = task.AssignedTo;
            data.Status = task.Status;
            data.Date = task.Date;

            _context.SaveChanges();

            return Ok(new
            {
                message = "Task Updated Successfully"
            });
        }
        // ❌ Delete Task
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var data = _context.Tasks.Find(id);

            if (data == null)
            {
                return NotFound("Task not found");
            }

            _context.Tasks.Remove(data);
            _context.SaveChanges();

            return Ok("Task Deleted");
        }


        [HttpGet("performance-report")]
        public IActionResult PerformanceReport()
        {
            var totalTasks =
                _context.Tasks.Count();

            var completedTasks =
                _context.Tasks
                .Count(x => x.Status == "Completed");

            var pendingTasks =
                _context.Tasks
                .Count(x => x.Status == "Pending");

            var performance =
                totalTasks == 0
                ? 0
                : (completedTasks * 100) / totalTasks;

            var monthlyData =
                _context.Tasks
                .GroupBy(x => x.Date.Month)
                .Select(g => new
                {
                    month = g.Key,

                    completed =
                        g.Count(x =>
                            x.Status == "Completed"),

                    pending =
                        g.Count(x =>
                            x.Status == "Pending")
                })
                .OrderBy(x => x.month)
                .ToList();

            var recentTasks =
                _context.Tasks
                .OrderByDescending(x => x.Date)
                .Take(3)
                .ToList();

            return Ok(new
            {
                totalTasks,
                completedTasks,
                pendingTasks,
                performance,
                monthlyData,
                recentTasks
            });
        }
        [HttpGet("months")]

        public IActionResult GetMonths()
        {

            var months = new List<string>
    {
        "January",
        "February",
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December"
    };

            return Ok(months);

        }

        [HttpGet("performance-by-month/{month}")]

        public IActionResult GetPerformanceByMonth(string month)
        {

            int monthNumber =
                DateTime.ParseExact(
                    month,
                    "MMMM",
                    null
                ).Month;

            var tasks =
                _context.Tasks
                .Where(x =>
                    x.Date.Month == monthNumber)
                .ToList();

            var totalTasks =
                tasks.Count();

            var completedTasks =
                tasks.Count(x =>
                    x.Status == "Completed");

            var pendingTasks =
                tasks.Count(x =>
                    x.Status == "Pending");

            var performance =
                totalTasks == 0
                ? 0
                : (completedTasks * 100) / totalTasks;

            var monthlyData =
                tasks
                .GroupBy(x =>
                    ((x.Date.Day - 1) / 7) + 1)
                .Select(g => new
                {
                    week = g.Key,

                    completed =
                        g.Count(x =>
                            x.Status == "Completed"),

                    pending =
                        g.Count(x =>
                            x.Status == "Pending")
                })
                .OrderBy(x => x.week)
                .ToList();

            return Ok(new
            {
                totalTasks,
                completedTasks,
                pendingTasks,
                performance,
                monthlyData
            });

        }

        [HttpGet("dashboard-stats")]
        public IActionResult DashboardStats()
        {
            var totalTasks =
                _context.Tasks.Count();

            var completedTasks =
                _context.Tasks
                .Count(x => x.Status == "Completed");

            var pendingTasks =
                _context.Tasks
                .Count(x => x.Status == "Pending");

            var performance =
                totalTasks == 0
                ? 0
                : (completedTasks * 100) / totalTasks;

            var recentTasks =
                _context.Tasks
                .OrderByDescending(x => x.Date)
                .Take(3)
                .ToList();

            // ✅ Monthly Data
            var monthlyData =
                _context.Tasks
                .GroupBy(x => x.Date.Month)
                .Select(g => new
                {
                    month = g.Key,

                    completed = g.Count(x =>
                        x.Status == "Completed"),

                    pending = g.Count(x =>
                        x.Status == "Pending")
                })
                .OrderBy(x => x.month)
                .ToList();

            return Ok(new
            {
                totalTasks,
                completedTasks,
                pendingTasks,
                performance,
                recentTasks,
                monthlyData   // ✅ IMPORTANT
            });
        }
    }

}
