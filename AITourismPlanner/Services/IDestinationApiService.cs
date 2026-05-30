namespace AITourismPlanner.Services
{
    public interface IDestinationApiService
    {
        Task<List<DestinationApiModel>> GetPopularDestinationsAsync(int limit = 12);
        Task<List<DestinationApiModel>> SearchDestinationsAsync(string query, int limit = 20);
        Task<DestinationDetailModel> GetDestinationDetailAsync(string name);
    }

    public class DestinationApiModel
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal? EstimatedCost { get; set; }
        public double? Rating { get; set; }
    }

    public class DestinationDetailModel : DestinationApiModel
    {
        public List<string> ThingsToDo { get; set; } = new();
        public string BestTimeToVisit { get; set; } = string.Empty;
        public string WeatherInfo { get; set; } = string.Empty;
    }
}