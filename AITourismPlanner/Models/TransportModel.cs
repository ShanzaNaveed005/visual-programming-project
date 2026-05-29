using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AITourismPlanner.Models
{
    [Table("transports")]
    public class Transport
    {
        [Key]
        public int transport_id { get; set; }

        [Required]
        [StringLength(50)]
        public string transport_type { get; set; }

        [Required]
        [StringLength(100)]
        public string company_name { get; set; }

        [Required]
        [StringLength(100)]
        public string departure_city { get; set; }

        [Required]
        [StringLength(100)]
        public string arrival_city { get; set; }

        [Required]
        public TimeSpan departure_time { get; set; }

        [Required]
        public TimeSpan arrival_time { get; set; }

        [StringLength(50)]
        public string duration { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal fare { get; set; }

        public int available_seats { get; set; }
        public int total_seats { get; set; }

        [StringLength(100)]
        public string operator_name { get; set; }

        public string amenities { get; set; }

        [StringLength(255)]
        public string image_url { get; set; }

        public bool is_active { get; set; } = true;

        public DateTime created_at { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<TransportBooking> TransportBookings { get; set; }
    }

    public class TransportBooking
    {
        [Key]
        public int booking_id { get; set; }

        public int user_id { get; set; }
        public int transport_id { get; set; }

        [StringLength(50)]
        public string booking_reference { get; set; }

        public int seats_booked { get; set; }

        [StringLength(255)]
        public string seat_numbers { get; set; }

        public DateTime journey_date { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal total_price { get; set; }

        [StringLength(100)]
        public string passenger_name { get; set; }

        [StringLength(20)]
        public string passenger_phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string passenger_email { get; set; }

        [StringLength(50)]
        public string status { get; set; } = "Pending";

        public DateTime booking_date { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string payment_status { get; set; } = "Pending";

        // Navigation properties
        [ForeignKey("user_id")]
        public virtual User User { get; set; }

        [ForeignKey("transport_id")]
        public virtual Transport Transport { get; set; }
    }
}