using AITourismPlanner.Data;
using AITourismPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AITourismPlanner.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // DASHBOARD
        // =========================================================
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            var stats = new AdminDashboardViewModel
            {
                TotalUsers = await _context.users.CountAsync(),
                TotalDestinations = await _context.destinations.CountAsync(),
                TotalBookings = await _context.hotel_bookings.CountAsync(),
                TotalRevenue = await _context.hotel_bookings
                    .Where(b => b.total_amount.HasValue)
                    .SumAsync(b => b.total_amount ?? 0),
                RecentBookings = await _context.hotel_bookings
                    .Include(b => b.User)
                    .Include(b => b.Hotel)
                    .OrderByDescending(b => b.booking_date)
                    .Take(10)
                    .ToListAsync(),
                PopularDestinations = await _context.destinations
                    .OrderByDescending(d => d.rating_average)
                    .Take(5)
                    .ToListAsync()
            };

            return View(stats);
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalDestinations { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<HotelBooking> RecentBookings { get; set; }
        public List<Destination> PopularDestinations { get; set; }
    }
}