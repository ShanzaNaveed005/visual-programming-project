using Microsoft.AspNetCore.Mvc;
using AITourismPlanner.Services;

namespace AITourismPlanner.Controllers
{
    public class WeatherController : Controller
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string city = "Islamabad")
        {
            ViewBag.City = city;

            var currentWeather = await _weatherService.GetCurrentWeatherAsync(city);
            var forecast = await _weatherService.GetWeatherForecastAsync(city, 5);

            var viewModel = new WeatherViewModel
            {
                CurrentWeather = currentWeather,
                Forecast = forecast
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrent(string city)
        {
            var weather = await _weatherService.GetCurrentWeatherAsync(city);
            return Json(weather);
        }

        [HttpGet]
        public async Task<IActionResult> GetForecast(string city, int days = 5)
        {
            var forecast = await _weatherService.GetWeatherForecastAsync(city, days);
            return Json(forecast);
        }
    }

    public class WeatherViewModel
    {
        public WeatherData CurrentWeather { get; set; }
        public WeatherForecast Forecast { get; set; }
    }
}