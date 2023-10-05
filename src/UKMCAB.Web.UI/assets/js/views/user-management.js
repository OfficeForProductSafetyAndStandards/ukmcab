var userManagementPage = (function () {
    'use strict';

    var sortOption = document.getElementById("sortOption");
    var sortForm = document.getElementById("sortForm");
    var sortButton = document.getElementById("sortButton");



    function toggleSortDirectionAndSubmit() {
        var sortDirectionField = document.getElementById("sortDirection");
        if (sortDirectionField.value === "asc") {
            sortDirectionField.value = "desc";
        } else {
            sortDirectionField.value = "asc";
        }
        sortForm.submit();
    }

    function init() {
        if (sortForm && sortButton && sortOption) {

            sortOption.addEventListener("change", function () {
                sortForm.submit();
            });

            sortButton.addEventListener("click", function (e) {
                e.preventDefault(); 
                toggleSortDirectionAndSubmit();
            });
        }
    }

    return {
        init: init
    };

})();

userManagementPage.init();