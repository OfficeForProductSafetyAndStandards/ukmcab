var searchPage = (function() {
    'use strict';

    var searchResultsPage = document.getElementById("search-result-page");
    var searchResultsForm = document.getElementById("search-results-form");
    var searchResultsFormColumn = document.getElementById("search-results-form-column");
    var searchResultsListColumn = document.getElementById("search-results-list-column");

    var govukPhaseBanner = document.getElementById("govuk-phase-banner");
    var govukHeader = document.getElementById("govuk-header");
    var govukFooter = document.getElementById("govuk-footer");

    var filterOptions = document.querySelectorAll('.search-form-filter-option');

    var searchResultsFilterToggle = document.getElementById('search-results-filter-toggle');
    var searchResultsListToggle = document.getElementById('search-results-list-toggle');

    var mql;

    function init() {
        if (searchResultsPage && searchResultsForm) {
            searchResultsPage.classList.add("js-enabled");
            searchResultsFilterToggle.addEventListener('click', showFilter);
            searchResultsListToggle.addEventListener('click', showList);

            mql = window.matchMedia('(min-width: 40.0625em)');
            if (mql.matches) {
                setUpFilterOptions();
            }
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


    function toggleFilter(filter) {
        if (filter) {
            searchResultsListColumn.classList.add("search-result-mobile-hidden");
            govukPhaseBanner.classList.add("search-result-mobile-hidden");
            govukHeader.classList.add("search-result-mobile-hidden");
            govukFooter.classList.add("search-result-mobile-hidden");
            searchResultsFormColumn.classList.remove("search-result-mobile-hidden");
        } else {
            searchResultsFormColumn.classList.add("search-result-mobile-hidden");
            searchResultsListColumn.classList.remove("search-result-mobile-hidden");
            govukPhaseBanner.classList.remove("search-result-mobile-hidden");
            govukHeader.classList.remove("search-result-mobile-hidden");
            govukFooter.classList.remove("search-result-mobile-hidden");
        }
    }

    return {
        init: init
    };
})();

searchPage.init();