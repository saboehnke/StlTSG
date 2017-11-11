using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StlTSG.Models
{
    public class AccountModel
    {
        public byte[] ProfileImage { get; set; }

        public bool Subscribed { get; set; }

        public bool IsEmailVerified { get; set; }

        public string Email { get; set; }

        public string UserID { get; set; }
    }
}