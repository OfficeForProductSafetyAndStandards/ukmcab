var signIn = (function () {
    'use strict';

    var showPasswordLinks = document.querySelectorAll('.show-password-button');


    function init() {
        if (showPasswordLinks) {
            setupShowPasswordLinks();
        }
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