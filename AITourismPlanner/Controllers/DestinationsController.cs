using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AITourismPlanner.Data;
using AITourismPlanner.Models;
using AITourismPlanner.Services;
using AITourismPlanner.ViewModels;
using Newtonsoft.Json;

namespace AITourismPlanner.Controllers
{
    public class DestinationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDestinationApiService _destinationApi;
        private readonly IWeatherService _weatherService;
        private readonly IImageService _imageService;
        public DestinationsController(
            ApplicationDbContext context,
            IDestinationApiService destinationApi,
            IWeatherService weatherService,
            IImageService imageService)
        {
            _context = context;
            _destinationApi = destinationApi;
            _weatherService = weatherService;
             _imageService = imageService;
        }

        // =========================================================
        // INDEX — API se destinations
        // =========================================================
        public async Task<IActionResult> Index(string searchTerm = "Pakistan")
        {
            ViewBag.SearchTerm = searchTerm;

            var popularCities = new List<string>
            {
                "Hunza", "Murree", "Skardu", "Lahore",
                "Islamabad", "Naran", "Swat", "Quetta"
            };

            if (!string.IsNullOrEmpty(searchTerm) && searchTerm != "Pakistan")
            {
                popularCities = new List<string> { searchTerm };
            }

            var destinations = new List<DestinationInfo>();

            foreach (var city in popularCities)
            {
                var info = await _destinationApi.GetDestinationInfoAsync(city);
                if (info != null)
                    destinations.Add(info);
            }

            return View(destinations);
        }

        // =========================================================
        // API DETAILS — Sab API se
        // =========================================================
        public async Task<IActionResult> ApiDetails(string cityName)
        {
            if (string.IsNullOrEmpty(cityName))
                return RedirectToAction("Index");

            var checkIn = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            var checkOut = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd");

            var destTask = _destinationApi.GetDestinationInfoAsync(cityName);
            var weatherTask = _weatherService.GetCurrentWeatherAsync(cityName);
            var forecastTask = _weatherService.GetWeatherForecastAsync(cityName, 5);

            await Task.WhenAll(destTask, weatherTask, forecastTask);

            var hotels = await GetHotelsDirectAsync(cityName, checkIn, checkOut);

            if (destTask.Result == null)
                return RedirectToAction("Index");

            var viewModel = new ApiDestinationViewModel
            {
                CityName = cityName,
                DestinationInfo = destTask.Result,
                CurrentWeather = weatherTask.Result,
                Forecast = forecastTask.Result,
                NearbyHotels = hotels
            };

            return View(viewModel);
        }

        // =========================================================
        // HOTEL FETCH — Direct
        // =========================================================
        private async Task<List<RealHotel>> GetHotelsDirectAsync(
            string city, string checkIn, string checkOut)
        {
            var hotels = new List<RealHotel>();
            var apiKey = "";
            var host = "booking-com15.p.rapidapi.com";

            try
            {
                var client = new HttpClient();

                var destRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(
                        $"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchDestination?query={city}"),
                    Headers =
                    {
                        { "X-RapidAPI-Key", apiKey },
                        { "X-RapidAPI-Host", host },
                    },
                };

                var destResponse = await client.SendAsync(destRequest);
                var destJson = await destResponse.Content.ReadAsStringAsync();
                dynamic destData = JsonConvert.DeserializeObject(destJson);

                string destId = null;
                if (destData?.data != null)
                {
                    foreach (var item in destData.data)
                    {
                        if (item?.dest_type?.ToString() == "city")
                        {
                            destId = item?.dest_id?.ToString();
                            break;
                        }
                    }
                }

                if (destId == null) return hotels;

                var hotelRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(
                        $"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchHotels" +
                        $"?dest_id={destId}" +
                        $"&search_type=city" +
                        $"&arrival_date={checkIn}" +
                        $"&departure_date={checkOut}" +
                        $"&adults=1" +
                        $"&room_qty=1" +
                        $"&page_number=1" +
                        $"&languagecode=en-us" +
                        $"&currency_code=USD"),
                    Headers =
                    {
                        { "X-RapidAPI-Key", apiKey },
                        { "X-RapidAPI-Host", host },
                    },
                };

                var hotelResponse = await client.SendAsync(hotelRequest);
                var hotelJson = await hotelResponse.Content.ReadAsStringAsync();
                dynamic hotelData = JsonConvert.DeserializeObject(hotelJson);

