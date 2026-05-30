using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AITourismPlanner.Models
{
    [Table("destination_reviews")]
    public class DestinationReview
    {
        [Key]
        public int review_id { get; set; }
        public int user_id { get; set; }
        public string destination_name { get; set; } = string.Empty;
        public int rating { get; set; }
        public string comment { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }

        [ForeignKey("user_id")]
        public virtual User User { get; set; }
    }
}