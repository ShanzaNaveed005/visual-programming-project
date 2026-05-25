using System.ComponentModel.DataAnnotations;

namespace AITourismPlanner.Models
{
    public class Category
    {
        [Key]
        public int category_id { get; set; }

        [Required]
        [StringLength(100)]
        public string category_name { get; set; }

        // Navigation property
        public virtual ICollection<Destination> Destinations { get; set; }
    }
}