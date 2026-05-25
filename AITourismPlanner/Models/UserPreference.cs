using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AITourismPlanner.Models
{
    public class UserPreference
    {
        [Key]
        public int preference_id { get; set; }

        public int? user_id { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? preferred_budget { get; set; }

        [StringLength(100)]
        public string favorite_category { get; set; }

        [StringLength(100)]

        public string preferred_transport { get; set; }

        [StringLength(50)]
        public string preferred_season { get; set; }

        // Navigation property
        [ForeignKey("user_id")]
        public virtual User User { get; set; }
    }
}