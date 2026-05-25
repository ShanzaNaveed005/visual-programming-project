using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AITourismPlanner.Models
{
    public class User
    {
        [Key]
        public int user_id { get; set; }

        [Required]
        [StringLength(100)]
        public string full_name { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [StringLength(255)]
        public string password_hash { get; set; }

        [StringLength(20)]
        public string phone { get; set; }

        public string gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? date_of_birth { get; set; }

        [StringLength(255)]
        public string profile_image { get; set; }

        public int? role_id { get; set; }

        public string status { get; set; } = "Active";

        public DateTime created_at { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("role_id")]
        public virtual Role Role { get; set; }

        public virtual UserPreference UserPreference { get; set; }
        public virtual ICollection<HotelBooking> HotelBookings { get; set; }
        public virtual ICollection<TransportBooking> TransportBookings { get; set; }
        public virtual ICollection<Trip> Trips { get; set; }
        public virtual ICollection<AIRecommendation> AIRecommendations { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<ChatbotHistory> ChatbotHistories { get; set; }
        public virtual ICollection<AdminLog> AdminLogs { get; set; }
    }
}