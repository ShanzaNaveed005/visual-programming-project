namespace AITourismPlanner.Services
{
    public class TransportApiService : ITransportApiService
    {
        public async Task<List<TransportOption>> GetTransportOptionsAsync(string from, string to, DateTime date)
        {
            var options = new List<TransportOption>();
            var distance = GetDistance(from, to);

            // Bus options
            options.Add(new TransportOption
            {
                Type = "Bus",
                Company = "Daewoo Express",
                From = from,
                To = to,
                DepartureTime = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0),
                ArrivalTime = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0).AddHours(distance.Hours),
                Duration = $"{distance.Hours} hours",
                Price = CalculateFare(distance.Kilometers, "Bus"),
                AvailableSeats = 40,
                BookingLink = "https://www.daewoo.com.pk"
            });

            options.Add(new TransportOption
            {
                Type = "Bus",
                Company = "Faisal Movers",
                From = from,
                To = to,
                DepartureTime = new DateTime(date.Year, date.Month, date.Day, 10, 0, 0),
                ArrivalTime = new DateTime(date.Year, date.Month, date.Day, 10, 0, 0).AddHours(distance.Hours),
                Duration = $"{distance.Hours} hours",
                Price = CalculateFare(distance.Kilometers, "Bus") * 0.9m,
                AvailableSeats = 35,
                BookingLink = "https://www.faisalmovers.com.pk"
            });

            // Train option for longer distances
            if (distance.Kilometers > 200)
            {
                options.Add(new TransportOption
                {
                    Type = "Train",
                    Company = "Pakistan Railways",
                    From = from,
                    To = to,
                    DepartureTime = new DateTime(date.Year, date.Month, date.Day, 22, 0, 0),
                    ArrivalTime = new DateTime(date.Year, date.Month, date.Day, 22, 0, 0).AddHours(distance.Hours + 1),
                    Duration = $"{distance.Hours + 1} hours",
                    Price = CalculateFare(distance.Kilometers, "Train"),
                    AvailableSeats = 100,
                    BookingLink = "https://www.pakrail.gov.pk"
                });
            }

            // Flight option for long distances
            if (distance.Kilometers > 300)
            {
                options.Add(new TransportOption
                {
                    Type = "Flight",
                    Company = "PIA",
                    From = from,
                    To = to,
                    DepartureTime = new DateTime(date.Year, date.Month, date.Day, 9, 0, 0),
                    ArrivalTime = new DateTime(date.Year, date.Month, date.Day, 9, 0, 0).AddHours(1),
                    Duration = "1 hour",
                    Price = CalculateFare(distance.Kilometers, "Flight"),
                    AvailableSeats = 120,
                    BookingLink = "https://www.piac.com.pk"
                });
            }

            return options;
        }

        public async Task<TransportOption> GetCheapestOptionAsync(string from, string to, DateTime date)
        {
            var options = await GetTransportOptionsAsync(from, to, date);
            return options.OrderBy(o => o.Price).FirstOrDefault() ?? new TransportOption();
        }

        private (double Kilometers, double Hours) GetDistance(string from, string to)
        {
            var distances = new Dictionary<string, Dictionary<string, (double km, double hours)>>
            {
                { "Islamabad", new Dictionary<string, (double, double)>
                    {
                        { "Murree", (50, 1.5) },
                        { "Hunza", (500, 12) },
                        { "Skardu", (700, 16) },
                        { "Lahore", (380, 5) },
                        { "Swat", (250, 6) },
                        { "Naran", (200, 5) }
                    }
                },
                { "Lahore", new Dictionary<string, (double, double)>
                    {
                        { "Islamabad", (380, 5) },
                        { "Murree", (330, 4.5) },
                        { "Karachi", (1200, 15) }
                    }
                },
                { "Karachi", new Dictionary<string, (double, double)>
                    {
                        { "Lahore", (1200, 15) },
                        { "Islamabad", (1450, 18) }
                    }
                }
            };

            if (distances.ContainsKey(from) && distances[from].ContainsKey(to))
                return distances[from][to];

            return (200, 4);
        }

        private decimal CalculateFare(double kilometers, string type)
        {
            return type switch
            {
                "Bus" => (decimal)(kilometers * 12),
                "Train" => (decimal)(kilometers * 8),
                "Flight" => (decimal)(kilometers * 25),
                _ => (decimal)(kilometers * 15)
            };
        }
    }
}
