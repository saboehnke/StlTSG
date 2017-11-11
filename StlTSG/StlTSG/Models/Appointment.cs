namespace StlTSG.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CDAR.Appointment")]
    public partial class Appointment
    {
        public int ID { get; set; }

        public int CustomerID { get; set; }

        [Required]
        [StringLength(100)]
        public string Brand { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; }

        public decimal? Amount { get; set; }

        public bool OwesPayment { get; set; }

        public DateTime DateOfRequest { get; set; }

        public string ChargeID { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
