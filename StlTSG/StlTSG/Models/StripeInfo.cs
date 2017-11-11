namespace StlTSG.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CDAR.StripeInfo")]
    public partial class StripeInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StripeInfo()
        {
            Clients = new HashSet<Client>();
        }

        public int ID { get; set; }

        [StringLength(255)]
        public string UserID { get; set; }

        [StringLength(255)]
        public string CustomerID { get; set; }

        [StringLength(255)]
        public string SubscriptionID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Client> Clients { get; set; }
    }
}
