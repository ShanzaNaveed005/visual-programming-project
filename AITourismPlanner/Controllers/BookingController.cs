using AITourismPlanner.Data;
using AITourismPlanner.Models;
using AITourismPlanner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AITourismPlanner.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHotelApiService _hotelApi;
        private readonly ITransportApiService _transportApi;

        public BookingController(
            ApplicationDbContext context,
            IHotelApiService hotelApi,
            ITransportApiService transportApi)
        {
            _context = context;
            _hotelApi = hotelApi;
            _transportApi = transportApi;
        }

        // =========================================================
        // BOOKING WIZARD - STEP 1: Select Hotel
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Step1Hotel(string destination, DateTime? checkIn, DateTime? checkOut, int guests = 2)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            ViewBag.Destination = destination;
            ViewBag.CheckIn = checkIn ?? DateTime.Now.AddDays(7);
            ViewBag.CheckOut = checkOut ?? DateTime.Now.AddDays(9);
            ViewBag.Guests = guests;
            ViewBag.Nights = (ViewBag.CheckOut - ViewBag.CheckIn).Days;

            var hotels = await _hotelApi.GetNearbyHotelsAsync(destination, ViewBag.CheckIn, ViewBag.CheckOut, guests);

            return View(hotels);
        }

        // =========================================================
        // BOOKING WIZARD - STEP 2: Select Transport
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Step2Transport(string destination, string hotelId, string hotelName, decimal hotelPrice, DateTime checkIn, DateTime checkOut, int guests)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var transportOptions = await _transportApi.GetTransportOptionsAsync("Islamabad", destination, checkIn);

            ViewBag.Destination = destination;
            ViewBag.HotelId = hotelId;
            ViewBag.HotelName = hotelName;
            ViewBag.HotelPrice = hotelPrice;
            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;
            ViewBag.Guests = guests;
            ViewBag.Nights = (checkOut - checkIn).Days;

            return View(transportOptions);
        }

        // =========================================================
        // BOOKING WIZARD - STEP 3: Passenger Details
        // =========================================================
        [HttpGet]
        public IActionResult Step3Details(string destination, string hotelId, string hotelName, decimal hotelPrice,
            DateTime checkIn, DateTime checkOut, int guests, string transportType, string transportCompany, decimal transportFare)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var nights = (checkOut - checkIn).Days;
            var totalHotelCost = hotelPrice * nights;
            var totalPrice = totalHotelCost + transportFare;

            var model = new BookingDetailsViewModel
            {
                Destination = destination,
                HotelId = hotelId,
                HotelName = hotelName,
                HotelPricePerNight = hotelPrice,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Nights = nights,
                Guests = guests,
                TransportType = transportType,
                TransportCompany = transportCompany,
                TransportFare = transportFare,
                TotalHotelCost = totalHotelCost,
                TotalPrice = totalPrice
            };

            return View(model);
        }

        // =========================================================
        // BOOKING WIZARD - STEP 4: Confirm Booking
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(BookingDetailsViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var bookingRef = GenerateBookingReference();

                var booking = new Booking
                {
                    booking_reference = bookingRef,
                    user_id = userId.Value,
                    destination_name = model.Destination,
                    hotel_id = model.HotelId,
                    hotel_name = model.HotelName,
                    hotel_price_per_night = model.HotelPricePerNight,
                    check_in_date = model.CheckIn,
                    check_out_date = model.CheckOut,
                    number_of_guests = model.Guests,
                    nights = model.Nights,
                    total_hotel_cost = model.TotalHotelCost,
                    transport_type = model.TransportType,
                    transport_company = model.TransportCompany,
                    transport_fare = model.TransportFare,
                    total_transport_cost = model.TransportFare,
                    total_price = model.TotalPrice,
                    passenger_name = model.PassengerName,
                    passenger_email = model.PassengerEmail,
                    passenger_phone = model.PassengerPhone,
                    special_requests = model.SpecialRequests,
                    booking_status = "Confirmed",
                    payment_status = "Pending",
                    created_at = DateTime.Now
                };

                _context.bookings.Add(booking);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Booking confirmed! Reference: {bookingRef}";
                return RedirectToAction("Confirmation", new { reference = bookingRef });
            }

            return View("Step3Details", model);
        }

        // =========================================================
        // BOOKING CONFIRMATION
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Confirmation(string reference)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var booking = await _context.bookings
                .FirstOrDefaultAsync(b => b.booking_reference == reference && b.user_id == userId);

            if (booking == null)
                return NotFound();

            return View(booking);
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

            var bookings = await _context.bookings
                .Where(b => b.user_id == userId)
                .OrderByDescending(b => b.created_at)
                .ToListAsync();

            return View(bookings);
        }

        // =========================================================
        // CANCEL BOOKING
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var booking = await _context.bookings.FindAsync(id);

            if (booking == null || booking.user_id != userId)
                return Json(new { success = false, message = "Booking not found" });

            if (booking.check_in_date < DateTime.Now.Date)
                return Json(new { success = false, message = "Cannot cancel past booking" });

            booking.booking_status = "Cancelled";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Booking cancelled successfully" });
        }

        private string GenerateBookingReference()
        {
            return "PKG" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);
        }
    }

    public class BookingDetailsViewModel
    {
        public string Destination { get; set; } = string.Empty;
        public string HotelId { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public decimal HotelPricePerNight { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Nights { get; set; }
        public int Guests { get; set; }
        public string TransportType { get; set; } = string.Empty;
        public string TransportCompany { get; set; } = string.Empty;
        public decimal TransportFare { get; set; }
        public decimal TotalHotelCost { get; set; }
        public decimal TotalPrice { get; set; }

        [Required]
        public string PassengerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string PassengerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PassengerPhone { get; set; } = string.Empty;

        public string SpecialRequests { get; set; } = string.Empty;
    }
}