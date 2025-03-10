﻿var addDesignatedStandardPage = (function () {
    'use strict';

    var addDesignatedStandardPage = document.getElementById("add-designated-standard-page");
    var addDesignatedStandardSelectAllCheckbox = document.getElementById("add-designated-standard-select-all");

    var keywordsButton = document.getElementById('search-keyword-button');
    const searchBox = document.querySelector(".search-box");
    const clearButton = document.querySelector(".clear-icon");
    const searchForm = document.getElementById('designatedStandardsForm');

    function init() {
        if (addDesignatedStandardPage) {

            searchForm.addEventListener('keydown', (event) => {
                if (event.key === 'Enter') {
                    event.preventDefault();
                    keywordsButton.click();
                }
            });

            clearButton.style.display = searchBox.value ? "block" : "none";

            clearButton.addEventListener("click", () => {
                searchBox.value = "";
                clearButton.style.display = "none";
            });

            keywordsButton.addEventListener('click', function () {
                keywordsButton.focus();
            });
        }

        if (addDesignatedStandardSelectAllCheckbox) {
            addDesignatedStandardSelectAllCheckbox.addEventListener('change', function () {
                var checkboxes = document.querySelectorAll('input[type="checkbox"][id^="PageSelectedDesignatedStandardIds-"]');
                for (var i = 0; i < checkboxes.length; i++) {
                    checkboxes[i].checked = this.checked;
                }
            });
        }
    }

    return {
        init: init
    };
})();

addDesignatedStandardPage.init();