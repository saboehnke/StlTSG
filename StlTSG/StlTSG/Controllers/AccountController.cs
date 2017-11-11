using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using StlTSG.Models;
using Stripe;
using System.Configuration;

namespace StlTSG.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private CDARModel db = new CDARModel();

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Account()
        {
            Client client = db.Clients.FirstOrDefault(c => c.Email == User.Identity.Name);

            if (client == null)
                return View("Error");

            var userID = UserManager.FindByEmail(client.Email).Id;

            AccountModel model = new AccountModel()
            {
                ProfileImage = client.ProfileImage,
                Subscribed = client.IsActive,
                IsEmailVerified = UserManager.IsEmailConfirmed(userID),
                UserID = userID,
                Email = client.Email
            };

            return View(model);
        }

        public ActionResult ImageUpload(HttpPostedFileBase file)
        {
            byte[] bytes;
            Image image;
            using (MemoryStream ms = new MemoryStream())
            {
                file.InputStream.CopyTo(ms);
                bytes = ms.GetBuffer();
                image = Image.FromStream(ms);
            }

            long fileLength = file.InputStream.Length;
            if (fileLength < 4000000)// && image.Width == 500 && image.Height == 500)
            {
                Client client = db.Clients.FirstOrDefault(c => c.Email == User.Identity.Name);
                if (client != null)
                {
                    client.ProfileImage = bytes;
                    db.Clients.Attach(client);
                    var entry = db.Entry(client);
                    entry.Property(e => e.ProfileImage).IsModified = true;
                    db.SaveChanges();
                    TempData["ProfileImageMessage"] = ConfigurationManager.AppSettings["ProfileImageSet"];
            }
        }
            else TempData["ProfileImageError"] = ConfigurationManager.AppSettings["InvalidImageSize"];

            return RedirectToAction("Account", "Account");
        }

        [HttpGet]
        public ActionResult StripeCallback(StripeCallback response)
        {
            try
            {
                var stripeOAuthTokenService = new StripeOAuthTokenService();
                var _stripeOAuthTokenCreateOptions = new StripeOAuthTokenCreateOptions()
                {
                    ClientSecret = ConfigurationManager.AppSettings["StripeSecret"],
                    Code = response.Code,
                    GrantType = "authorization_code"
                };

                StripeOAuthToken stripeOAuthToken = stripeOAuthTokenService.Create(_stripeOAuthTokenCreateOptions);

                Client client = db.Clients.FirstOrDefault(c => c.Email == User.Identity.Name);

                if (client.StripeID != null)
                {
                    string customerID = db.StripeInfoes.FirstOrDefault(s => s.ID == client.StripeID).CustomerID;
                    var customerService = new StripeCustomerService();
                    StripeCustomer stripeCustomer = customerService.Get(customerID);

                    var subscriptionService = new StripeSubscriptionService();
                    StripeSubscription stripeSubscription = subscriptionService.Create(customerID, ConfigurationManager.AppSettings["PlanID"]);

                    StripeInfo stripeInfo = db.StripeInfoes.FirstOrDefault(s => s.ID == client.StripeID);
                    stripeInfo.SubscriptionID = stripeSubscription.Id;
                    var sEntry = db.Entry(stripeInfo);
                    sEntry.Property(e => e.SubscriptionID).IsModified = true;
                    db.SaveChanges();

                    client.IsActive = true;
                    db.Clients.Attach(client);
                    var cEntry = db.Entry(client);
                    cEntry.Property(e => e.IsActive).IsModified = true;
                    db.SaveChanges();
                }
                else
                {
                    var myCustomer = new StripeCustomerCreateOptions();
                    myCustomer.Email = client.Email;
                    myCustomer.Description = client.Name;

                    var customerService = new StripeCustomerService();
                    StripeCustomer stripeCustomer = customerService.Create(myCustomer);

                    var subscriptionService = new StripeSubscriptionService();
                    StripeSubscription stripeSubscription = subscriptionService.Create(stripeCustomer.Id, ConfigurationManager.AppSettings["PlanID"]);

                    StripeInfo stripeInfo = new StripeInfo()
                    {
                        UserID = stripeOAuthToken.StripeUserId,
                        CustomerID = stripeCustomer.Id,
                        SubscriptionID = stripeSubscription.Id
                    };
                    db.StripeInfoes.Add(stripeInfo);
                    db.SaveChanges();

                    stripeInfo = db.StripeInfoes.FirstOrDefault(s => s.UserID == stripeOAuthToken.StripeUserId && s.CustomerID == stripeCustomer.Id);

                    client.StripeID = stripeInfo.ID;
                    client.IsActive = true;
                    db.Clients.Attach(client);
                    var entry = db.Entry(client);
                    entry.Property(e => e.StripeID).IsModified = true;
                    entry.Property(e => e.IsActive).IsModified = true;
                    db.SaveChanges();
                }
                SendSubscriptionEmail(client.Name, client.Email);
            }
            catch (StripeException ex)
            {
                ErrorHandler.PostError(db, string.Format("{0}: {1}", ex.StripeError.Error, ex.StripeError.ErrorDescription));
                return View("Error");
            }
            catch (Exception ex)
            {
                ErrorHandler.PostError(db, ex.ToString());
                return View("Error");
            }
            return RedirectToAction("Dashboard", "Dashboard");
        }

        public ActionResult Unsubscribe()
        {
            Client client = db.Clients.FirstOrDefault(c => c.Email == User.Identity.Name);

            if (client != null)
            {
                StripeInfo sInfo = db.StripeInfoes.FirstOrDefault(s => s.ID == client.StripeID);

                if (sInfo != null)
                {
                    try
                    {
                        var subscriptionService = new StripeSubscriptionService();
                        subscriptionService.Cancel(sInfo.SubscriptionID, true);

                        client.IsActive = false;
                        db.Clients.Attach(client);
                        var entry = db.Entry(client);
                        entry.Property(e => e.IsActive).IsModified = true;
                        db.SaveChanges();

                        return View("Account");
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.PostError(db, ex.ToString());
                        return View("Error");
                    }
                }
                else
                {
                    ErrorHandler.PostError(db, ConfigurationManager.AppSettings["NoStripeInfo"]);
                    return View("Error");
                }
            }
            else
            {
                ErrorHandler.PostError(db, ConfigurationManager.AppSettings["NoClientFound"]);
                return View("Error");
            }
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    Client client = db.Clients.FirstOrDefault(c => c.Email == model.Email);

                    if (client == null)
                        return View("Error");

                    if (client.IsActive)
                        return RedirectToAction("Dashboard", "Dashboard");
                    else return RedirectToAction("Account");
                case SignInStatus.LockedOut:
                    TempData["Error"] = ConfigurationManager.AppSettings["LockedOut"];
                    return RedirectToAction("Index", "Home");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    TempData["Error"] = ConfigurationManager.AppSettings["InvalidCredentials"];
                    return RedirectToAction("Index", "Home");
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Client client = db.Clients.FirstOrDefault(c => c.Name.ToLower() == model.Institution);

                if (client == null)
                {
                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // Add user id to client table and set that as user.id
                        CreateClientInstance(model, user.Id);

                        // Send validation email to user
                        SendValidationEmail(user.Id, model.Email);

                        return RedirectToAction("Account", "Account");
                    }
                    AddErrors(result);
                }
                else
                {
                    TempData["RegisterAccountError"] = ConfigurationManager.AppSettings["InstitutionAlreadyExists"];
                    return RedirectToAction("Index", "Home");
                }
            }
            
            ErrorHandler.PostError(db, ConfigurationManager.AppSettings["RegisterError"]);
            return View("Error");
        }

        private void CreateClientInstance(RegisterViewModel model, string userID)
        {
            Client client = new Client()
            {
                Name = model.Institution,
                Email = model.Email,
                UserID = Guid.Parse(userID),
                IsActive = false
            };

            db.Clients.Add(client);
            db.SaveChanges();
        }

        public ActionResult SendValidationEmail(string userID, string email)
        {
            try
            {
                string code = UserManager.GenerateEmailConfirmationToken(userID);
                string institution = db.Clients.FirstOrDefault(c => c.Email == email).Name;
                string subject = "CDAR Account Validation";
                string body = string.Format("Dear {0},<br><br>Thank you for registering for an account with CDAR! Please click on the "
                    + "link below to validate your email address.<br><a href=\"{1}\" title=\"Email Validation\">Validate Account</a><br><br>Thank you,<br><br>CDAR", institution,
                    Url.Action("ConfirmEmail", "Account", new { UserID = userID, Code = code }, Request.Url.Scheme));

                MailHandler mHandler = new MailHandler(institution, email);
                mHandler.Send(subject, body);

                TempData["VerificationEmailMessage"] = ConfigurationManager.AppSettings["EmailSent"];
            }
            catch (Exception ex)
            {
                TempData["Error"] = ConfigurationManager.AppSettings["ErrorSendingEmail"];
                ErrorHandler.PostError(db, ex.ToString());
            }
            return RedirectToAction("Account", "Account");
        }

        private void SendForgottenPasswordEmail(string userID, string email)
        {
            try
            {
                string code = UserManager.GeneratePasswordResetToken(userID);
                string institution = db.Clients.FirstOrDefault(c => c.Email == email).Name;
                string subject = "CDAR Forgotten Password";
                string body = string.Format("Dear {0},<br><br>Please click on the link below to reset your password.<br>"
                    + "<a href=\"{1}\" title=\"Email Validation\">Reset Password</a><br><br>Thank you,<br><br>CDAR", institution,
                    Url.Action("ResetPassword", "Account", new { Email = email, Code = code }, Request.Url.Scheme));

                MailHandler mHandler = new MailHandler(institution, email);
                mHandler.Send(subject, body);
            }
            catch (Exception ex)
            {
                ErrorHandler.PostError(db, ex.ToString());
            }
        }

        public void SendSubscriptionEmail(string institution, string email)
        {
            try
            {
                string subject = "CDAR Subscription";
                string body = string.Format("Dear {0},<br><br>Your subscription to CDAR's services has successfully been activated!<br><br>"
                    + "Thank you for subscribing!<br><br>Thank you,<br><br>CDAR", institution);

                MailHandler mHandler = new MailHandler(institution, email);
                mHandler.Send(subject, body);
            }
            catch (Exception ex)
            {
                ErrorHandler.PostError(db, ex.ToString());
            }
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);

            if (result.Succeeded)
                return View("ConfirmEmail");
            else
            {
                string errors = "";
                foreach (var error in result.Errors)
                {
                    string.Format("{0}\r\n", error);
                }
                ErrorHandler.PostError(db, errors);
            }
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // Send password reset email to user
                SendForgottenPasswordEmail(user.Id, model.Email);

                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string email, string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(string password, string confirmPassword)
        {
            if (password == confirmPassword)
            {
                var user = UserManager.FindByEmail(User.Identity.Name);
                var code = UserManager.GeneratePasswordResetToken(user.Id);
                var result = UserManager.ResetPassword(user.Id, code, password);
                if (result.Succeeded)
                    TempData["ChangePasswordMessage"] = "Password successfully changed.";
                else TempData["ChangePasswordMessage"] = "Could not change password.";
            }
            else TempData["ChangePasswordMessage"] = "Passwords do not match.";
            return RedirectToAction("Account", "Account");
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            string err = "";
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
                err = string.Format("{0}\r\n{1}", err, error);
            }
            if (!String.IsNullOrEmpty(err))
                ErrorHandler.PostError(db, err);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}