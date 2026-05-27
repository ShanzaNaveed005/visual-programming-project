using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AITourismPlanner.Data;
using AITourismPlanner.Models;
using AITourismPlanner.ViewModels;

namespace AITourismPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get destinations with NULL handling using raw SQL
                var destinations = new List<Destination>();
                var hotels = new List<Hotel>();
                var categories = new List<Category>();
                var testimonials = new List<Review>();

                // Fetch destinations using raw SQL to avoid NULL issues
                var destSql = @"SELECT 
                                    destination_id,
                                    IFNULL(name, 'Unknown Destination') AS name,
                                    IFNULL(description, 'No description available') AS description,
                                    IFNULL(city, 'Pakistan') AS city,
                                    IFNULL(country, 'Pakistan') AS country,
                                    IFNULL(estimated_cost, 30000) AS estimated_cost,
                                    IFNULL(rating_average, 0) AS rating_average,
                                    IFNULL(thumbnail, '/images/default.jpg') AS thumbnail,
                                    IFNULL(best_season, 'All Year') AS best_season,
                                    category_id
                                FROM destinations 
                                LIMIT 6";

                destinations = await _context.destinations
                    .FromSqlRaw(destSql)
                    .ToListAsync();

                // Fetch hotels using raw SQL
                var hotelSql = @"SELECT 
                                    hotel_id,
                                    IFNULL(hotel_name, 'Unknown Hotel') AS hotel_name,
                                    IFNULL(star_rating, 3) AS star_rating,
                                    IFNULL(price_per_night, 5000) AS price_per_night,
                                    IFNULL(image, '/images/hotel-default.jpg') AS image,
                                    IFNULL(address, 'Main City') AS address,
                                    destination_id
                                FROM hotels 
                                ORDER BY star_rating DESC 
                                LIMIT 4";

                hotels = await _context.hotels
                    .FromSqlRaw(hotelSql)
                    .ToListAsync();

                // Fetch categories
                var catSql = @"SELECT 
                                    category_id,
                                    IFNULL(category_name, 'General') AS category_name
                                FROM categories";

                categories = await _context.categories
                    .FromSqlRaw(catSql)
                    .ToListAsync();

                // Fetch reviews
                var reviewSql = @"SELECT 
                                    review_id,
                                    IFNULL(rating, 4) AS rating,
                                    IFNULL(review_text, 'Great experience!') AS review_text,
                                    review_date,
                                    user_id,
                                    destination_id
                                FROM reviews 
                                LIMIT 3";

                testimonials = await _context.reviews
                    .FromSqlRaw(reviewSql)
                    .ToListAsync();

                var viewModel = new HomeViewModel
                {
                    PopularDestinations = destinations,
                    FeaturedHotels = hotels,
                    Categories = categories,
                    Testimonials = testimonials
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Return empty view model on error
                return View(new HomeViewModel
                {
                    PopularDestinations = new List<Destination>(),
                    FeaturedHotels = new List<Hotel>(),
                    Categories = new List<Category>(),
                    Testimonials = new List<Review>()
                });
            }
        }

        public IActionResult About() => View();
        public IActionResult Contact() => View();
        public IActionResult Privacy() => View();
    }
}