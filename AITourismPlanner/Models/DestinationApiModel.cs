using System.Collections.Generic;

namespace AITourismPlanner.Models
{
    public class DestinationApiModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public double Rating { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> ThingsToDo { get; set; } = new();
        public string BestTimeToVisit { get; set; } = string.Empty;
        public string Weather { get; set; } = string.Empty;
    }
}