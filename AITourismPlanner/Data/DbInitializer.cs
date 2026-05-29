using AITourismPlanner.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AITourismPlanner.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.categories.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Seed Categories
            var categories = new Category[]
            {
                new Category { category_name = "Adventure" },
                new Category { category_name = "Historical" },
                new Category { category_name = "Religious" },
                new Category { category_name = "Beach" },
                new Category { category_name = "Mountains" },
                new Category { category_name = "Honeymoon" }
            };
            await context.categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // Seed Roles
            var roles = new Role[]
            {
                new Role { role_name = "Admin" },
                new Role { role_name = "Customer" },
                new Role { role_name = "Agent" }
            };
            await context.roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();

            // Seed Admin User
            var adminUser = new User
            {
                full_name = "Admin User",
                email = "admin@aitourism.com",
                password_hash = HashPassword("admin123"),
                phone = "03001234567",
                gender = "Male",
                role_id = 1,
                status = "Active",
                created_at = DateTime.Now
            };
            await context.users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            // Seed Destinations
            var destinations = new Destination[]
            {
                new Destination { name = "Hunza Valley", city = "Hunza", country = "Pakistan", description = "Beautiful mountain valley with stunning views of Rakaposhi", estimated_cost = 50000, best_season = "Summer", category_id = 5, rating_average = 4.8m, thumbnail = "/images/hunza.jpg" },
                new Destination { name = "Murree", city = "Murree", country = "Pakistan", description = "Popular hill station with pine forests", estimated_cost = 25000, best_season = "Winter", category_id = 5, rating_average = 4.5m, thumbnail = "/images/murree.jpg" },
                new Destination { name = "Skardu", city = "Skardu", country = "Pakistan", description = "Gateway to the world's highest mountains", estimated_cost = 70000, best_season = "Summer", category_id = 1, rating_average = 4.9m, thumbnail = "/images/skardu.jpg" },
                new Destination { name = "Swat Valley", city = "Swat", country = "Pakistan", description = "Switzerland of the East", estimated_cost = 35000, best_season = "Spring", category_id = 5, rating_average = 4.6m, thumbnail = "/images/swat.jpg" },
                new Destination { name = "Naran Kaghan", city = "Naran", country = "Pakistan", description = "Beautiful lakes and valleys", estimated_cost = 30000, best_season = "Summer", category_id = 5, rating_average = 4.4m, thumbnail = "/images/naran.jpg" },
                new Destination { name = "Lahore", city = "Lahore", country = "Pakistan", description = "Cultural heart of Pakistan with rich history", estimated_cost = 40000, best_season = "Winter", category_id = 2, rating_average = 4.3m, thumbnail = "/images/lahore.jpg" },
                new Destination { name = "Islamabad", city = "Islamabad", country = "Pakistan", description = "Modern capital with beautiful parks", estimated_cost = 45000, best_season = "Spring", category_id = 2, rating_average = 4.4m, thumbnail = "/images/islamabad.jpg" },
                new Destination { name = "Fairy Meadows", city = "Fairy Meadows", country = "Pakistan", description = "Heaven on Earth with Nanga Parbat view", estimated_cost = 80000, best_season = "Summer", category_id = 1, rating_average = 4.9m, thumbnail = "/images/fairy-meadows.jpg" }
            };
            await context.destinations.AddRangeAsync(destinations);
            await context.SaveChangesAsync();

            // Seed Hotels
            var hotels = new Hotel[]
            {
                new Hotel { destination_id = 1, hotel_name = "Hunza Serena Hotel", address = "Hunza City", star_rating = 5, description = "Luxury hotel with mountain views", price_per_night = 15000, contact_number = "05811123456", email = "info@hunzaserena.com" },
                new Hotel { destination_id = 1, hotel_name = "Eagle's Nest Hotel", address = "Karimabad", star_rating = 4, description = "Amazing sunrise views", price_per_night = 10000, contact_number = "05811789012" },
                new Hotel { destination_id = 2, hotel_name = "Pearl Continental Murree", address = "Mall Road", star_rating = 4, description = "Comfortable family hotel", price_per_night = 10000, contact_number = "05111123456" },
                new Hotel { destination_id = 2, hotel_name = "Shelton Hotel", address = "GPO Chowk", star_rating = 3, description = "Budget friendly hotel", price_per_night = 6000, contact_number = "05111234567" },
                new Hotel { destination_id = 3, hotel_name = "Skardu Serena Hotel", address = "Skardu City", star_rating = 5, description = "Luxury in the mountains", price_per_night = 20000, contact_number = "05815123456" }
            };
            await context.hotels.AddRangeAsync(hotels);
            await context.SaveChangesAsync();

            

            // Seed Hotel Rooms
            var hotelRooms = new HotelRoom[]
            {
                new HotelRoom { hotel_id = 1, room_type = "Standard", room_price = 15000, total_rooms = 20, available_rooms = 15 },
                new HotelRoom { hotel_id = 1, room_type = "Deluxe", room_price = 20000, total_rooms = 10, available_rooms = 8 },
                new HotelRoom { hotel_id = 2, room_type = "Standard", room_price = 6000, total_rooms = 30, available_rooms = 20 }
            };
            await context.hotel_rooms.AddRangeAsync(hotelRooms);
            await context.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}