using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AITourismPlanner.Data;
using AITourismPlanner.Models;
using AITourismPlanner.Services;
using AITourismPlanner.ViewModels;

namespace AITourismPlanner.Controllers
{
    public class DestinationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDestinationApiService _destinationApi;
        private readonly IHotelApiService _hotelApi;
        private readonly IWeatherApiService _weatherApi;
        private readonly ITransportApiService _transportApi;

        public DestinationController(
            ApplicationDbContext context,
            IDestinationApiService destinationApi,
            IHotelApiService hotelApi,
            IWeatherApiService weatherApi,
            ITransportApiService transportApi)
        {
            _context = context;
            _destinationApi = destinationApi;
            _hotelApi = hotelApi;
            _weatherApi = weatherApi;
            _transportApi = transportApi;
        }

        // =========================================================
        // HOMEPAGE
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var viewModel = new HomepageViewModel();

            // Featured destinations (top rated)
            var allDestinations = await _destinationApi.GetPopularDestinationsAsync();
            viewModel.FeaturedDestinations = allDestinations.Take(6).ToList();

            // Trending destinations (most viewed in last 7 days)
            var trendingNames = await _context.destination_views
                .Where(v => v.viewed_at >= DateTime.Now.AddDays(-7))
                .GroupBy(v => v.destination_name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(6)
                .Select(x => x.Name)
                .ToListAsync();

            viewModel.TrendingDestinations = allDestinations
                .Where(d => trendingNames.Contains(d.Name))
                .ToList();

            // Seasonal picks (Summer destinations)
            var summerDestinations = new[] { "Murree", "Naran Kaghan", "Hunza Valley", "Skardu", "Swat Valley" };
            viewModel.SeasonalPicks = allDestinations
                .Where(d => summerDestinations.Contains(d.Name))
                .Take(4)
                .ToList();

            // Recent reviews
            viewModel.RecentReviews = await _context.destination_reviews
                .Include(r => r.User)
                .OrderByDescending(r => r.created_at)
                .Take(6)
                .Select(r => new ReviewViewModel
                {
                    UserName = r.User.full_name,
                    DestinationName = r.destination_name,
                    Rating = r.rating,
                    Comment = r.comment,
                    CreatedAt = r.created_at
                })
                .ToListAsync();

            return View(viewModel);
        }

        // =========================================================
        // DESTINATION DETAILS
        // =========================================================
        public async Task<IActionResult> Details(string name)
        {
            if (string.IsNullOrEmpty(name))
                return RedirectToAction("Index");

            // Track view for trending
            var userId = HttpContext.Session.GetInt32("UserId");
            _context.destination_views.Add(new DestinationView
            {
                destination_name = name,
                user_id = userId
            });
            await _context.SaveChangesAsync();

            // Get destination info from API
            var destination = await _destinationApi.GetDestinationDetailAsync(name);
            if (destination == null)
                return RedirectToAction("Index");

            // Get hotels
            var checkIn = DateTime.Now.AddDays(7);
            var checkOut = DateTime.Now.AddDays(9);
            var hotels = await _hotelApi.GetNearbyHotelsAsync(name, checkIn, checkOut);

            // Get weather
            var currentWeather = await _weatherApi.GetCurrentWeatherAsync(name);
            var forecast = await _weatherApi.GetWeatherForecastAsync(name, 5);

            // Get transport options from Islamabad (major hub)
            var transportOptions = await _transportApi.GetTransportOptionsAsync("Islamabad", name, checkIn);

            // Get reviews
            var reviews = await _context.destination_reviews
                .Include(r => r.User)
                .Where(r => r.destination_name == name)
                .OrderByDescending(r => r.created_at)
                .Select(r => new ReviewViewModel
                {
                    ReviewId = r.review_id,
                    UserName = r.User.full_name,
                    Rating = r.rating,
                    Comment = r.comment,
                    CreatedAt = r.created_at,
                    CanEdit = r.user_id == userId
                })
                .ToListAsync();

            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            var viewModel = new DestinationDetailViewModel
            {
                Destination = destination,
                Hotels = hotels,
                CurrentWeather = currentWeather,
                WeatherForecast = forecast,
                TransportOptions = transportOptions,
                Reviews = reviews,
                AverageRating = averageRating,
                TotalReviews = reviews.Count,
                CheckIn = checkIn,
                CheckOut = checkOut
            };

            return View(viewModel);
        }

        // =========================================================
        // SEARCH DESTINATIONS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Search(string q, string category, decimal? minBudget, decimal? maxBudget, double? minRating, string sortBy = "rating")
        {
            var destinations = await _destinationApi.SearchDestinationsAsync(q, 50);

            // Apply filters
            if (!string.IsNullOrEmpty(category))
                destinations = destinations.Where(d => d.Category == category).ToList();

            if (minBudget.HasValue)
                destinations = destinations.Where(d => d.EstimatedCost >= minBudget.Value).ToList();

            if (maxBudget.HasValue)
                destinations = destinations.Where(d => d.EstimatedCost <= maxBudget.Value).ToList();

            if (minRating.HasValue)
                destinations = destinations.Where(d => d.Rating >= minRating.Value).ToList();

            // Apply sorting
            destinations = sortBy switch
            {
                "price_low" => destinations.OrderBy(d => d.EstimatedCost).ToList(),
                "price_high" => destinations.OrderByDescending(d => d.EstimatedCost).ToList(),
                "rating" => destinations.OrderByDescending(d => d.Rating).ToList(),
                "name" => destinations.OrderBy(d => d.Name).ToList(),
                _ => destinations.OrderByDescending(d => d.Rating).ToList()
            };

            ViewBag.Categories = new[] { "Mountains", "Valleys", "Historical", "Urban", "Beach", "Adventure" };
            ViewBag.SearchTerm = q;
            ViewBag.SelectedCategory = category;
            ViewBag.MinBudget = minBudget;
            ViewBag.MaxBudget = maxBudget;
            ViewBag.MinRating = minRating;
            ViewBag.SortBy = sortBy;

            return View(destinations);
        }

        // =========================================================
        // AUTOCOMPLETE
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Autocomplete(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
                return Json(new List<string>());

            var destinations = await _destinationApi.SearchDestinationsAsync(term, 10);
            return Json(destinations.Select(d => d.Name));
        }
    }
}