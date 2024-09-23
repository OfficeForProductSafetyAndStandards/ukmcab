var cabManagementPage = (function() {
    'use strict';

    var filterOptions = document.querySelectorAll('.cab-management-options');
    var cabManagementForm = document.getElementById("cab-management-form");
    var cabManagementPage = document.getElementById("admin-cab-management");
    
    function init() {
        if (cabManagementForm && filterOptions && cabManagementPage) {
            cabManagementPage.classList.add("js-enabled");

            filterOptions.forEach((fo) => {
                fo.addEventListener("change", () => {
                    cabManagementForm.submit();
                });
            });
        }

        document.addEventListener('DOMContentLoaded', function () {
            const urlParams = new URLSearchParams(window.location.search);
            const tabName = urlParams.get('tabName');
            const tabNav = urlParams.get('tabNav');

            if (tabNav == "true") {
                var tab = document.getElementById(tabName);
                tab.focus();
            }
        });
    }

    return {
        init: init
    };
})();

cabManagementPage.init();