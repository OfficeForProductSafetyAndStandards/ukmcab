var cabProfilePage = (function () {
    'use strict';

    var productScheduleFilterForm = document.getElementById("product-schedule-filter-form");
    var productScheduleFilter = document.getElementById("product-schedule-filter");
    var productScheduleFilterButton = document.getElementById("apply-product-schedule-filter");

    function init() {
        if (productScheduleFilterForm && productScheduleFilter) {
            productScheduleFilter.onchange = function () {
                productScheduleFilterForm.submit();
            };
        }

        if (productScheduleFilterButton) {
            productScheduleFilterButton.remove();
        }
    }

    return {
        init: init
    };
})();

cabProfilePage.init();