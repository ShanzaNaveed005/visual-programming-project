using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using AITourismPlanner.Data;
using AITourismPlanner.Models;
using AITourismPlanner.ViewModels;

namespace AITourismPlanner.Controllers
{
    // Admin Authorization Filter
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");
            var userRoleId = context.HttpContext.Session.GetInt32("UserRoleId");

            if (string.IsNullOrEmpty(userRole) || (userRole != "Admin" && userRoleId != 1))
            {
                context.Result = new RedirectResult("/Account/Login?returnUrl=/Admin/Dashboard");
            }
        }
    }

    [AdminAuthorize]  // Only Admin can access this controller
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ADMIN DASHBOARD
        // =========================================================
        public async Task<IActionResult> Dashboard()
        {
            var adminName = HttpContext.Session.GetString("UserName");
            ViewBag.AdminName = adminName;

            var stats = new AdminDashboardViewModel
            {
                TotalUsers = await _context.users.CountAsync(),
                TotalCustomers = await _context.users.CountAsync(u => u.role_id == 2),
                TotalAgents = await _context.users.CountAsync(u => u.role_id == 3),
                TotalDestinations = await _context.destinations.CountAsync(),
                TotalHotels = await _context.hotels.CountAsync(),
                TotalBookings = await _context.hotel_bookings.CountAsync(),
                TotalRevenue = await _context.hotel_bookings
                    .Where(b => b.total_amount.HasValue && b.booking_status == "Confirmed")
                    .SumAsync(b => b.total_amount ?? 0),
                PendingBookings = await _context.hotel_bookings.CountAsync(b => b.booking_status == "Pending"),

                RecentUsers = await _context.users
                    .Include(u => u.Role)
                    .OrderByDescending(u => u.created_at)
                    .Take(10)
                    .ToListAsync(),

                RecentBookings = await _context.hotel_bookings
                    .Include(b => b.User)
                    .Include(b => b.Hotel)
                    .OrderByDescending(b => b.booking_date)
                    .Take(10)
                    .ToListAsync(),

                PopularDestinations = await _context.destinations
                    .OrderByDescending(d => d.rating_average)
                    .Take(5)
                    .ToListAsync(),

                RecentFeedbacks = await _context.reviews
                    .Include(r => r.User)
                    .Include(r => r.Destination)
                    .OrderByDescending(r => r.review_date)
                    .Take(5)
                    .ToListAsync(),

                MonthlyStats = await GetMonthlyStats()
            };

            return View(stats);
        }

        // =========================================================
        // MANAGE USERS
        // =========================================================
        public async Task<IActionResult> Users()
        {
            var users = await _context.users
                .Include(u => u.Role)
                .OrderByDescending(u => u.created_at)
                .ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserStatus(int userId, string status)
        {
            var user = await _context.users.FindAsync(userId);
            if (user != null)
            {
                user.status = status;
                await _context.SaveChangesAsync();

                // Log admin action
                await LogAdminAction($"Updated user {user.email} status to {status}");
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _context.users.FindAsync(userId);
            if (user != null && user.role_id != 1) // Cannot delete admin
            {
                _context.users.Remove(user);
                await _context.SaveChangesAsync();
                await LogAdminAction($"Deleted user {user.email}");
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // =========================================================
        // MANAGE DESTINATIONS
        // =========================================================
        public async Task<IActionResult> Destinations()
        {
            var destinations = await _context.destinations
                .Include(d => d.Category)
                .OrderByDescending(d => d.created_at)
                .ToListAsync();
            return View(destinations);
        }

        [HttpGet]
        public IActionResult AddDestination()
        {
            ViewBag.Categories = _context.categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDestination(Destination destination, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/destinations", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    destination.thumbnail = "/images/destinations/" + fileName;
                }

                destination.created_at = DateTime.Now;
                _context.destinations.Add(destination);
                await _context.SaveChangesAsync();

                await LogAdminAction($"Added new destination: {destination.name}");
                TempData["Success"] = "Destination added successfully!";
                return RedirectToAction("Destinations");
            }
            ViewBag.Categories = _context.categories.ToList();
            return View(destination);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDestination(int id)
        {
            var destination = await _context.destinations.FindAsync(id);
            if (destination != null)
            {
                _context.destinations.Remove(destination);
                await _context.SaveChangesAsync();
                await LogAdminAction($"Deleted destination: {destination.name}");
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // =========================================================
        // MANAGE HOTELS
        // =========================================================
        public async Task<IActionResult> Hotels()
        {
            var hotels = await _context.hotels
                .Include(h => h.Destination)
                .OrderByDescending(h => h.hotel_id)
                .ToListAsync();
            return View(hotels);
        }

        [HttpGet]
        public IActionResult AddHotel()
        {
            ViewBag.Destinations = _context.destinations.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHotel(Hotel hotel, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/hotels", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    hotel.image = "/images/hotels/" + fileName;
                }

                _context.hotels.Add(hotel);
                await _context.SaveChangesAsync();

                await LogAdminAction($"Added new hotel: {hotel.hotel_name}");
                TempData["Success"] = "Hotel added successfully!";
                return RedirectToAction("Hotels");
            }
            ViewBag.Destinations = _context.destinations.ToList();
            return View(hotel);
        }

        // =========================================================
        // MANAGE BOOKINGS
        // =========================================================
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.hotel_bookings
                .Include(b => b.User)
                .Include(b => b.Hotel)
                .OrderByDescending(b => b.booking_date)
                .ToListAsync();
            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, string status)
        {
            var booking = await _context.hotel_bookings.FindAsync(bookingId);
            if (booking != null)
            {
                booking.booking_status = status;
                await _context.SaveChangesAsync();

                await LogAdminAction($"Updated booking {bookingId} status to {status}");
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // =========================================================
        // REPORTS & ANALYTICS
        // =========================================================
        public async Task<IActionResult> Reports()
        {
            var reports = new AdminReportsViewModel
            {
                TotalRevenue = await _context.payments.SumAsync(p => p.amount ?? 0),
                MonthlyRevenue = await GetMonthlyRevenue(),
                TopDestinations = await _context.destinations
                    .OrderByDescending(d => d.rating_average)
                    .Take(10)
                    .ToListAsync(),
                BookingStats = await GetBookingStats()
            };
            return View(reports);
        }

        // =========================================================
        // ADMIN LOGS
        // =========================================================
        public async Task<IActionResult> Logs()
        {
            var logs = await _context.admin_logs
                .Include(l => l.Admin)
                .OrderByDescending(l => l.created_at)
                .Take(100)
                .ToListAsync();
            return View(logs);
        }

        // =========================================================
        // HELPER METHODS
        // =========================================================
        private async Task LogAdminAction(string action)
        {
            var adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId.HasValue)
            {
                var log = new AdminLog
                {
                    admin_id = adminId.Value,
                    action = action,
                    created_at = DateTime.Now
                };
                _context.admin_logs.Add(log);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<List<MonthlyStat>> GetMonthlyStats()
        {
            var stats = new List<MonthlyStat>();
            for (int i = 5; i >= 0; i--)
            {
                var month = DateTime.Now.AddMonths(-i);
                var bookings = await _context.hotel_bookings
                    .Where(b => b.booking_date.Month == month.Month && b.booking_date.Year == month.Year)
                    .CountAsync();

                stats.Add(new MonthlyStat
                {
                    Month = month.ToString("MMM"),
                    Bookings = bookings,
                    Revenue = await _context.hotel_bookings
                        .Where(b => b.booking_date.Month == month.Month && b.booking_date.Year == month.Year)
                        .SumAsync(b => b.total_amount ?? 0)
                });
            }
            return stats;
        }

        private async Task<decimal[]> GetMonthlyRevenue()
        {
            var revenue = new decimal[12];
            for (int i = 0; i < 12; i++)
            {
                var month = DateTime.Now.AddMonths(-i);
                revenue[i] = await _context.payments
                    .Where(p => p.payment_date.Month == month.Month && p.payment_date.Year == month.Year)
                    .SumAsync(p => p.amount ?? 0);
            }
            return revenue;
        }

        private async Task<BookingStatsViewModel> GetBookingStats()
        {
            return new BookingStatsViewModel
            {
                Total = await _context.hotel_bookings.CountAsync(),
                Pending = await _context.hotel_bookings.CountAsync(b => b.booking_status == "Pending"),
                Confirmed = await _context.hotel_bookings.CountAsync(b => b.booking_status == "Confirmed"),
                Cancelled = await _context.hotel_bookings.CountAsync(b => b.booking_status == "Cancelled")
            };
        }
    }

    // ViewModels for Admin
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalAgents { get; set; }
        public int TotalDestinations { get; set; }
        public int TotalHotels { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingBookings { get; set; }
        public List<User> RecentUsers { get; set; }
        public List<HotelBooking> RecentBookings { get; set; }
        public List<Destination> PopularDestinations { get; set; }
        public List<Review> RecentFeedbacks { get; set; }
        public List<MonthlyStat> MonthlyStats { get; set; }
    }

    public class MonthlyStat
    {
        public string Month { get; set; }
        public int Bookings { get; set; }
        public decimal Revenue { get; set; }
    }

    public class AdminReportsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal[] MonthlyRevenue { get; set; }
        public List<Destination> TopDestinations { get; set; }
        public BookingStatsViewModel BookingStats { get; set; }
    }

    public class BookingStatsViewModel
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Cancelled { get; set; }
    }
}