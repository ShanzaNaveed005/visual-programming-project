using Newtonsoft.Json;

namespace AITourismPlanner.Services
{
<<<<<<< HEAD
    public class DestinationInfo
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int Population { get; set; }
        public List<NearbyPlace> Attractions { get; set; } = new();

        // ✅ YEH METHOD ADD KARO - Image URL generate karne ke liye

    }

    public class NearbyPlace
    {
        public string Xid { get; set; }
        public string Name { get; set; }
        public string Kinds { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Distance { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }

    public interface IDestinationApiService
    {
        Task<DestinationInfo> GetDestinationInfoAsync(string cityName);
        Task<List<NearbyPlace>> GetNearbyAttractionsAsync(double lat, double lon, int radius = 10000);
    }

=======
>>>>>>> 7adf219ef362a34ab5d4df51b1979f2ab1fc82d4
    public class DestinationApiService : IDestinationApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public DestinationApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["TravelDataAPI:ApiKey"] ?? "";
        }

        public async Task<List<DestinationApiModel>> GetPopularDestinationsAsync(int limit = 12)
        {
            var destinations = GetPakistaniDestinations();
            return destinations.Take(limit).ToList();
        }

        public async Task<List<DestinationApiModel>> SearchDestinationsAsync(string query, int limit = 20)
        {
            if (string.IsNullOrEmpty(query))
                return GetPakistaniDestinations().Take(limit).ToList();

            var allDestinations = GetPakistaniDestinations();
            var results = allDestinations
                .Where(d => d.Name.ToLower().Contains(query.ToLower()) ||
                           d.State.ToLower().Contains(query.ToLower()))
                .Take(limit)
                .ToList();

            // Try to fetch from API for better results
            try
            {
                var url = $"https://travel-data-api.omkar.cloud/travel/hotels/search?query={Uri.EscapeDataString(query)}%20Pakistan&limit={limit}";
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url),
                    Headers = { { "API-Key", _apiKey } }
                };

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(json);

<<<<<<< HEAD
                // Nearby attractions bhi lo
                info.Attractions = await GetNearbyAttractionsAsync(info.Lat, info.Lon);

                return info;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DestinationAPI Error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<NearbyPlace>> GetNearbyAttractionsAsync(
           double lat, double lon, int radius = 10000)
        {
            var places = new List<NearbyPlace>();

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(
                        $"https://opentripmap-places-v1.p.rapidapi.com/en/places/radius" +
                        $"?radius={radius}" +
                        $"&lon={lon}" +
                        $"&lat={lat}" +
                        $"&kinds=interesting_places" +
                        $"&limit=10"),
                    Headers =
=======
                    if (data?.data != null && data.data.Count > 0)
>>>>>>> 7adf219ef362a34ab5d4df51b1979f2ab1fc82d4
                    {
                        foreach (var item in data.data)
                        {
                            var name = item?.name?.ToString();
                            if (!string.IsNullOrEmpty(name) && !results.Any(r => r.Name == name))
                            {
                                results.Add(new DestinationApiModel
                                {
                                    Name = name,
                                    Country = "Pakistan",
                                    State = item?.state?.ToString() ?? "",
                                    Description = item?.description?.ToString() ?? $"Beautiful {name} in Pakistan",
                                    ImageUrl = item?.images != null && item.images.Count > 0 ?
                                        item.images[0]?.ToString() : GetImageForCity(name),
                                    Category = item?.category?.ToString() ?? "Attraction",
                                    EstimatedCost = 30000,
                                    Latitude = item?.latitude != null ? double.Parse(item.latitude.ToString()) : 0,
                                    Longitude = item?.longitude != null ? double.Parse(item.longitude.ToString()) : 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
            }

            return results;
        }

        public async Task<DestinationDetailModel> GetDestinationDetailAsync(string name)
        {
            var allDestinations = GetPakistaniDestinations();
            var destination = allDestinations.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());

            if (destination == null)
                return null;

            return new DestinationDetailModel
            {
                Name = destination.Name,
                Country = destination.Country,
                State = destination.State,
                Latitude = destination.Latitude,
                Longitude = destination.Longitude,
                Description = GetDescriptionForCity(name),
                ImageUrl = GetImageForCity(name),
                Category = destination.Category,
                EstimatedCost = destination.EstimatedCost,
                Rating = destination.Rating,
                ThingsToDo = GetThingsToDo(name),
                BestTimeToVisit = GetBestTimeToVisit(name)
            };
        }

        private List<DestinationApiModel> GetPakistaniDestinations()
        {
            return new List<DestinationApiModel>
            {
                new DestinationApiModel { Name = "Hunza Valley", Country = "Pakistan", State = "Gilgit-Baltistan", Latitude = 36.3167, Longitude = 74.6500, EstimatedCost = 50000, Category = "Mountains", Rating = 4.9, Description = "Breathtaking valley with ancient forts, stunning mountain views, and warm hospitality." },
                new DestinationApiModel { Name = "Murree", Country = "Pakistan", State = "Punjab", Latitude = 33.9062, Longitude = 73.3903, EstimatedCost = 25000, Category = "Mountains", Rating = 4.7, Description = "Popular hill station with pine forests, colonial architecture, and beautiful viewpoints." },
                new DestinationApiModel { Name = "Skardu", Country = "Pakistan", State = "Gilgit-Baltistan", Latitude = 35.2971, Longitude = 75.6333, EstimatedCost = 70000, Category = "Adventure", Rating = 4.9, Description = "Gateway to K2 with stunning lakes, cold desert, and breathtaking mountain views." },
                new DestinationApiModel { Name = "Swat Valley", Country = "Pakistan", State = "Khyber Pakhtunkhwa", Latitude = 35.2222, Longitude = 72.4250, EstimatedCost = 35000, Category = "Valleys", Rating = 4.8, Description = "Switzerland of the East - lush green valleys, rivers, and rich Buddhist heritage." },
                new DestinationApiModel { Name = "Naran Kaghan", Country = "Pakistan", State = "Khyber Pakhtunkhwa", Latitude = 34.9100, Longitude = 73.6500, EstimatedCost = 30000, Category = "Valleys", Rating = 4.7, Description = "Beautiful lakes including Saif-ul-Malook, stunning mountain views, and adventure activities." },
                new DestinationApiModel { Name = "Lahore", Country = "Pakistan", State = "Punjab", Latitude = 31.5204, Longitude = 74.3587, EstimatedCost = 40000, Category = "Historical", Rating = 4.8, Description = "Cultural heart of Pakistan with Mughal architecture, amazing food, and vibrant festivals." },
                new DestinationApiModel { Name = "Islamabad", Country = "Pakistan", State = "Islamabad", Latitude = 33.6844, Longitude = 73.0479, EstimatedCost = 45000, Category = "Urban", Rating = 4.6, Description = "Modern capital with beautiful parks, Faisal Mosque, and Daman-e-Koh viewpoint." },
                new DestinationApiModel { Name = "Karachi", Country = "Pakistan", State = "Sindh", Latitude = 24.8607, Longitude = 67.0011, EstimatedCost = 50000, Category = "Beach", Rating = 4.5, Description = "City of lights with beautiful beaches, vibrant food scene, and rich history." },
                new DestinationApiModel { Name = "Fairy Meadows", Country = "Pakistan", State = "Gilgit-Baltistan", Latitude = 35.3167, Longitude = 74.5833, EstimatedCost = 80000, Category = "Adventure", Rating = 4.9, Description = "Heaven on Earth with breathtaking Nanga Parbat views and amazing trekking." },
                new DestinationApiModel { Name = "Neelum Valley", Country = "Pakistan", State = "Azad Kashmir", Latitude = 34.4167, Longitude = 73.9167, EstimatedCost = 45000, Category = "Valleys", Rating = 4.8, Description = "Paradise on Earth with crystal clear rivers, lush forests, and stunning views." }
            };
        }

        private string GetImageForCity(string name)
        {
            var images = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Hunza", "https://images.unsplash.com/photo-1587925358603-c2eea5305bbc?w=800" },
                { "Murree", "https://images.unsplash.com/photo-1587925358603-c2eea5305bbc?w=800" },
                { "Skardu", "https://images.unsplash.com/photo-1626621341517-bbfa3c999d9a?w=800" },
                { "Swat", "https://images.unsplash.com/photo-1590041792601-d8d0cf2b3405?w=800" },
                { "Naran", "https://images.unsplash.com/photo-1590041792601-d8d0cf2b3405?w=800" },
                { "Lahore", "https://images.unsplash.com/photo-1589561253898-768105ca91b2?w=800" },
                { "Islamabad", "https://images.unsplash.com/photo-1589561253898-768105ca91b2?w=800" },
                { "Karachi", "https://images.unsplash.com/photo-1587925358603-c2eea5305bbc?w=800" },
                { "Fairy Meadows", "https://images.unsplash.com/photo-1626621341517-bbfa3c999d9a?w=800" },
                { "Neelum", "https://images.unsplash.com/photo-1590041792601-d8d0cf2b3405?w=800" }
            };

            foreach (var kvp in images)
            {
                if (name.ToLower().Contains(kvp.Key.ToLower()))
                    return kvp.Value;
            }
            return "https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?w=800";
        }

        private string GetDescriptionForCity(string name)
        {
            var descriptions = new Dictionary<string, string>
            {
                { "Hunza", "Hunza Valley is a mountainous valley in Gilgit-Baltistan, known for breathtaking landscapes, ancient forts (Baltit & Altit), friendly locals, and stunning views of Rakaposhi. It's a paradise for nature lovers and adventure seekers." },
                { "Murree", "Murree is a popular hill station in Punjab, offering stunning views, pine forests, colonial-era architecture, and pleasant weather. Perfect for families and couples looking for a weekend getaway." },
                { "Skardu", "Skardu is the gateway to some of the world's highest peaks including K2. It features stunning landscapes, serene lakes (Shangrila, Satpara), cold desert, and ancient forts." },
                { "Swat", "Known as the Switzerland of the East, Swat Valley offers lush green valleys, rushing rivers, rich Buddhist history, and beautiful mountains. A perfect destination for nature lovers." },
                { "Lahore", "Lahore is the cultural heart of Pakistan, known for its Mughal architecture (Badshahi Mosque, Lahore Fort), amazing food, vibrant festivals, and warm hospitality." }
            };

            foreach (var kvp in descriptions)
            {
                if (name.ToLower().Contains(kvp.Key.ToLower()))
                    return kvp.Value;
            }
            return $"Beautiful {name} is a must-visit destination in Pakistan, offering unique experiences, stunning landscapes, and rich cultural heritage.";
        }

        private List<string> GetThingsToDo(string name)
        {
            var activities = new Dictionary<string, List<string>>
            {
                { "Hunza", new List<string> { "Visit Altit and Baltit Forts", "Drive on Karakoram Highway", "Visit Attabad Lake", "Explore Eagle's Nest viewpoint", "Walk on Hussaini Suspension Bridge" } },
                { "Murree", new List<string> { "Walk on Mall Road", "Visit Kashmir Point", "Ride at Pindi Point", "Visit Patriata Chairlift", "Explore Bhurban" } },
                { "Skardu", new List<string> { "Visit Shangrila Resort", "Explore Katpana Cold Desert", "Visit Manthokha Waterfall", "See Satpara Lake", "Visit K2 Base Camp" } },
                { "Lahore", new List<string> { "Visit Badshahi Mosque", "Explore Lahore Fort", "Enjoy food at Food Street", "Visit Wagah Border", "See Shalimar Gardens" } },
                { "Islamabad", new List<string> { "Visit Faisal Mosque", "Explore Daman-e-Koh", "Visit Pakistan Monument", "Walk at Lake View Park", "Explore Centaurus Mall" } }
            };

            foreach (var kvp in activities)
            {
                if (name.ToLower().Contains(kvp.Key.ToLower()))
                    return kvp.Value;
            }
            return new List<string> { "Sightseeing", "Photography", "Local cuisine tasting", "Cultural exploration", "Nature walks" };
        }

        private string GetBestTimeToVisit(string name)
        {
            var bestTimes = new Dictionary<string, string>
            {
                { "Hunza", "May to September (Summer) - Pleasant weather, blooming flowers, and clear mountain views" },
                { "Murree", "March to October (Summer/Autumn) - Mild weather, green landscapes, and beautiful scenery" },
                { "Skardu", "June to September (Summer) - Best for trekking, lake visits, and mountain views" },
                { "Swat", "April to October (Spring/Summer) - Lush green valleys, pleasant weather" },
                { "Lahore", "October to March (Winter) - Pleasant weather perfect for sightseeing and food tours" },
                { "Karachi", "November to February (Winter) - Best beach weather and outdoor activities" }
            };

            foreach (var kvp in bestTimes)
            {
                if (name.ToLower().Contains(kvp.Key.ToLower()))
                    return kvp.Value;
            }
            return "October to March (Winter months) - Best weather for sightseeing and outdoor activities";
        }

    }
}