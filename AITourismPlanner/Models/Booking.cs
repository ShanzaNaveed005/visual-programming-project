using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AITourismPlanner.Models
{
    [Table("bookings")]
    public class Booking
    {
        [Key]
        public int booking_id { get; set; }

        [StringLength(50)]
        public string booking_reference { get; set; } = string.Empty;

        public int user_id { get; set; }

        [StringLength(200)]
        public string destination_name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,8)")]
        public decimal? destination_lat { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public decimal? destination_lng { get; set; }

        [StringLength(100)]
        public string hotel_id { get; set; } = string.Empty;

        [StringLength(200)]
        public string hotel_name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? hotel_price_per_night { get; set; }

        public DateTime check_in_date { get; set; }
        public DateTime check_out_date { get; set; }
        public int number_of_guests { get; set; } = 1;
        public int nights { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? total_hotel_cost { get; set; }

        [StringLength(50)]
        public string transport_type { get; set; } = string.Empty;

        [StringLength(100)]
        public string transport_company { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? transport_fare { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? total_transport_cost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal total_price { get; set; }

        [StringLength(50)]
        public string booking_status { get; set; } = "Pending";

        [StringLength(50)]
        public string payment_status { get; set; } = "Pending";

        [StringLength(200)]
        public string passenger_name { get; set; } = string.Empty;

        [StringLength(200)]
        public string passenger_email { get; set; } = string.Empty;

        [StringLength(20)]
        public string passenger_phone { get; set; } = string.Empty;

        public string special_requests { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;

        [ForeignKey("user_id")]
        public virtual User User { get; set; }
    }
}