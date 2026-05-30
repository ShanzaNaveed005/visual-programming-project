using AITourismPlanner.Services;

namespace AITourismPlanner.ViewModels
{
    public class ApiDestinationViewModel
    {
        public string CityName { get; set; }
        public DestinationInfo DestinationInfo { get; set; }
        public WeatherData CurrentWeather { get; set; }
        public WeatherForecast Forecast { get; set; }
        public List<RealHotel> NearbyHotels { get; set; } = new();
    }
}