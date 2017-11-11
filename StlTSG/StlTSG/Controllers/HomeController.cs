using System;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using StlTSG.Models;
using System.Net;
using System.IO;
using Stripe;
using System.Collections.Generic;

namespace StlTSG.Controllers
{
    public class HomeController : Controller
    {
        private CDARModel db = new CDARModel();

        public ActionResult Index()
        {
            InputFormModel ifm = new InputFormModel()
            {
                Clients = db.Clients.Where(c => c.IsActive == true).ToList()
            };
            return View(ifm);
        }

        public ActionResult InstitutionalServices()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ContactUs(InputFormModel ifm)
        {
            SendContactEmail(ifm);
            return View("RequestSuccess");
        }

        [HttpPost]
        public ActionResult DiagnosticRequest(InputFormModel ifm)
        {
            SendEmail(ifm, "CDAR Diagnostic Request");
            return View("RequestSuccess");
        }

        [HttpPost]
        public ActionResult AppointmentRequest(InputFormModel ifm)
        {
            try
            {
                Client client = db.Clients.FirstOrDefault(c => c.Name == ifm.Institution);

                if (client != null)
                {
                    Customer customer = new Customer()
                    {
                        FirstName = ifm.FirstName,
                        LastName = ifm.LastName,
                        Email = ifm.Email,
                        ClientID = client.ID
                    };

                    if (db.Customers.FirstOrDefault(c => c.ClientID == customer.ClientID && c.Email == customer.Email
                        && c.FirstName == customer.FirstName && c.LastName == c.LastName) != null)
                        CreateAppointment(customer, ifm);
                    else
                    {
                        db.Customers.Add(customer);
                        db.SaveChanges();
                        CreateAppointment(customer, ifm);
                    }
                }
                else
                {
                    ErrorHandler.PostError(db, ConfigurationManager.AppSettings["InvalidInstitution"]);
                    CustomError customError = new CustomError()
                    {
                        Message = ConfigurationManager.AppSettings["InvalidInstitution"]
                    };
                    return View("CustomError", customError);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.PostError(db, ex.ToString());
                return View("Error");
            }
            return View("RequestSuccess");
        }

        [HttpPost]
        public ActionResult Payment(InputFormModel ifm)
        {
            Customer customer = db.Customers.FirstOrDefault(c => c.Email == ifm.Email);

            if (customer != null)
            {
                List<Appointment> appointments = db.Appointments.Where(a => a.CustomerID == customer.ID && a.OwesPayment == true).ToList();
                decimal amountOwed = appointments.Sum(a => a.Amount) ?? 0;

                if (amountOwed > 0)
                {
                    PaymentForm paymentForm = new PaymentForm()
                    {
                        Amount = amountOwed,
                        Email = customer.Email
                    };
                    return View("PaymentForm", paymentForm);
                }
                else
                {
                    CustomError customError = new CustomError()
                    {
                        Message = ConfigurationManager.AppSettings["NoBalanceDue"]
                    };
                    return View("CustomError", customError);
                }
            }
            else
            {
                CustomError customError = new CustomError()
                {
                    Message = ConfigurationManager.AppSettings["AccountNotFound"]
                };
                return View("CustomError", customError);
            }
        }

        [HttpPost]
        public ActionResult MakePayment(PaymentForm paymentForm)
        {
            try
            {
                var myCharge = new StripeChargeCreateOptions();
                myCharge.Amount = Convert.ToInt32(paymentForm.Amount * 100);
                myCharge.Currency = "usd";
                myCharge.Description = "CDAR Services";

                string[] tokens = paymentForm.Expiration.Split('/');

                var token = new StripeTokenCreateOptions();
                token.Card = new StripeCreditCardOptions()
                {
                    Number = paymentForm.CardNumber,
                    ExpirationMonth = Convert.ToInt32(tokens[0]),
                    ExpirationYear = Convert.ToInt32(tokens[1])
                };

                var tokenService = new StripeTokenService();
                StripeToken stripeToken = tokenService.Create(token);

                myCharge.SourceTokenOrExistingSourceId = stripeToken.Id;

                myCharge.ApplicationFee = Convert.ToInt32((paymentForm.Amount * 100) * Convert.ToDecimal(.025f));
                myCharge.Capture = true;

                Customer customer = db.Customers.FirstOrDefault(c => c.Email == paymentForm.Email);
                Client client = db.Clients.FirstOrDefault(c => c.ID == customer.ClientID);
                StripeInfo stripeInfo = db.StripeInfoes.FirstOrDefault(s => s.ID == client.StripeID);

                StripeRequestOptions options = new StripeRequestOptions()
                {
                    ApiKey = ConfigurationManager.AppSettings["StripeApiKey"],
                    StripeConnectAccountId = stripeInfo.UserID
                };

                var chargeService = new StripeChargeService();
                StripeCharge stripeCharge = chargeService.Create(myCharge, options);
                
                Appointment appointment = db.Appointments.FirstOrDefault(a => a.CustomerID == customer.ID && a.OwesPayment == true);

                appointment.OwesPayment = false;
                appointment.ChargeID = stripeCharge.Id;
                db.Appointments.Attach(appointment);
                var entry = db.Entry(appointment);
                entry.Property(e => e.OwesPayment).IsModified = true;
                db.SaveChanges();
            }
            catch (StripeException ex)
            {
                CustomError customError = new CustomError();
                switch (ex.StripeError.ErrorType)
                {
                    case "card_error":
                        customError.Message = ConfigurationManager.AppSettings["InvalidCardNumber"];
                        ErrorHandler.PostError(db, ConfigurationManager.AppSettings["InvalidCardNumber"]);
                        return View("CustomError", customError);
                    default:
                        ErrorHandler.PostError(db, ex.ToString());
                        return View("Error");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.PostError(db, ex.ToString());
                return View("Error");
            }

            return View("PaymentSuccess");
        }

        private void CreateAppointment(Customer customer, InputFormModel ifm)
        {
            try
            {
                Customer customerInstance = db.Customers.FirstOrDefault(c => c.ClientID == customer.ClientID && c.Email == customer.Email
                        && c.FirstName == customer.FirstName && c.LastName == c.LastName);

                if (customerInstance != null)
                {
                    Appointment appointment = new Appointment()
                    {
                        Brand = ifm.DeviceBrand,
                        Model = ifm.DeviceModel,
                        CustomerID = customerInstance.ID,
                        DateOfRequest = DateTime.Now,
                        OwesPayment = true,
                        Amount = 0
                    };

                    db.Appointments.Add(appointment);
                    db.SaveChanges();

                    SendEmail(ifm, "CDAR Appointment Request");
                }
                else ErrorHandler.PostError(db, ConfigurationManager.AppSettings["InvalidCustomerInstance"]);
            }
            catch (Exception ex)
            {
                ErrorHandler.PostError(db, ex.ToString());
            }
        }

        private void SendEmail(InputFormModel ifm, string subject)
        {
            try
            {
                string clientEmail = db.Clients.FirstOrDefault(c => c.Name == ifm.Institution).Email;
                string name = string.Format("{0} {1}", ifm.FirstName, ifm.LastName);
                WebRequest request = WebRequest.Create(GetLink(subject));
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string body = reader.ReadToEnd();
                body = body.Replace("[Request Type]", subject)
                           .Replace("[Customer]", name)
                           .Replace("[Email]", ifm.Email)
                           .Replace("[Brand]", ifm.DeviceBrand)
                           .Replace("[Model]", ifm.DeviceModel)
                           .Replace("[Issue]", ifm.Issue);

                MailHandler mHandler = new MailHandler(ifm.Institution, clientEmail);
                mHandler.Send(subject, body);
            }
            catch (Exception ex)
            {
                ErrorHandler.PostError(db, ex.ToString());
            }
        }

        private void SendContactEmail(InputFormModel ifm)
        {
            string subject = "Contact Us";
            WebRequest request = WebRequest.Create(GetLink(subject));
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string body = reader.ReadToEnd();
            body = body.Replace("[Email]", ifm.Email)
                       .Replace("[Message]", ifm.Issue);

            MailHandler mHandler = new MailHandler(null, ConfigurationManager.AppSettings["CDARContactUs"]);
            mHandler.Send(subject, body);
        }

        private string GetLink(string subject)
        {
            if (subject == "Contact Us")
                return "http://www.blitzkrieg-games.com/CDARContactEmail.txt";
            else if (subject == "CDAR Diagnostic Request")
                return "http://www.blitzkrieg-games.com/CDARDiagnosticEmail.txt";
            else return "http://www.blitzkrieg-games.com/CDARAppointmentEmail.txt";
        }
    }
}
