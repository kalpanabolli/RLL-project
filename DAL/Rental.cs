//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Rental
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Rental()
        {
            this.Costs = new HashSet<Cost>();
        }
    
        public int RentID { get; set; }
        public int CustomerID { get; set; }
        public int CarID { get; set; }
        public System.DateTime RentOrderDate { get; set; }
        public System.DateTime ReturnDate { get; set; }
        public Nullable<int> OdoReading { get; set; }
        public Nullable<int> ReturnOdoReading { get; set; }
        public string LicenseNumber { get; set; }
    
        public virtual Car Car { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cost> Costs { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
