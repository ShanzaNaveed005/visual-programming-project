using AITourismPlanner.Data;
using AITourismPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace AITourismPlanner.Controllers
{
    public class TransportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // SEARCH TRANSPORT
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Search(string from, string to, DateTime? date, string type = "All")
        {
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.Date = date ?? DateTime.Now.AddDays(1);
            ViewBag.Type = type;

            var query = _context.transports
                .Where(t => t.is_active)
                .AsQueryable();

            if (!string.IsNullOrEmpty(from))
                query = query.Where(t => t.departure_city.Contains(from));

            if (!string.IsNullOrEmpty(to))
                query = query.Where(t => t.arrival_city.Contains(to));

            if (type != "All")
                query = query.Where(t => t.transport_type == type);

            var transports = await query
                .OrderBy(t => t.departure_time)
                .ToListAsync();

            // Get unique cities for filter
            ViewBag.Cities = await _context.transports
                .Select(t => t.departure_city)
                .Distinct()
                .ToListAsync();

            return View(transports);
        }

        // =========================================================
        // TRANSPORT DETAILS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id, DateTime? date)
        {
            var transport = await _context.transports
                .FirstOrDefaultAsync(t => t.transport_id == id);

            if (transport == null)
                return NotFound();

            var journeyDate = date ?? DateTime.Now.AddDays(1);
            ViewBag.JourneyDate = journeyDate;

            // Check seat availability
            var bookedSeats = await _context.transport_bookings
                .Where(b => b.transport_id == id &&
                       b.journey_date.Date == journeyDate.Date &&
                       b.status != "Cancelled")
                .SumAsync(b => b.seats_booked);

            ViewBag.BookedSeats = bookedSeats;
            ViewBag.AvailableSeats = transport.available_seats - bookedSeats;

            return View(transport);
        }

        // =========================================================
        // BOOK TRANSPORT - GET
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Book(int id, DateTime date)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var transport = await _context.transports
                .FindAsync(id);

            if (transport == null)
                return NotFound();

            var bookedSeats = await _context.transport_bookings
                .Where(b => b.transport_id == id && b.journey_date.Date == date.Date && b.status != "Cancelled")
                .SumAsync(b => b.seats_booked);

            var availableSeats = transport.available_seats - bookedSeats;

            if (availableSeats <= 0)
            {
                TempData["Error"] = "No seats available for this journey";
                return RedirectToAction("Details", new { id, date });
            }

            var model = new TransportBookingViewModel
            {
                TransportId = id,
                Transport = transport,
                JourneyDate = date,
                AvailableSeats = availableSeats,
                Fare = transport.fare
            };

            return View(model);
        }

        // =========================================================
        // BOOK TRANSPORT - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(TransportBookingViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var transport = await _context.transports.FindAsync(model.TransportId);
                if (transport == null)
                    return NotFound();

                // Check availability again
                var bookedSeats = await _context.transport_bookings
                    .Where(b => b.transport_id == model.TransportId &&
                           b.journey_date.Date == model.JourneyDate.Date &&
                           b.status != "Cancelled")
                    .SumAsync(b => b.seats_booked);

                var availableSeats = transport.available_seats - bookedSeats;

                if (model.Seats > availableSeats)
                {
                    TempData["Error"] = "Selected seats not available";
                    return RedirectToAction("Details", new { id = model.TransportId, date = model.JourneyDate });
                }

                // Generate booking reference
                var reference = GenerateBookingReference();

                var booking = new TransportBooking
                {
                    user_id = userId.Value,
                    transport_id = model.TransportId,
                    booking_reference = reference,
                    seats_booked = model.Seats,
                    journey_date = model.JourneyDate,
                    total_price = transport.fare * model.Seats,
                    passenger_name = model.PassengerName,
                    passenger_phone = model.PassengerPhone,
                    passenger_email = model.PassengerEmail,
                    status = "Confirmed",
                    payment_status = "Pending",
                    booking_date = DateTime.Now
                };

                _context.transport_bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Create notification
                var notification = new Notification
                {
                    user_id = userId.Value,
                    title = "Transport Booking Confirmed",
                    message = $"Your booking from {transport.departure_city} to {transport.arrival_city} on {model.JourneyDate:dd MMM yyyy} is confirmed. Reference: {reference}",
                    created_at = DateTime.Now,
                    is_read = false
                };
                _context.notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Booking confirmed! Reference: {reference}";
                return RedirectToAction("MyBookings");
            }

            model.Transport = await _context.transports.FindAsync(model.TransportId);
            return View(model);
        }

        // =========================================================
        // MY BOOKINGS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var bookings = await _context.transport_bookings
                .Include(b => b.Transport)
                .Where(b => b.user_id == userId)
                .OrderByDescending(b => b.booking_date)
                .ToListAsync();

            return View(bookings);
        }

        // =========================================================
        // BOOKING DETAILS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> BookingDetails(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var booking = await _context.transport_bookings
                .Include(b => b.Transport)
                .FirstOrDefaultAsync(b => b.booking_id == id);

            if (booking == null)
                return NotFound();

            if (booking.user_id != userId)
                return Forbid();

            return View(booking);
        }

        // =========================================================
        // CANCEL BOOKING
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var booking = await _context.transport_bookings.FindAsync(id);

            if (booking == null || booking.user_id != userId)
                return Json(new { success = false, message = "Booking not found" });

            if (booking.journey_date < DateTime.Now.Date)
                return Json(new { success = false, message = "Cannot cancel past journey" });

            booking.status = "Cancelled";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Booking cancelled successfully" });
        }

        // =========================================================
        // HELPER METHODS
        // =========================================================
        private string GenerateBookingReference()
        {
            return "TRP" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999).ToString();
        }
    }

    public class TransportBookingViewModel
    {
        public int TransportId { get; set; }
        public Transport Transport { get; set; }
        public DateTime JourneyDate { get; set; }
        public int Seats { get; set; } = 1;
        public int AvailableSeats { get; set; }
        public decimal Fare { get; set; }
        public decimal TotalPrice => Fare * Seats;

        [Required]
        [Display(Name = "Passenger Name")]
        public string PassengerName { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PassengerPhone { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string PassengerEmail { get; set; }
    }
}