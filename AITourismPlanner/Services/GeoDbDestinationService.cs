using Microsoft.Extensions.Configuration;

namespace AITourismPlanner.Services
{
    public class GeoDbDestinationService : IGeoDbDestinationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public GeoDbDestinationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeoDB:ApiKey"];
            _baseUrl = configuration["GeoDB:BaseUrl"] ?? "https://wft-geo-db.p.rapidapi.com/v1";

            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", "wft-geo-db.p.rapidapi.com");
        }

        public async Task<List<CityDestination>> SearchCitiesAsync(string namePrefix, int limit = 20)
        {
            var cities = new List<CityDestination>();

            try
            {
                var url = $"{_baseUrl}/geo/cities?namePrefix={Uri.EscapeDataString(namePrefix)}&limit={limit}&sort=-population";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return GetMockCities(namePrefix);

                var json = await response.Content.ReadAsStringAsync();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                if (data?.data != null)
                {
                    foreach (var item in data.data)
                    {
                        cities.Add(new CityDestination
                        {
                            Id = item.id != null ? int.Parse(item.id.ToString()) : 0,
                            Name = item.city?.ToString() ?? item.name?.ToString() ?? "Unknown",
                            Country = item.country?.ToString() ?? "Pakistan",
                            CountryCode = item.countryCode?.ToString() ?? "PK",
                            Region = item.region?.ToString(),
                            Latitude = item.latitude != null ? double.Parse(item.latitude.ToString()) : null,
                            Longitude = item.longitude != null ? double.Parse(item.longitude.ToString()) : null,
                            Population = item.population != null ? int.Parse(item.population.ToString()) : null,
                            Timezone = item.timezone?.ToString(),
                            WikiDataId = item.wikiDataId?.ToString()
                        });
                    }
                }

                if (cities.Count == 0)
                    return GetMockCities(namePrefix);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GeoDB API Error: {ex.Message}");
                return GetMockCities(namePrefix);
            }

            return cities;
        }

        public async Task<CityDestination> GetCityDetailsAsync(int cityId)
        {
            try
            {
                var url = $"{_baseUrl}/geo/cities/{cityId}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                if (data?.data != null)
                {
                    return new CityDestination
                    {
                        Id = data.data.id != null ? int.Parse(data.data.id.ToString()) : 0,
                        Name = data.data.city?.ToString() ?? data.data.name?.ToString(),
                        Country = data.data.country?.ToString(),
                        CountryCode = data.data.countryCode?.ToString(),
                        Region = data.data.region?.ToString(),
                        Latitude = data.data.latitude != null ? double.Parse(data.data.latitude.ToString()) : null,
                        Longitude = data.data.longitude != null ? double.Parse(data.data.longitude.ToString()) : null,
                        Population = data.data.population != null ? int.Parse(data.data.population.ToString()) : null,
                        Timezone = data.data.timezone?.ToString(),
                        WikiDataId = data.data.wikiDataId?.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GeoDB API Error: {ex.Message}");
            }

            return null;
        }

        public async Task<List<CityDestination>> GetNearbyCitiesAsync(double lat, double lng, int radiusKm = 50)
        {
            var cities = new List<CityDestination>();

            try
            {
                var url = $"{_baseUrl}/geo/cities?location={lat}%2B{lng}&radius={radiusKm}&limit=20&sort=-population";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return cities;

                var json = await response.Content.ReadAsStringAsync();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                if (data?.data != null)
                {
                    foreach (var item in data.data)
                    {
                        cities.Add(new CityDestination
                        {
                            Id = item.id != null ? int.Parse(item.id.ToString()) : 0,
                            Name = item.city?.ToString() ?? item.name?.ToString(),
                            Country = item.country?.ToString(),
                            CountryCode = item.countryCode?.ToString(),
                            Latitude = item.latitude != null ? double.Parse(item.latitude.ToString()) : null,
                            Longitude = item.longitude != null ? double.Parse(item.longitude.ToString()) : null,
                            Population = item.population != null ? int.Parse(item.population.ToString()) : null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GeoDB API Error: {ex.Message}");
            }

            return cities;
        }

        public async Task<List<CityDestination>> GetPopularDestinationsAsync(string countryCode = "PK", int limit = 10)
        {
            var cities = new List<CityDestination>();

            try
            {
                var url = $"{_baseUrl}/geo/cities?countryIds={countryCode}&limit={limit}&sort=-population";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return GetMockCities(countryCode);

                var json = await response.Content.ReadAsStringAsync();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                if (data?.data != null)
                {
                    foreach (var item in data.data)
                    {
                        cities.Add(new CityDestination
                        {
                            Id = item.id != null ? int.Parse(item.id.ToString()) : 0,
                            Name = item.city?.ToString() ?? item.name?.ToString(),
                            Country = item.country?.ToString(),
                            CountryCode = item.countryCode?.ToString(),
                            Latitude = item.latitude != null ? double.Parse(item.latitude.ToString()) : null,
                            Longitude = item.longitude != null ? double.Parse(item.longitude.ToString()) : null,
                            Population = item.population != null ? int.Parse(item.population.ToString()) : null
                        });
                    }
                }

                if (cities.Count == 0)
                    return GetMockCities(countryCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GeoDB API Error: {ex.Message}");
                return GetMockCities(countryCode);
            }

            return cities;
        }

        private List<CityDestination> GetMockCities(string query)
        {
            var pakistaniCities = new List<CityDestination>
            {
                new CityDestination { Id = 1, Name = "Islamabad", Country = "Pakistan", CountryCode = "PK", Population = 1016825, Latitude = 33.6844, Longitude = 73.0479 },
                new CityDestination { Id = 2, Name = "Lahore", Country = "Pakistan", CountryCode = "PK", Population = 11100000, Latitude = 31.5204, Longitude = 74.3587 },
                new CityDestination { Id = 3, Name = "Karachi", Country = "Pakistan", CountryCode = "PK", Population = 14910000, Latitude = 24.8607, Longitude = 67.0011 },
                new CityDestination { Id = 4, Name = "Murree", Country = "Pakistan", CountryCode = "PK", Population = 25000, Latitude = 33.9062, Longitude = 73.3903 },
                new CityDestination { Id = 5, Name = "Hunza", Country = "Pakistan", CountryCode = "PK", Population = 50000, Latitude = 36.3167, Longitude = 74.6500 },
                new CityDestination { Id = 6, Name = "Skardu", Country = "Pakistan", CountryCode = "PK", Population = 26500, Latitude = 35.2971, Longitude = 75.6333 }
            };

            if (!string.IsNullOrEmpty(query))
            {
                return pakistaniCities.Where(c =>
                    c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return pakistaniCities;
        }
    }
}