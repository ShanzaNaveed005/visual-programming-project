using Microsoft.AspNetCore.Mvc;
using AITourismPlanner.Services;

namespace AITourismPlanner.Controllers
{
    public class RealHotelsController : Controller
    {
        private readonly IRealHotelService _hotelService;

        public RealHotelsController(IRealHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string city = "Islamabad", string checkIn = null, string checkOut = null)
        {
            if (string.IsNullOrEmpty(checkIn))
                checkIn = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            if (string.IsNullOrEmpty(checkOut))
                checkOut = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd");

            ViewBag.City = city;
            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;

            var hotels = await _hotelService.SearchHotelsAsync(city, checkIn, checkOut);
            return View(hotels);
        }
    }
}