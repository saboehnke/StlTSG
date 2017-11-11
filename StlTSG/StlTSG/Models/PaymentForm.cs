using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StlTSG.Models
{
    public class PaymentForm
    {
        public decimal? Amount { get; set; }

        public string CardNumber { get; set; }

        public string Expiration { get; set; }

        public string Email { get; set; }
    }
}