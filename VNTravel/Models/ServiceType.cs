//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VNTravel.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ServiceType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ServiceType()
        {
            this.TravelServices = new HashSet<TravelService>();
        }
    
        public int ID_service_type { get; set; }
        public Nullable<int> ID_price { get; set; }
        public string Service_type_name { get; set; }
    
        public virtual Price Price { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TravelService> TravelServices { get; set; }
    }
}