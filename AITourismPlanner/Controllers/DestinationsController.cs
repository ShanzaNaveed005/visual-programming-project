using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AITourismPlanner.Data;
using AITourismPlanner.Models;
using AITourismPlanner.ViewModels;

namespace AITourismPlanner.Controllers
{
    public class DestinationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DestinationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX - List all destinations
        // =========================================================
        public async Task<IActionResult> Index(string searchTerm, string category, decimal? minBudget, decimal? maxBudget)
        {
            var query = _context.destinations
                .Include(d => d.Category)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.name.Contains(searchTerm) ||
                                         (d.description != null && d.description.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(d => d.Category != null && d.Category.category_name == category);
            }

            if (minBudget.HasValue)
            {
                query = query.Where(d => d.estimated_cost >= minBudget.Value);
            }

            if (maxBudget.HasValue)
            {
                query = query.Where(d => d.estimated_cost <= maxBudget.Value);
            }

            var destinations = await query
                .OrderByDescending(d => d.rating_average)
                .ToListAsync();

            var categories = await _context.categories.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategory = category;

            return View(destinations);
        }

        // =========================================================
        // DETAILS - View single destination
        // =========================================================
        public async Task<IActionResult> Details(int id)
        {
            var destination = await _context.destinations
                .Include(d => d.Category)
                .Include(d => d.Hotels)
                .Include(d => d.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(d => d.destination_id == id);

            if (destination == null)
            {
                return NotFound();
            }

            // Get weather info
            var weather = await _context.weather
                .FirstOrDefaultAsync(w => w.destination_id == id);

            // Get nearby emergency services
            var emergencyServices = await _context.emergency_services
                .Where(e => e.destination_id == id)
                .ToListAsync();

            var viewModel = new DestinationDetailViewModel
            {
                Destination = destination,
                Weather = weather,
                EmergencyServices = emergencyServices,
                SimilarDestinations = await _context.destinations
                    .Where(d => d.category_id == destination.category_id && d.destination_id != id)
                    .Take(3)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // =========================================================
        // SEARCH API - For AJAX calls
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return Json(new List<object>());
            }

            var results = await _context.destinations
                .Where(d => d.name.Contains(q))
                .Take(10)
                .Select(d => new { d.destination_id, d.name, d.city, d.thumbnail })
                .ToListAsync();

            return Json(results);
        }

        // =========================================================
        // ADD REVIEW
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int destinationId, int rating, string comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Please login to review" });
            }

            var review = new Review
            {
                destination_id = destinationId,
                user_id = userId.Value,
                rating = rating,
                review_text = comment,
                review_date = DateTime.Now
            };

            _context.reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update destination average rating
            var avgRating = await _context.reviews
                .Where(r => r.destination_id == destinationId)
                .AverageAsync(r => r.rating ?? 0);

            var destination = await _context.destinations.FindAsync(destinationId);
            if (destination != null)
            {
                destination.rating_average = (decimal)avgRating;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Review added successfully!" });
        }
        // =========================================================
        // ADVANCED SEARCH WITH FILTERS
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> AdvancedSearch(
            string searchTerm,
            string category,
            decimal? minBudget,
            decimal? maxBudget,
            string sortBy = "rating",
            int page = 1,
            int pageSize = 9)
        {
            var query = _context.destinations
                .Include(d => d.Category)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.name.Contains(searchTerm) ||
                                         (d.description != null && d.description.Contains(searchTerm)));
            }

            // Category filter
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(d => d.Category != null && d.Category.category_name == category);
            }

            // Budget filter
            if (minBudget.HasValue)
            {
                query = query.Where(d => d.estimated_cost >= minBudget.Value);
            }
            if (maxBudget.HasValue)
            {
                query = query.Where(d => d.estimated_cost <= maxBudget.Value);
            }

            // Sorting
            query = sortBy switch
            {
                "price_low" => query.OrderBy(d => d.estimated_cost),
                "price_high" => query.OrderByDescending(d => d.estimated_cost),
                "rating" => query.OrderByDescending(d => d.rating_average),
                "name" => query.OrderBy(d => d.name),
                _ => query.OrderByDescending(d => d.rating_average)
            };

            // Pagination
            var totalCount = await query.CountAsync();
            var destinations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.categories.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategory = category;
            ViewBag.MinBudget = minBudget;
            ViewBag.MaxBudget = maxBudget;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(destinations);
        }
    }

}