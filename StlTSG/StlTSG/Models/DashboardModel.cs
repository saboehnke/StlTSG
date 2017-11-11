using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StlTSG.Models
{
    public class DashboardModel
    {
        public DashboardModel()
        {
            DailyStats = new List<DailyStat>();
            CustomersData = new List<CustomerData>();
        }

        public string DateRange { get; set; }

        public string Year { get; set; }

        public List<DailyStat> DailyStats { get; set; }

        public List<MonthlyStat> MonthlyStats { get; set; }

        public List<CustomerData> CustomersData { get; set; }

        public byte[] ProfileImage { get; set; }
    }
}