var resetPassword = document.getElementById("rePassword");
var resetConfirmPassword = document.getElementById("reConfirmPassword");

var ResetPasswordConfirmation = function ()
{
    var regex = new RegExp("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$", "g");

    if (resetPassword.value !== resetConfirmPassword.value)
        resetConfirmPassword.setCustomValidity("Passwords do not match.");
    else if (!regex.test(resetPassword.value))
        resetConfirmPassword.setCustomValidity("Password must meet the following criteria:\r\n"
            + " - At least one upper case letter.\r\n"
            + " - At least one lower case letter.\r\n"
            + " - At least one digit.\r\n"
            + " - At least one special character.\r\n"
            + " - Minimum of 6 characters.");
    else resetConfirmPassword.setCustomValidity("");
}

resetPassword.onchange = ResetPasswordConfirmation;
resetConfirmPassword.onkeyup = ResetPasswordConfirmation;