using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StlTSG.Models
{
    public class InputFormModel
    {
        public InputFormModel()
        {
            Clients = new List<Client>();
        }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DeviceBrand { get; set; }

        public string DeviceModel { get; set; }

        public string Issue { get; set; }

        public string Institution { get; set; }

        public string Password { get; set; }

        public List<Client> Clients { get; set; }
    }
}