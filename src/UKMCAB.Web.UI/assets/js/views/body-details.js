var bodyDetailsPage = (function () {
    'use strict';

    var testLocationsContainer = document.getElementById("test-locations-container");
    var testLocations = document.querySelectorAll(".test-location");

    var clonedTestLocation;
    var addTestLocationLink = document.getElementById("add-test-location-link");

    function init() {
        if (testLocationsContainer && testLocations) {
            var removeButtons = testLocationsContainer.getElementsByClassName("test-location-remove-link");
            for(const rb of removeButtons) {
                rb.addEventListener("click", removeTestLocation);
            }

            var testLocation = testLocations[0];
            clonedTestLocation = testLocation.cloneNode(true);
            showHideRemoveLink(clonedTestLocation, true);
            if (testLocations.length > 1) {
                testLocations.forEach(tl => {
                    showHideRemoveLink(tl, true);
                });
            }
            addTestLocationLink.addEventListener('click', addTestLocation);
        }
    }

    function removeTestLocation(e) {
        e.preventDefault();
        var link = e.currentTarget;
        var parentTestLocation = link.parentElement;
        parentTestLocation.remove();
        updateTestLocations();
        if (testLocations.length == 1) {
            showHideRemoveLink(testLocations[0], false);
        }
    }

    function showHideRemoveLink(tl, show) {
        if (show) {
            tl.getElementsByClassName("test-location-remove-link")[0].classList.remove("govuk-visually-hidden");
        } else {
            tl.getElementsByClassName("test-location-remove-link")[0].classList.add("govuk-visually-hidden");
        }
    }

    function addTestLocation(e) {
        e.preventDefault();
        if (testLocations.length == 1) {
            showHideRemoveLink(testLocations[0], true);
        }
        var newNode = clonedTestLocation.cloneNode(true);
        newNode.getElementsByClassName("test-location-remove-link")[0].addEventListener("click", removeTestLocation);
        testLocationsContainer.appendChild(newNode);
        updateTestLocations();
    }

    function updateTestLocations() {
        testLocations = document.querySelectorAll(".test-location");
    }

    return {
        init: init
    };
})();

bodyDetailsPage.init();