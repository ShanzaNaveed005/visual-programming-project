using AITourismPlanner.Models;

namespace AITourismPlanner.ViewModels
{
    public class HomeViewModel
    {
        public List<Destination> PopularDestinations { get; set; } = new();
        public List<Hotel> FeaturedHotels { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Review> Testimonials { get; set; } = new();
    }
}