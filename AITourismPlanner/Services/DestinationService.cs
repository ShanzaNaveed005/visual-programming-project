using AITourismPlanner.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AITourismPlanner.Services
{
    public class DestinationService : IDestinationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public DestinationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OmkarAPI:ApiKey"] ?? "";
        }

        public async Task<List<DestinationApiModel>> GetAllDestinationsAsync()
        {
            var destinations = GetPakistaniDestinations();
            return await Task.FromResult(destinations);
        }

        public async Task<List<DestinationApiModel>> GetDestinationsByCategoryAsync(string category)
        {
            var allDestinations = GetPakistaniDestinations();
            if (string.IsNullOrEmpty(category))
                return await Task.FromResult(allDestinations);

            var filtered = allDestinations.Where(d => d.Category == category).ToList();
            return await Task.FromResult(filtered);
        }

        public async Task<DestinationApiModel> GetDestinationDetailAsync(string name)
        {
            var allDestinations = GetPakistaniDestinations();
            var destination = allDestinations.FirstOrDefault(d => d.Name == name);
            return await Task.FromResult(destination);
        }

        public async Task<List<DestinationApiModel>> SearchDestinationsAsync(string query)
        {
            var allDestinations = GetPakistaniDestinations();

            if (string.IsNullOrEmpty(query))
                return await Task.FromResult(allDestinations);

            var results = allDestinations
                .Where(d => d.Name.ToLower().Contains(query.ToLower()) ||
                           d.City.ToLower().Contains(query.ToLower()) ||
                           d.State.ToLower().Contains(query.ToLower()))
                .ToList();

            return await Task.FromResult(results);
        }

        public async Task<List<string>> GetAutocompleteSuggestionsAsync(string term)
        {
            var allDestinations = GetPakistaniDestinations();

            if (string.IsNullOrEmpty(term) || term.Length < 2)
                return await Task.FromResult(new List<string>());

            var suggestions = allDestinations
                .Where(d => d.Name.ToLower().Contains(term.ToLower()))
                .Select(d => d.Name)
                .Take(10)
                .ToList();

            return await Task.FromResult(suggestions);
        }

        // Complete Pakistani Tourist Destinations Data
        private List<DestinationApiModel> GetPakistaniDestinations()
        {
            return new List<DestinationApiModel>
            {
                // Gilgit-Baltistan
                new DestinationApiModel
                {
                    Id = 1,
                    Name = "Hunza Valley",
                    City = "Hunza",
                    State = "Gilgit-Baltistan",
                    Country = "Pakistan",
                    Description = "Hunza Valley is a mountainous valley in Gilgit-Baltistan, known for breathtaking landscapes, ancient forts (Baltit & Altit), friendly locals, and stunning views of Rakaposhi. It's a paradise for nature lovers and adventure seekers.",
                    ImageUrl = "https://images.unsplash.com/photo-1587925358603-c2eea5305bbc?w=800",
                    Category = "Mountains",
                    EstimatedCost = 50000,
                    Rating = 4.9,
                    Latitude = 36.3167,
                    Longitude = 74.6500,
                    ThingsToDo = new List<string> { "Visit Altit and Baltit Forts", "Drive on Karakoram Highway", "Visit Attabad Lake", "Explore Eagle's Nest viewpoint", "Walk on Hussaini Suspension Bridge" },
                    BestTimeToVisit = "May to September (Summer)",
                    Weather = "Pleasant summers, cold winters"
                },
                new DestinationApiModel
                {
                    Id = 2,
                    Name = "Skardu",
                    City = "Skardu",
                    State = "Gilgit-Baltistan",
                    Country = "Pakistan",
                    Description = "Skardu is the gateway to some of the world's highest peaks including K2. It features stunning landscapes, serene lakes (Shangrila, Satpara), cold desert, and ancient forts.",
                    ImageUrl = "https://images.unsplash.com/photo-1626621341517-bbfa3c999d9a?w=800",
                    Category = "Adventure",
                    EstimatedCost = 70000,
                    Rating = 4.9,
                    Latitude = 35.2971,
                    Longitude = 75.6333,
                    ThingsToDo = new List<string> { "Visit Shangrila Resort", "Explore Katpana Cold Desert", "Visit Manthokha Waterfall", "See Satpara Lake", "Visit K2 Base Camp" },
                    BestTimeToVisit = "June to September (Summer)",
                    Weather = "Mild summers, very cold winters"
                },
                new DestinationApiModel
                {
                    Id = 3,
                    Name = "Fairy Meadows",
                    City = "Fairy Meadows",
                    State = "Gilgit-Baltistan",
                    Country = "Pakistan",
                    Description = "Fairy Meadows offers breathtaking views of Nanga Parbat and is a paradise for trekkers. The lush green meadows surrounded by snow-capped peaks create a magical atmosphere.",
                    ImageUrl = "https://images.unsplash.com/photo-1626621341517-bbfa3c999d9a?w=800",
                    Category = "Adventure",
                    EstimatedCost = 80000,
                    Rating = 4.9,
                    Latitude = 35.3167,
                    Longitude = 74.5833,
                    ThingsToDo = new List<string> { "Trekking", "Camping", "Photography", "View Nanga Parbat" },
                    BestTimeToVisit = "June to August (Summer)",
                    Weather = "Cool, pleasant in summer"
                },

                // Punjab
                new DestinationApiModel
                {
                    Id = 4,
                    Name = "Murree",
                    City = "Murree",
                    State = "Punjab",
                    Country = "Pakistan",
                    Description = "Murree is a popular hill station in Punjab, offering stunning views, pine forests, colonial-era architecture, and pleasant weather. Perfect for families and couples looking for a weekend getaway.",
                    ImageUrl = "https://images.unsplash.com/photo-1587925358603-c2eea5305bbc?w=800",
                    Category = "Mountains",
                    EstimatedCost = 25000,
                    Rating = 4.7,
                    Latitude = 33.9062,
                    Longitude = 73.3903,
                    ThingsToDo = new List<string> { "Walk on Mall Road", "Visit Kashmir Point", "Ride at Pindi Point", "Visit Patriata Chairlift", "Explore Bhurban" },
                    BestTimeToVisit = "March to October (Summer/Autumn)",
                    Weather = "Mild summers, cold winters with snow"
                },
                new DestinationApiModel
                {
                    Id = 5,
                    Name = "Lahore",
                    City = "Lahore",
                    State = "Punjab",
                    Country = "Pakistan",
                    Description = "Lahore is the cultural heart of Pakistan, known for its Mughal architecture (Badshahi Mosque, Lahore Fort), amazing food, vibrant festivals, and warm hospitality.",
                    ImageUrl = "https://images.unsplash.com/photo-1589561253898-768105ca91b2?w=800",
                    Category = "Historical",
                    EstimatedCost = 40000,
                    Rating = 4.8,
                    Latitude = 31.5204,
                    Longitude = 74.3587,
                    ThingsToDo = new List<string> { "Visit Badshahi Mosque", "Explore Lahore Fort", "Enjoy food at Food Street", "Visit Wagah Border", "See Shalimar Gardens" },
                    BestTimeToVisit = "October to March (Winter)",
                    Weather = "Mild winters, hot summers"
                },

                // Khyber Pakhtunkhwa
                new DestinationApiModel
                {
                    Id = 6,
                    Name = "Swat Valley",
                    City = "Swat",
                    State = "Khyber Pakhtunkhwa",
                    Country = "Pakistan",
                    Description = "Known as the Switzerland of the East, Swat Valley offers lush green valleys, rushing rivers, rich Buddhist history, and beautiful mountains. A perfect destination for nature lovers.",
                    ImageUrl = "https://images.unsplash.com/photo-1590041792601-d8d0cf2b3405?w=800",
                    Category = "Valleys",
                    EstimatedCost = 35000,
                    Rating = 4.8,
                    Latitude = 35.2222,
                    Longitude = 72.4250,
                    ThingsToDo = new List<string> { "Visit Malam Jabba", "Explore Buddhist Stupas", "River rafting", "Visit Fizagat Park" },
                    BestTimeToVisit = "April to October (Spring/Summer)",
                    Weather = "Pleasant summers, cold winters"
                },
                new DestinationApiModel
                {
                    Id = 7,
                    Name = "Naran Kaghan",
                    City = "Naran",
                    State = "Khyber Pakhtunkhwa",
                    Country = "Pakistan",
                    Description = "Naran Kaghan is famous for its beautiful lakes, including Saif-ul-Malook, stunning mountain views, and adventure activities. A perfect summer destination.",
                    ImageUrl = "https://images.unsplash.com/photo-1590041792601-d8d0cf2b3405?w=800",
                    Category = "Valleys",
                    EstimatedCost = 30000,
                    Rating = 4.7,
                    Latitude = 34.9100,
                    Longitude = 73.6500,
                    ThingsToDo = new List<string> { "Visit Saif-ul-Malook Lake", "River rafting", "Horse riding", "Photography" },
                    BestTimeToVisit = "May to September (Summer)",
                    Weather = "Pleasant summers, very cold winters"
                },

                // Sindh
                new DestinationApiModel
                {
                    Id = 8,
                    Name = "Karachi",
                    City = "Karachi",
                    State = "Sindh",
                    Country = "Pakistan",
                    Description = "Karachi is Pakistan's largest city, known for its beaches, food scene, and bustling lifestyle. Clifton Beach, Port Grand, and Quaid's Mausoleum are major attractions.",
                    ImageUrl = "https://images.unsplash.com/photo-1587925358603-c2eea5305bbc?w=800",
                    Category = "Beach",
                    EstimatedCost = 50000,
                    Rating = 4.5,
                    Latitude = 24.8607,
                    Longitude = 67.0011,
                    ThingsToDo = new List<string> { "Visit Clifton Beach", "Explore Port Grand", "Visit Quaid's Mausoleum", "Shopping at Tariq Road" },
                    BestTimeToVisit = "November to February (Winter)",
                    Weather = "Mild winters, hot and humid summers"
                },

                // Azad Kashmir
                new DestinationApiModel
                {
                    Id = 9,
                    Name = "Neelum Valley",
                    City = "Neelum",
                    State = "Azad Kashmir",
                    Country = "Pakistan",
                    Description = "Neelum Valley features crystal clear rivers, lush forests, and stunning mountain views. Known as Paradise on Earth, it's a must-visit destination.",
                    ImageUrl = "https://images.unsplash.com/photo-1590041792601-d8d0cf2b3405?w=800",
                    Category = "Valleys",
                    EstimatedCost = 45000,
                    Rating = 4.8,
                    Latitude = 34.4167,
                    Longitude = 73.9167,
                    ThingsToDo = new List<string> { "River rafting", "Hiking", "Photography", "Visit Keran" },
                    BestTimeToVisit = "April to October (Summer)",
                    Weather = "Pleasant summers, cold winters"
                },

                // Islamabad Capital Territory
                new DestinationApiModel
                {
                    Id = 10,
                    Name = "Islamabad",
                    City = "Islamabad",
                    State = "Islamabad",
                    Country = "Pakistan",
                    Description = "Islamabad is the modern capital of Pakistan, known for its greenery, Faisal Mosque, peaceful environment, and beautiful parks.",
                    ImageUrl = "https://images.unsplash.com/photo-1589561253898-768105ca91b2?w=800",
                    Category = "Urban",
                    EstimatedCost = 45000,
                    Rating = 4.6,
                    Latitude = 33.6844,
                    Longitude = 73.0479,
                    ThingsToDo = new List<string> { "Visit Faisal Mosque", "Explore Daman-e-Koh", "Visit Pakistan Monument", "Walk at Lake View Park", "Explore Centaurus Mall" },
                    BestTimeToVisit = "October to March (Winter)",
                    Weather = "Mild winters, pleasant summers"
                }
            };
        }
    }
}