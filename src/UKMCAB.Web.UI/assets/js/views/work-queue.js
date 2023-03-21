var workQueuePage = (function() {
    'use strict';

    var filterOptions = document.querySelectorAll('.work-queue-options');
    var workQueueForm = document.getElementById("work-queue-form");
    var workQueuePage = document.getElementById("admin-work-queue");


    function init() {
        if (workQueueForm && filterOptions && workQueuePage) {
            workQueuePage.classList.add("js-enabled");


            filterOptions.forEach((fo) => {
                fo.addEventListener("change", () => {
                    workQueueForm.submit();
                });
            });
        }
    }

    return {
        init: init
    };
})();

workQueuePage.init();