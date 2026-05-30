using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AITourismPlanner.Models
{
    [Table("destination_views")]
    public class DestinationView
    {
        [Key]
        public int view_id { get; set; }
        public string destination_name { get; set; } = string.Empty;
        public int? user_id { get; set; }
        public DateTime viewed_at { get; set; } = DateTime.Now;
    }
}