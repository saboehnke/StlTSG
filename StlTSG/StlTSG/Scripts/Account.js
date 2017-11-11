$('.cp-hidden').on('click', function (e)
{
    e.preventDefault();
    var $this = $(this);
    var $collapse = $this.closest('.collapse-group').find('.collapse');
    $collapse.collapse('toggle');
});

var ClearChangePassword = function ()
{
    document.getElementById("cPassword").value = "";
    document.getElementById("cConfirmPassword").value = "";
}

var changePassword = document.getElementById("cPassword");
var changeConfirmPassword = document.getElementById("cConfirmPassword");

var PasswordConfirmation = function ()
{
    var regex = new RegExp("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{6,}$", "g");

    if (changePassword.value !== changeConfirmPassword.value)
        changeConfirmPassword.setCustomValidity("Passwords do not match.");
    else if (!regex.test(changePassword.value))
        changeConfirmPassword.setCustomValidity("Password must meet the following criteria:\r\n"
            + " - At least one upper case letter.\r\n"
            + " - At least one lower case letter.\r\n"
            + " - At least one digit.\r\n"
            + " - At least one special character.\r\n"
            + " - Minimum of 6 characters.");
    else changeConfirmPassword.setCustomValidity("");
}

changePassword.onchange = PasswordConfirmation;
changeConfirmPassword.onkeyup = PasswordConfirmation;