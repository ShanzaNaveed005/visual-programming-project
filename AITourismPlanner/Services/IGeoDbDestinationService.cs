using Newtonsoft.Json;
using System.Text;

namespace AITourismPlanner.Services
{
    public interface IGeoDbDestinationService
    {
        Task<List<CityDestination>> SearchCitiesAsync(string namePrefix, int limit = 20);
        Task<CityDestination> GetCityDetailsAsync(int cityId);
        Task<List<CityDestination>> GetNearbyCitiesAsync(double lat, double lng, int radiusKm = 50);
        Task<List<CityDestination>> GetPopularDestinationsAsync(string countryCode = "PK", int limit = 10);
    }

    public class CityDestination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Region { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? Population { get; set; }
        public string Timezone { get; set; }
        public string WikiDataId { get; set; }
        public string ImageUrl => GetImageUrl(Name);

        private static string GetImageUrl(string cityName)
        {
            // Fallback to Unsplash images based on city name
            var images = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "islamabad", "https://images.unsplash.com/photo-1589561253898-768105ca91b2?w=500" },
                { "lahore", "https://images.unsplash.com/photo-1589561253898-768105ca91b2?w=500" },
                { "karachi", "https://images.unsplash.com/photo-1589561253898-768105ca91b2?w=500" },
                { "murree", "https://images.unsplash.com/photo-1587925358603-c2eea5305bbc?w=500" },
                { "hunza", "https://images.unsplash.com/photo-1587925358603-c2eea5305bbc?w=500" },
                { "skardu", "https://images.unsplash.com/photo-1626621341517-bbfa3c999d9a?w=500" }
            };

            foreach (var kvp in images)
            {
                if (cityName.ToLower().Contains(kvp.Key))
                    return kvp.Value;
            }
            return "https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?w=500";
        }
    }
}