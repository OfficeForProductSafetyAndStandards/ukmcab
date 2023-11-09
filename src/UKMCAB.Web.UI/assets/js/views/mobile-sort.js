var mobileSort = (function () {
    'use strict';
    var sortOption = document.querySelectorAll(".sort-option");
    function init() {
        if (sortOption) {
            sortOption.forEach(elem => elem.addEventListener("change", function () {
                this.closest("form").submit();
            }));
        }
    }
    return {
        init: init
    };
})();

mobileSort.init();