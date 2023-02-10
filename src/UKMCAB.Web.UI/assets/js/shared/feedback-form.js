var feedbackForm = (function() {
    'use strict';

    // Form buttons
    var detailsCloseButton = document.getElementById("details-close-button");
    var feedbackSubmitButton = document.getElementById("feedback-sumbit-button");
    var feedbackSubmitAgainButton = document.getElementById("feedback-form-submit-again-button");

    // Form fields
    var whatWereYouDoing = document.getElementById("WhatWereYouDoing");
    var whatWentWrong = document.getElementById("WhatWentWrong");

    // Form sections
    var feedbackDetails = document.getElementById("feedback-form-details");
    var feedbackForm = document.getElementById("feedback-form");
    var feedbackSuccess = document.getElementById("feedback-success");
    var feedbackError = document.getElementById("feedback-form-error");
    var feedbackErrorMessage = document.getElementById("feedback-form-error-message");

    function init() {
        if (detailsCloseButton && feedbackSubmitButton && feedbackSubmitAgainButton) {
            detailsCloseButton.addEventListener('click', closeDetails);
            feedbackSubmitButton.addEventListener('click', submitDetails);
            feedbackSubmitAgainButton.addEventListener('click', reset);
        }
    }

    function closeDetails(e) {
        e.preventDefault();
        if (feedbackDetails) {
            feedbackDetails.removeAttribute("open");
        }
    }

    function submitDetails(e) {
        e.preventDefault();
        if (validValue(whatWereYouDoing.value) && validValue(whatWentWrong.value)) {
            var formData = new FormData();
            formData.append("WhatWereYouDoing", whatWereYouDoing.value);
            formData.append("WhatWentWrong", whatWentWrong.value);
            formData.append("ReturnURL", window.location.pathname);

            fetch("/feedback-form/submit-js",
                {
                    body: formData,
                    method: "post"
                })
                .then((data) => {
                    if (data.ok) {
                        return data.json();
                    } else {
                        throw new Error("There was an error submitting your details, please try again.");
                    }
                }).then((result) => {
                    if (result.submitted) {
                        displaySuccess();
                    } else {
                        displayErrors(result.errorMessage);
                    }
                }).catch((error) => {
                    displayErrors(error);
                });
        } else {
            displayErrors();
        }
    }

    function validValue(text) {
        return text && text.length > 0;
    }

    function displaySuccess() {
        feedbackForm.classList.add("govuk-visually-hidden");
        feedbackError.classList.add("govuk-visually-hidden");
        feedbackSuccess.classList.remove("govuk-visually-hidden");
    }

    function displayErrors(errorMessage) {
        if (!errorMessage) {
            errorMessage = "";
            if (!validValue(whatWereYouDoing.value)) {
                whatWereYouDoing.classList.add("feedback-form-error");
                errorMessage = "Please enter details of what you were doing";
            } else {
                whatWereYouDoing.classList.remove("feedback-form-error");
            }
            if (!validValue(whatWentWrong.value)) {
                whatWentWrong.classList.add("feedback-form-error");
                errorMessage += errorMessage.length ? " and what went wrong" : "Please enter details of what went wrong";
            } else {
                whatWentWrong.classList.remove("feedback-form-error");
            }
        }

        feedbackError.classList.remove("govuk-visually-hidden");
        feedbackErrorMessage.innerText = errorMessage + ".";
    }

    function reset() {
        // Show form
        feedbackForm.classList.remove("govuk-visually-hidden");
        // Remove errors
        feedbackError.classList.add("govuk-visually-hidden");
        feedbackErrorMessage.innerText = "";
        whatWereYouDoing.classList.remove("feedback-form-error");
        whatWentWrong.classList.remove("feedback-form-error");
        // Clear values
        whatWereYouDoing.value = "";
        whatWentWrong.value = "";
        // Remove success
        feedbackSuccess.classList.add("govuk-visually-hidden");
    }

    return {
        init: init
    };

})();

feedbackForm.init();