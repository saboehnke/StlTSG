/************************* ↓ Sidebar Menu ↓ *************************/

// Closes the sidebar menu
$("#menu-close").click(function (e)
{
    e.preventDefault();
    $("#sidebar-wrapper").toggleClass("active");
});

// Opens the sidebar menu
$("#menu-toggle").click(function (e)
{
    e.preventDefault();
    $("#sidebar-wrapper").toggleClass("active");
});

/************************* ↑ Sidebar Menu ↑ *************************/

/************************* ↓ Anchor Scrolling ↓ *************************/

// Scrolls to the selected menu item on the page
$(function ()
{
    $('a[href*=\\#]:not([href=\\#],[data-toggle],[data-target],[data-slide])').click(function ()
    {
        if (location.pathname.replace(/^\//, '') === this.pathname.replace(/^\//, '') || location.hostname === this.hostname)
        {
            var target = $(this.hash);
            target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
            if (target.length)
            {
                $('html,body').animate(
                {
                    scrollTop: target.offset().top
                }, 1000);
                return false;
            }
        }
    });
});

//#to-top button appears after scrolling
var fixed = false;
$(document).scroll(function ()
{
    if ($(this).scrollTop() > 250)
    {
        if (!fixed)
        {
            fixed = true;
            $('#to-top').show("slow", function ()
            {
                $('#to-top').css(
                {
                    position: 'fixed',
                    display: 'block'
                });
            });
        }
    }
    else
    {
        if (fixed)
        {
            fixed = false;
            $('#to-top').hide("slow", function ()
            {
                $('#to-top').css(
                {
                    display: 'none'
                });
            });
        }
    }
});

/************************* ↑ Anchor Scrolling ↑ *************************/

/************************* ↓ Institution Dropdown ↓ *************************/

var ChangeChosenInstitution = function ()
{
    var dd = document.getElementById("institutionDropdown");
    var inst = dd.options[dd.selectedIndex].text;
    document.getElementById("chosenDInstitution").value = inst;
    document.getElementById("chosenAInstitution").value = inst;
    document.getElementById("institutionDDropdown").selectedIndex = dd.selectedIndex;
    document.getElementById("institutionADropdown").selectedIndex = dd.selectedIndex;
}

var ChangeDInstitution = function ()
{
    var dd = document.getElementById("institutionDDropdown");
    var inst = dd.options[dd.selectedIndex].text;
    document.getElementById("chosenDInstitution").value = inst;
}

var ChangeAInstitution = function ()
{
    var dd = document.getElementById("institutionADropdown");
    var inst = dd.options[dd.selectedIndex].text;
    document.getElementById("chosenAInstitution").value = inst;
}

/************************* ↑ Institution Dropdown ↑ *************************/

/************************* ↓ Form Handling ↓ *************************/

var ClearDiagnostic = function ()
{
    document.getElementById("dEmail").value = "";
    document.getElementById("dMessage").value = "";
}

var ClearAppointment = function ()
{
    document.getElementById("aEmail").value = "";
    document.getElementById("aFirstName").value = "";
    document.getElementById("aLastName").value = "";
    document.getElementById("aBrand").value = "";
    document.getElementById("aModel").value = "";
    document.getElementById("aMessage").value = "";
}

var ClearPayment = function ()
{
    document.getElementById("pEmail").value = "";
}

var ClearPaymentForm = function ()
{
    document.getElementById("cardNumber").value = "";
    document.getElementById("cardExpiration").value = "";
}

var ClearForgotPassword = function ()
{
    document.getElementById("fEmail").value = "";
}

var ClearForgottenPassword = function ()
{
    document.getElementById("fPassword").value = "";
    document.getElementById("fConfirmPassword").value = "";
}

var registerPassword = document.getElementById("rPassword");
var registerConfirmPassword = document.getElementById("rConfirmPassword");

var PasswordConfirmation = function ()
{
    var regex = new RegExp("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$", "g");

    if (registerPassword.value !== registerConfirmPassword.value)
        registerConfirmPassword.setCustomValidity("Passwords do not match.");
    else if (!regex.test(registerPassword.value))
        registerConfirmPassword.setCustomValidity("Password must meet the following criteria:\r\n"
            + " - At least one upper case letter.\r\n"
            + " - At least one lower case letter.\r\n"
            + " - At least one digit.\r\n"
            + " - At least one special character.\r\n"
            + " - Minimum of 6 characters.");
    else registerConfirmPassword.setCustomValidity("");
}

registerPassword.onchange = PasswordConfirmation;
registerConfirmPassword.onkeyup = PasswordConfirmation;

var cardNumber = document.getElementById("cardNumber");
var cardExpiration = document.getElementById("cardExpiration");

var CardConfirmation = function ()
{
    var regex = new RegExp("^((0[1-9])|(1[0-2]))\\/(\\d{4})$");

    if (cardNumber.value.length !== 16)
    {
        cardNumber.setCustomValidity("Please enter a valid card number.");
        cardExpiration.setCustomValidity("");
    }
    else if (!regex.test(cardExpiration.value))
    {
        cardExpiration.setCustomValidity("Please enter a valid expiration date (MM/YYYY)");
        cardNumber.setCustomValidity("");
    }
    else
    {
        cardNumber.setCustomValidity("");
        cardExpiration.setCustomValidity("");
    }
}

if (cardNumber !== null && cardExpiration !== null)
{
    cardNumber.onkeyup = CardConfirmation;
    cardExpiration.onfocusout = CardConfirmation;
}

var dMessage = document.getElementById("dMessage");
var aMessage = document.getElementById("aMessage");
var institution = document.getElementById("institutionDropdown");
var dInstitution = document.getElementById("institutionDDropdown");
var aInstitution = document.getElementById("institutionADropdown");

var ValidateDRequestForm = function ()
{
    if (dInstitution.value !== "" && dInstitution.value !== "Select Your Institution")
        dInstitution.setCustomValidity("");
    else dInstitution.setCustomValidity("Please select a valid institution for your request.");
}

var ValidateARequestForm = function ()
{
    if (aInstitution.value !== "" && aInstitution.value !== "Select Your Institution")
        aInstitution.setCustomValidity("");
    else aInstitution.setCustomValidity("Please select a valid institution for your request.");
}

var ValidateRequestForms = function ()
{
    ChangeChosenInstitution();
    ValidateARequestForm();
    ValidateDRequestForm();
}

if (dMessage !== null && aMessage !== null)
{
    dMessage.onkeyup = ValidateDRequestForm;
    aMessage.onkeyup = ValidateARequestForm;
    //institution.onchange = ValidateRequestForms;
    dInstitution.onchange = ChangeDInstitution;
    aInstitution.onchange = ChangeAInstitution;
}

/************************* ↑ Form Handling ↑ *************************/