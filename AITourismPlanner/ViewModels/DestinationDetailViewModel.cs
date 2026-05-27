using AITourismPlanner.Models;

namespace AITourismPlanner.ViewModels
{
    public class DestinationDetailViewModel
    {
        public Destination Destination { get; set; }
        public Weather Weather { get; set; }
        public List<EmergencyService> EmergencyServices { get; set; }
        public List<Destination> SimilarDestinations { get; set; }
    }
}