                if (hotelData?.data?.hotels != null)
                {
                    foreach (var item in hotelData.data.hotels)
                    {
                        try
                        {
                            decimal? price = null;
                            try
                            {
                                if (item?.property?.priceBreakdown?.grossPrice?.value != null)
                                    price = decimal.Parse(
                                        item.property.priceBreakdown.grossPrice.value.ToString()) * 278;
                            }
                            catch { }

                            string imageUrl = null;
                            try
                            {
                                if (item?.property?.photoUrls != null)
                                    imageUrl = item.property.photoUrls[0]?.ToString();
                            }
                            catch { }

                            decimal? rating = null;
                            try
                            {
                                if (item?.property?.reviewScore != null)
                                    rating = decimal.Parse(
                                        item.property.reviewScore.ToString());
                            }
                            catch { }

                            int? reviewCount = null;
                            try
                            {
                                if (item?.property?.reviewCount != null)
                                    reviewCount = int.Parse(
                                        item.property.reviewCount.ToString());
                            }
                            catch { }

                            string name = item?.property?.name?.ToString();

                            if (!string.IsNullOrEmpty(name))
                            {
                                hotels.Add(new RealHotel
                                {
                                    Id = item?.property?.id?.ToString(),
                                    Name = name,
                                    Address = item?.property?.wishlistName?.ToString() ?? city,
                                    Rating = rating,
                                    PricePerNight = price,
                                    Currency = "PKR",
                                    ImageUrl = imageUrl,
                                    ReviewCount = reviewCount
                                });
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hotels Error: {ex.Message}");
            }

            return hotels;
        }
        public async Task<IActionResult> DebugDestHotels2()
        {
            var client = new HttpClient();
            var apiKey = "";
            var host = "booking-com15.p.rapidapi.com";

            // Step 1 - dest_id check karo
            var destRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    "https://booking-com15.p.rapidapi.com/api/v1/hotels/searchDestination?query=Islamabad"),
                Headers =
        {
            { "X-RapidAPI-Key", apiKey },
            { "X-RapidAPI-Host", host },
        },
            };

            var destResponse = await client.SendAsync(destRequest);
            var destJson = await destResponse.Content.ReadAsStringAsync();

            dynamic destData = Newtonsoft.Json.JsonConvert.DeserializeObject(destJson);

            string destId = null;
            if (destData?.data != null)
            {
                foreach (var item in destData.data)
                {
                    if (item?.dest_type?.ToString() == "city")
                    {
                        destId = item?.dest_id?.ToString();
                        break;
                    }
                }
            }

            if (destId == null)
                return Content($"DestId NULL. Raw JSON: {destJson}", "text/plain");

            // Step 2 - Hotels check karo
            var hotelRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    $"https://booking-com15.p.rapidapi.com/api/v1/hotels/searchHotels" +
                    $"?dest_id={destId}" +
                    $"&search_type=city" +
                    $"&arrival_date=2026-06-10" +
                    $"&departure_date=2026-06-12" +
                    $"&adults=1" +
                    $"&room_qty=1" +
                    $"&page_number=1" +
                    $"&languagecode=en-us" +
                    $"&currency_code=USD"),
                Headers =
        {
            { "X-RapidAPI-Key", apiKey },
            { "X-RapidAPI-Host", host },
        },
            };

            var hotelResponse = await client.SendAsync(hotelRequest);
            var hotelJson = await hotelResponse.Content.ReadAsStringAsync();

            dynamic hotelData = Newtonsoft.Json.JsonConvert.DeserializeObject(hotelJson);

            int count = 0;
            string firstName = "none";
            string dataNull = hotelData?.data == null ? "NULL" : "NOT NULL";
            string hotelsNull = hotelData?.data?.hotels == null ? "NULL" : "NOT NULL";

            try
            {
                if (hotelData?.data?.hotels != null)
                {
                    foreach (var h in hotelData.data.hotels)
                    {
                        count++;
                        if (count == 1)
                            firstName = h?.property?.name?.ToString() ?? "name null";
                    }
                }
            }
            catch (Exception ex)
            {
                firstName = "Error: " + ex.Message;
            }

            return Content(
                $"DestId: {destId}\n" +
                $"Data null: {dataNull}\n" +
                $"Hotels null: {hotelsNull}\n" +
                $"Count: {count}\n" +
                $"First: {firstName}\n" +
                $"Raw hotel response (first 500 chars): {hotelJson.Substring(0, Math.Min(500, hotelJson.Length))}",
                "text/plain");
        }
        // =========================================================
        // ADD REVIEW
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(
            int destinationId, int rating, string comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Json(new { success = false, message = "Please login first" });

            var review = new Review
            {
                destination_id = destinationId,
                user_id = userId.Value,
                rating = rating,
                review_text = comment,
                review_date = DateTime.Now
            };

            _context.reviews.Add(review);
            await _context.SaveChangesAsync();

            var avg = await _context.reviews
                .Where(r => r.destination_id == destinationId)
                .AverageAsync(r => r.rating ?? 0);

            var dest = await _context.destinations.FindAsync(destinationId);
            if (dest != null)
            {
                dest.rating_average = (decimal)avg;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Review added!" });
        }
    }
}