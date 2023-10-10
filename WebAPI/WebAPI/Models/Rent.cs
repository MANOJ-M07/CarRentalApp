using PROD.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROD.Models
{
    [Table("Rentals")] // Specifies the table name
    public class RentModel
    {
        [Key]
        public int RentID { get; set; }

        [Required]
        [ForeignKey("Customer")] // You might need a model called "CustomerModel" and refer here as "CustomerModel" if using Entity Framework
        public int CustomerID { get; set; }

        [Required]
        [ForeignKey("Car")] // Assuming your Car model is named CarModel and refer here as "CarModel" if using Entity Framework
        public int CarID { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime RentOrderDate { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime ReturnDate { get; set; }

        public int? OdoReading { get; set; } // Nullable since not marked as NOT NULL in DB schema

        public int? ReturnOdoReading { get; set; } // Nullable since not marked as NOT NULL in DB schema

        [Required]
        [StringLength(255)]
        [RegularExpression(@"^[A-Z]{2}\d{2}\s\d{4}\d{7}$", ErrorMessage = "License Number must be in the format: two uppercase letters for state, two digits for RTO code, four digits for the license year, and seven digits at the end.")]
        public string LicenseNumber { get; set; }

        // Navigation properties if using Entity Framework:
        public virtual CarModel Car { get; set; }
        public virtual CustomerModel Customer { get; set; } // Assuming there's a CustomerModel in your project
    }

    public class SearchDates
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}", ApplyFormatInEditMode = true)]
        public DateTime RentDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReturnDate { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        public TimeSpan RentTime { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        public TimeSpan ReturnTime { get; set; }

    }
}
