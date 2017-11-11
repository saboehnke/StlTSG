namespace StlTSG.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CDAR.Error")]
    public partial class Error
    {
        public int ID { get; set; }

        [Required]
        [StringLength(1500)]
        public string Value { get; set; }

        public DateTime Date { get; set; }
    }
}
