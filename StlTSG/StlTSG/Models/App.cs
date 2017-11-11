namespace StlTSG.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CDAR.App")]
    public partial class App
    {
        public int ID { get; set; }

        [StringLength(250)]
        public string Status { get; set; }

        [StringLength(1500)]
        public string DiagnosticsTitle { get; set; }

        [StringLength(1500)]
        public string AppointmentTitle { get; set; }
    }
}
