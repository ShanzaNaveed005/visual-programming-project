using Microsoft.AspNetCore.Mvc;
using AITourismPlanner.Services;
using AITourismPlanner.Models;

namespace AITourismPlanner.Controllers
{
    public class DestinationController : Controller
    {
        private readonly IGeoDbService _geoDbService;
        private readonly IWeatherService _weatherService;
        private readonly IHotelService _hotelService;

        // Constructor - Services inject karo
        public DestinationController(
            IGeoDbService geoDbService,
            IWeatherService weatherService,
            IHotelService hotelService)
        {
            _geoDbService = geoDbService;
            _weatherService = weatherService;
            _hotelService = hotelService;
        }

        [HttpGet]
        public async Task<IActionResult> ApiSearch(string q)
        {
            if (string.IsNullOrEmpty(q) || q.Length < 2)
                return Json(new List<object>());

            var cities = await _geoDbService.SearchCitiesAsync(q, 10);

            var results = cities.Select(c => new
            {
                c.Id,
                c.Name,
                c.Country,
                c.CountryCode,
                c.Population,
                ImageUrl = c.ImageUrl,
                DetailsUrl = Url.Action("ApiDestinationDetail", new { id = c.Id })
            });

            return Json(results);
        }

        [HttpGet]
        public async Task<IActionResult> ApiDestinationDetail(int id)
        {
            var city = await _geoDbService.GetCityDetailsAsync(id);

            if (city == null)
                return NotFound();

            // Get weather for this city
            var weather = await _weatherService.GetWeatherAsync(city.Name);

            // Get nearby hotels
            var hotels = new List<HotelApiModel>();
            if (city.Latitude.HasValue && city.Longitude.HasValue)
            {
                hotels = await _hotelService.GetNearbyHotelsByCoordsAsync(
                    city.Latitude.Value, city.Longitude.Value);
            }

            ViewBag.City = city;
            ViewBag.Weather = weather;
            ViewBag.Hotels = hotels;

            return View();
        }
    }
}