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
        [ForeignKey("Customer")] 
        public int CustomerID { get; set; }

        [Required]
        [ForeignKey("Car")] 
        public int CarID { get; set; }

        [Required]
        public DateTime RentOrderDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        public int? OdoReading { get; set; } 

        public int? ReturnOdoReading { get; set; } 

        [Required]
        [StringLength(255)]
        public string LicenseNumber { get; set; }

    }

    public class SearchDates
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}", ApplyFormatInEditMode = true)]
        public DateTime RentDate { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReturnDate { get;set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        public TimeSpan RentTime { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        public TimeSpan ReturnTime { get; set; }

    }
}
