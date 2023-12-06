let notificationTabs = (function () {
    'use strict';
    let tabs = document.querySelectorAll("div.notifications-tabs a.govuk-tabs__tab");

    function init() {
        if (tabs) {
            tabs.forEach(elem => elem.addEventListener("click", function () {
                const urlParams = new URLSearchParams(window.location.search);
                const pageNumberKey = 'pagenumber';
                const pageNumber = urlParams.get(pageNumberKey);
                if (pageNumber !== undefined && pageNumber !== null && pageNumber !== "1")
                {
                    window.location.search = pageNumberKey +"=1"
                }
            }));
        }
    }

    return {
        init: init
    };
})();

notificationTabs.init();