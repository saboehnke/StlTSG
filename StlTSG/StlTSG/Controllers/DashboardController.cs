using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using StlTSG.Models;
using System.Configuration;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace StlTSG.Controllers
{
    public class DashboardController : Controller
    {
        private CDARModel db = new CDARModel();
        private UserStore<ApplicationUser> store = null;

        public ActionResult Dashboard()
        {
            List<DailyStat> dailyStats = new List<DailyStat>();
            List<MonthlyStat> monthlyStats = new List<MonthlyStat>();
            List<CustomerData> customersData = new List<CustomerData>();
            List<Appointment> appointments = new List<Appointment>();
            Dictionary<DateTime, int> ds = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> ms = new Dictionary<DateTime, int>();
            List<Appointment> allAppointments = db.Appointments.Where(a => a.OwesPayment == true).OrderBy(a => a.DateOfRequest).ToList();
            Client currentClient = db.Clients.FirstOrDefault(c => c.Email == User.Identity.Name);

            if (currentClient == null)
                return View("Error");

            if (store == null)
                store = new UserStore<ApplicationUser>(new ApplicationDbContext());

            var userManager = new UserManager<ApplicationUser>(store);
            ApplicationUser user = userManager.FindByNameAsync(User.Identity.Name).Result;

            if (currentClient.IsActive && user.EmailConfirmed)
            {
                foreach (Appointment app in allAppointments)
                {
                    Customer customer = db.Customers.FirstOrDefault(c => c.ID == app.CustomerID);
                    if (customer != null && customer.ClientID == currentClient.ID)
                        appointments.Add(app);
                }

                foreach (Appointment app in appointments)
                {
                    if (!ds.ContainsKey(app.DateOfRequest.Date))
                        ds.Add(app.DateOfRequest.Date, 1);
                    else ds[app.DateOfRequest.Date]++;

                    DateTime msFormat = Convert.ToDateTime(string.Format(@"{0}/1/{1}", app.DateOfRequest.Month, app.DateOfRequest.Year));
                    if (!ms.ContainsKey(msFormat))
                        ms.Add(msFormat, 1);
                    else ms[msFormat]++;
                }

                foreach (var stat in ds)
                {
                    DailyStat dailyStat = new DailyStat()
                    {
                        NumberOfUsers = stat.Value,
                        Date = stat.Key
                    };
                    dailyStats.Add(dailyStat);
                }

                foreach (var stat in ms)
                {
                    MonthlyStat monthlyStat = new MonthlyStat()
                    {
                        NumberOfUsers = stat.Value,
                        Date = stat.Key
                    };
                    monthlyStats.Add(monthlyStat);
                }

                List<Customer> customers = db.Customers.Where(c => c.ClientID == currentClient.ID).OrderBy(c => c.ID).ToList();

                foreach (Customer customer in customers)
                {
                    decimal amountOwed = db.Appointments.Where(a => a.CustomerID == customer.ID).Sum(c => c.Amount) ?? 0;

                    CustomerData customerData = new CustomerData()
                    {
                        CustomerID = customer.ID,
                        Name = string.Format("{0} {1}", customer.FirstName, customer.LastName),
                        Email = customer.Email,
                        AmountOwed = amountOwed
                    };
                    customersData.Add(customerData);
                }
            }
            else TempData["AccountStatusError"] = ConfigurationManager.AppSettings["NotValidatedOrSubscribed"];

            DashboardModel model = new DashboardModel()
            {
                DailyStats = dailyStats,
                MonthlyStats = monthlyStats,
                CustomersData = customersData,
                ProfileImage = currentClient.ProfileImage
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult SaveOwedAmount(CustomerUpdate customerUpdate)
        {
            Appointment appointment = db.Appointments.FirstOrDefault(a => a.CustomerID == customerUpdate.ID);

            if (appointment != null)
            {
                appointment.Amount = customerUpdate.Amount;
                db.Appointments.Attach(appointment);
                var entry = db.Entry(appointment);
                entry.Property(e => e.Amount).IsModified = true;
                db.SaveChanges();

                return new EmptyResult();
            }
            else
            {
                ErrorHandler.PostError(db, ConfigurationManager.AppSettings["SaveError"]);
                return View("Error");
            }
        }
    }
}