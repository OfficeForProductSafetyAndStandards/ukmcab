var searchPage = (function() {
    'use strict';

    var searchResultsPage = document.getElementById("search-result-page");
    var searchResultsForm = document.getElementById("search-results-form");
    var searchResultsFormColumn = document.getElementById("search-results-form-column");
    var searchResultsListColumn = document.getElementById("search-results-list-column");

    var filterOptions = document.querySelectorAll('.search-form-filter-option');

    var clearFilterButton = document.getElementById('clear-filters-link');
    var searchResultsFilterToggle = document.getElementById('search-results-filter-toggle');
    var searchResultsListToggle = document.getElementById('search-results-list-toggle');

    function init() {
        if (searchResultsPage && searchResultsForm && clearFilterButton) {
            searchResultsPage.classList.add("js-enabled");
            setUpFilterOptions();
            clearFilterButton.addEventListener('click', clearFilters);
            searchResultsFilterToggle.addEventListener('click', showFilter);
            searchResultsListToggle.addEventListener('click', showList);
        }
    }

    function setUpFilterOptions() {
        filterOptions.forEach(function (fo) {
            fo.onchange = function () {
                searchResultsForm.submit();
            };
        });
    }

    function showFilter(e) {
        e.preventDefault();
        toggleFilter(true);
    }

    function showList(e) {
        e.preventDefault();
        toggleFilter(false);
    }

    function clearFilters(e) {
        e.preventDefault();
        filterOptions.forEach(function (cbx) {
            cbx.checked = false;
        });
        searchResultsForm.submit();
    }

    function toggleFilter(filter) {
        if (filter) {
            searchResultsListColumn.classList.add("search-result-mobile-hidden");
            searchResultsFormColumn.classList.remove("search-result-mobile-hidden");
        } else {
            searchResultsFormColumn.classList.add("search-result-mobile-hidden");
            searchResultsListColumn.classList.remove("search-result-mobile-hidden");
        }
    }

    return {
        init: init
    };
})();

searchPage.init();