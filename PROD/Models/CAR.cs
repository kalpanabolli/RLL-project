using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROD.Models 
{
    [Table("Cars")] // Specifies the table name
    public class CarModel
    {
        [Key]
        public int CarID { get; set; }

        [Required]
        [StringLength(255)]
        public string CarName { get; set; }

        [Required]
        public bool Available { get; set; } 

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PerDayCharge { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ChargePerKm { get; set; }

        [Required]
        [StringLength(255)]
        public string CarType { get; set; }

        [StringLength(255)]
        public string Photo { get; set; }

    }
}
