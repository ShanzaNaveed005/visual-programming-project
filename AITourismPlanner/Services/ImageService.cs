namespace AITourismPlanner.Services
{
    public interface IImageService
    {
        string GetCityImageUrl(string cityName, int width = 800, int height = 600);
        string GetRandomImageUrl(string query, int width = 800, int height = 600);
    }

    public class ImageService : IImageService
    {
        // No API key needed! Direct Unsplash Source API
        private const string UnsplashBaseUrl = "https://source.unsplash.com";

        public string GetCityImageUrl(string cityName, int width = 800, int height = 600)
        {
            if (string.IsNullOrEmpty(cityName))
                return $"{UnsplashBaseUrl}/featured/{width}x{height}/?travel";

            // URL encode the city name for safe query parameter
            var encodedCity = Uri.EscapeDataString(cityName);

            // Multiple keywords increase chance of relevant images
            return $"{UnsplashBaseUrl}/featured/{width}x{height}/?{encodedCity},travel,landmark";
        }

        public string GetRandomImageUrl(string query, int width = 800, int height = 600)
        {
            if (string.IsNullOrEmpty(query))
                return $"{UnsplashBaseUrl}/random/{width}x{height}";

            var encodedQuery = Uri.EscapeDataString(query);
            return $"{UnsplashBaseUrl}/random/{width}x{height}/?{encodedQuery}";
        }
    }
}