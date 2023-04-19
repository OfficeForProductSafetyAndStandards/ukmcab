var helpPage = (function () {
    'use strict';

    var hashValue = window.location.hash;

    function init() {

        if (window.location.pathname === "/help" && !hashValue) {
            document.getElementById('contact-footer-link').addEventListener('click', function (e) {

                e.preventDefault();

                var url = e.target.href;

                window.location.href = url;

                setTimeout(function () {
                    location.reload();
                }, 200); 
            });
        }


        if (hashValue) {

            try {
                window.sessionStorage.clear();
                window.sessionStorage.setItem("accordion-default-content-20", "true");
            }
            catch (error) {

                console.error("Error accessing session storage:", error);
                
                var targetDiv = document.getElementById("contact-info");

                if (targetDiv) {
                    targetDiv.classList.add("govuk-accordion__section--expanded");
                }

            }
        }

    }

    return {
        init: init
    };
})();

helpPage.init();