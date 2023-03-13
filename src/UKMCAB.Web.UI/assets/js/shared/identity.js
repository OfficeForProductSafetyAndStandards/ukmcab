var signIn = (function () {
    'use strict';

    var showPasswordLinks = document.querySelectorAll('.show-password-button');
    var logoutLink = document.getElementById("logoutLink");
    var logoutForm = document.getElementById("logoutForm");


    function init() {
        if (showPasswordLinks) {
            setupShowPasswordLinks();
        }
        if (logoutLink) {
            setUpLogoutLink();
        }
    }

    function setUpLogoutLink() {
        logoutLink.addEventListener('click',
            function (e) {
                e.preventDefault();
                logoutForm.submit();
            });
    }

    function setupShowPasswordLinks() {
        showPasswordLinks.forEach(function(pl) {
            pl.addEventListener('click', togglePassword);
        });
    }

    function togglePassword(e) {
        e.preventDefault();
        var toggleButton = e.currentTarget;
        var input = toggleButton.previousElementSibling;
        if (input.type === "password") {
            input.type = "text";
            toggleButton.innerText = "Hide";
        } else {
            input.type = "password";
            toggleButton.innerText = "Show";
        }
    }

    return {
        init: init
    };
})();

signIn.init();