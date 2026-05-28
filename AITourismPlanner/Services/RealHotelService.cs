using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AITourismPlanner.Services
{
    public interface IRealHotelService
    {
        Task<List<RealHotel>> SearchHotelsAsync(string city, string checkIn, string checkOut, int guests = 1);
        Task<RealHotelDetails> GetHotelDetailsAsync(string hotelId);
    }

    public class RealHotel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal? Rating { get; set; }
        public decimal? PricePerNight { get; set; }
        public string Currency { get; set; }
        public string ImageUrl { get; set; }
        public int? ReviewCount { get; set; }
        public string Description { get; set; }
        public List<string> Amenities { get; set; }
        public List<string> Images { get; set; }
    }

    public class RealHotelDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public decimal? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public List<string> Images { get; set; }
        public List<string> Amenities { get; set; }
        public List<Review> Reviews { get; set; }
    }

    public class Review
    {
        public string UserName { get; set; }
        public int? Rating { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Date { get; set; }
    }

    public class RealHotelService : IRealHotelService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _host;

        public RealHotelService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["RapidAPI:Key"];
            _host = configuration["RapidAPI:BookingHost"] ?? "booking-com.p.rapidapi.com";
        }

        public async Task<List<RealHotel>> SearchHotelsAsync(string city, string checkIn, string checkOut, int guests = 1)
        {
            var hotels = new List<RealHotel>();

            try
            {
                var url = $"https://booking-com.p.rapidapi.com/v1/hotels/search?locale=en-gb&units=metric&dest_type=city&dest_id=-{GetCityCode(city)}&checkin_date={checkIn}&checkout_date={checkOut}&adults_number={guests}&room_number=1&order_by=popularity&filter_by_currency=PKR&page_number=0";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url),
                    Headers = {
                        { "X-RapidAPI-Key", _apiKey },
                        { "X-RapidAPI-Host", _host },
                    },
                };

                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                dynamic data = JsonConvert.DeserializeObject(json);

                if (data?.result != null)
                {
                    foreach (var item in data.result)
                    {
                        hotels.Add(new RealHotel
                        {
                            Id = item.hotel_id.ToString(),
                            Name = item.hotel_name.ToString(),
                            Address = item.address?.ToString() ?? city,
                            Rating = item.review_score != null ? (decimal?)decimal.Parse(item.review_score.ToString()) : null,
                            PricePerNight = item.min_total_price != null ? (decimal?)decimal.Parse(item.min_total_price.ToString()) / 3 : null,
                            Currency = "PKR",
                            ImageUrl = item.main_photo_url?.ToString(),
                            ReviewCount = item.review_count != null ? (int?)int.Parse(item.review_count.ToString()) : null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
            }

            return hotels;
        }

        public async Task<RealHotelDetails> GetHotelDetailsAsync(string hotelId)
        {
            try
            {
                var url = $"https://booking-com.p.rapidapi.com/v1/hotels/data?hotel_id={hotelId}&locale=en-gb";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url),
                    Headers = {
                        { "X-RapidAPI-Key", _apiKey },
                        { "X-RapidAPI-Host", _host },
                    },
                };

                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(json);

                var details = new RealHotelDetails
                {
                    Id = hotelId,
                    Name = data?.hotel_name,
                    Description = data?.description,
                    Address = data?.address,
                    Rating = data?.review_score != null ? (decimal?)decimal.Parse(data.review_score.ToString()) : null,
                    ReviewCount = data?.review_count,
                    Images = new List<string>(),
                    Amenities = new List<string>(),
                    Reviews = new List<Review>()
                };

                if (data?.images != null)
                {
                    foreach (var img in data.images)
                    {
                        details.Images.Add(img?.url?.ToString());
                    }
                }

                if (data?.amenities != null)
                {
                    foreach (var amenity in data.amenities)
                    {
                        details.Amenities.Add(amenity?.name?.ToString());
                    }
                }

                return details;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                return null;
            }
        }

        private string GetCityCode(string city)
        {
            var cities = new Dictionary<string, string>
            {
                { "islamabad", "221847" },
                { "lahore", "221884" },
                { "karachi", "221876" },
                { "murree", "20033875" },
                { "hunza", "900055149" },
                { "skardu", "900055151" },
                { "multan", "221893" }
            };

            var key = city?.ToLower() ?? "";
            return cities.ContainsKey(key) ? cities[key] : "221847";
        }
    }
}