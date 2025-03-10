﻿var searchPage = (function () {
    'use strict';

    var searchPage = document.getElementById("search-page");

    var searchResultsForm = document.getElementById("search-results-form");
    var searchFilterContainer = document.getElementById("search-filter-container");
    var searchKeywordContainer = document.getElementById("search-keyword-container");
    var searchResultsListColumn = document.getElementById("search-results-list-column");
    var searchResultsPaginationMobile = document.getElementById("search-results-pagination-container-mobile");

    var govukPhaseBanner = document.getElementById("govuk-phase-banner");
    var govukHeader = document.getElementById("govuk-header");
    var govukFooter = document.getElementById("govuk-footer");
    var feedbackSection = document.getElementById("feedback-form-details");
    var bottomAtomFeed = document.getElementsByClassName("atom-feed-bottom")[0];

    var filterOptions = document.querySelectorAll('.search-form-filter-option');

    var searchResultsFilterToggle = document.getElementById('search-results-filter-toggle');
    var searchResultsListToggle = document.getElementById('search-results-list-toggle');
    var clearFiltersLink = document.getElementById('clear-filters-link');
    var keywordsInput = document.getElementById('Keywords');
    var keywordsButton = document.getElementById('search-keyword-button');
    const searchBox = document.querySelector(".search-box");
    const clearButton = document.querySelector(".clear-icon");

    const skipToSearchResultsAnchor = document.getElementById("skip-to-search-results-anchor");
    const pageTitleElement = document.getElementById("search-page-title");

    const searchKeywordButton = document.getElementById("search-keyword-button");
    const searchForm = document.getElementById('search-results-form');

    var mql;

    function init() {
        if (searchPage && searchResultsForm) {
            searchForm.addEventListener('keydown', (event) => {
                if (event.key === 'Enter') {
                    event.preventDefault();
                    searchKeywordButton.click();
                }
            });

            searchFilterContainer.classList.add("search-result-mobile-hidden");
            searchResultsFilterToggle.addEventListener('click', showFilter);
            searchResultsListToggle.addEventListener('click', showList);

            clearButton.style.display = searchBox.value ? "block" : "none";

            clearButton.addEventListener("click", () => {
                searchBox.value = "";
                clearButton.style.display = "none";
            });

            keywordsButton.addEventListener('click', function () {
                keywordsButton.focus();
            });

            mql = window.matchMedia('(min-width: 40.0625em)');
            mql.addListener(checkMode.bind(this));
            checkMode();
        }
    }

    function checkMode() {
        if (mql.matches) {
            setUpFilterOptions();
        } else {
            teardown();
        }
    }

    function teardown() {
        filterOptions.forEach(function (fo) {
            fo.onchange = function () {
            };
        });
        clearFiltersLink.addEventListener('click', clearFilters);
    }

    function setUpFilterOptions() {
        filterOptions.forEach(function (fo) {
            fo.onchange = function () {
                searchResultsForm.submit();
            };
        });
        clearFiltersLink.removeEventListener('click', clearFilters);
    }

    function clearFilters(e) {
        e.preventDefault();
        keywordsInput.value = "";
        filterOptions.forEach(function (fo) {
            fo.checked = false;
        });
    }

    function showFilter(e) {
        e.preventDefault();
        toggleFilter(true);
        searchResultsListToggle.focus();
    }

    function showList(e) {
        e.preventDefault();
        toggleFilter(false);
        searchResultsFilterToggle.focus();
    }

    function toggleFilter(filter) {
        if (filter) {
            searchResultsListColumn.classList.add("search-result-mobile-hidden");
            govukPhaseBanner.classList.add("search-result-mobile-hidden");
            govukHeader.classList.add("search-result-mobile-hidden");
            govukFooter.classList.add("search-result-mobile-hidden");
            bottomAtomFeed.classList.add("search-result-mobile-hidden");
            feedbackSection.classList.add("search-result-mobile-hidden");
            searchKeywordContainer.classList.add("search-result-mobile-hidden");
            searchResultsPaginationMobile.classList.add("search-result-mobile-hidden");
            skipToSearchResultsAnchor.classList.add("search-result-mobile-hidden");
            pageTitleElement.classList.add("search-result-mobile-hidden");

            searchFilterContainer.classList.remove("search-result-mobile-hidden");
        } else {
            searchResultsListColumn.classList.remove("search-result-mobile-hidden");
            govukPhaseBanner.classList.remove("search-result-mobile-hidden");
            govukHeader.classList.remove("search-result-mobile-hidden");
            govukFooter.classList.remove("search-result-mobile-hidden");
            bottomAtomFeed.classList.remove("search-result-mobile-hidden");
            feedbackSection.classList.remove("search-result-mobile-hidden");
            searchKeywordContainer.classList.remove("search-result-mobile-hidden");
            searchResultsPaginationMobile.classList.remove("search-result-mobile-hidden");
            skipToSearchResultsAnchor.classList.remove("search-result-mobile-hidden");
            pageTitleElement.classList.remove("search-result-mobile-hidden");

            searchFilterContainer.classList.add("search-result-mobile-hidden");
        }
    }

    return {
        init: init
    };
})();

searchPage.init();