namespace AITourismPlanner.Services
{
    public interface IWeatherApiService
    {
        Task<WeatherData> GetCurrentWeatherAsync(string city);
        Task<WeatherForecastData> GetWeatherForecastAsync(string city, int days = 5);
    }

    public class WeatherData
    {
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public int Pressure { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class WeatherForecastData
    {
        public string City { get; set; } = string.Empty;
        public List<DailyForecast> DailyForecasts { get; set; } = new();
    }

    public class DailyForecast
    {
        public DateTime Date { get; set; }
        public double TempMax { get; set; }
        public double TempMin { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
    }
}