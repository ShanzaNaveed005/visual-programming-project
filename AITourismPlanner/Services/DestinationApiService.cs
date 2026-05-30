using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AITourismPlanner.Services
{
    public class DestinationInfo
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int Population { get; set; }
        public List<NearbyPlace> Attractions { get; set; } = new();

        // ✅ YEH METHOD ADD KARO - Image URL generate karne ke liye
        public string GetImageUrl(int width = 800, int height = 600)
        {
            if (string.IsNullOrEmpty(Name))
                return "https://source.unsplash.com/featured/800x600/?travel";

            return $"https://source.unsplash.com/featured/{width}x{height}/?{Uri.EscapeDataString(Name)},travel";
        }
    }

    public class NearbyPlace
    {
        public string Xid { get; set; }
        public string Name { get; set; }
        public string Kinds { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Distance { get; set; }
    }

    public interface IDestinationApiService
    {
        Task<DestinationInfo> GetDestinationInfoAsync(string cityName);
        Task<List<NearbyPlace>> GetNearbyAttractionsAsync(double lat, double lon, int radius = 10000);
    }

    public class DestinationApiService : IDestinationApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _host = "opentripmap-places-v1.p.rapidapi.com";

        public DestinationApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenTripMap:Key"];
        }

        public async Task<DestinationInfo> GetDestinationInfoAsync(string cityName)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(
                        $"https://opentripmap-places-v1.p.rapidapi.com/en/places/geoname?name={cityName}"),
                    Headers =
                    {
                        { "X-RapidAPI-Key", _apiKey },
                        { "X-RapidAPI-Host", _host },
                    },
                };

                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(json);

                if (data?.status?.ToString() != "OK") return null;

                var info = new DestinationInfo
                {
                    Name = data?.name?.ToString(),
                    Country = data?.country?.ToString(),
                    Lat = double.Parse(data?.lat?.ToString()),
                    Lon = double.Parse(data?.lon?.ToString()),
                    Population = data?.population != null
                        ? int.Parse(data.population.ToString()) : 0
                };

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
                    {
                        { "X-RapidAPI-Key", _apiKey },
                        { "X-RapidAPI-Host", _host },
                    },
                };

                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(json);

                if (data?.features != null)
                {
                    foreach (var feature in data.features)
                    {
                        string name = feature?.properties?.name?.ToString();
                        if (string.IsNullOrEmpty(name)) continue;

                        places.Add(new NearbyPlace
                        {
                            Xid = feature?.properties?.xid?.ToString(),
                            Name = name,
                            Kinds = feature?.properties?.kinds?.ToString(),
                            Lat = double.Parse(
                                feature?.geometry?.coordinates[1]?.ToString()),
                            Lon = double.Parse(
                                feature?.geometry?.coordinates[0]?.ToString()),
                            Distance = double.Parse(
                                feature?.properties?.dist?.ToString())
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NearbyPlaces Error: {ex.Message}");
            }

            return places;
        }
    }
}