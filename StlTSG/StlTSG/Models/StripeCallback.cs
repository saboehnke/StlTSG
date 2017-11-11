using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StlTSG.Models
{
    public class StripeCallback
    {
        public string Code { get; set; }

        public string Scope { get; set; }

        public string State { get; set; }
    }
}