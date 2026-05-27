using AITourismPlanner.Models;

namespace AITourismPlanner.ViewModels
{
    public class TripPlannerViewModel
    {
        public List<Destination>? Recommendations { get; set; }
        public object? Destinations { get; set; }
        public int? SelectedDestinationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Budget { get; set; }
        public int Travelers { get; set; } = 1;
        public string? Interests { get; set; }
    }
}