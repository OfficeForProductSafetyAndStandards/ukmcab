var cabProfile = (function () {
    'use strict';

    var archiveModal = document.getElementById('archive-modal');
    var archiveReason = document.getElementById("archive-reason");
    var archiveReasonError = document.getElementById("archive-reason-error");
    var archiveErrorMessage = document.getElementById("archive-error-message");
    var archiveReasonFormGroup = document.getElementById("archive-reason-formgroup");
    var submitButton = document.getElementById("archive-submit-button");
    var cabId = document.getElementById("CABId");
    var archiveModalCloseButtons = document.querySelectorAll("#archive-modal .modal-close");

    function init() {
        if (archiveModal) {
            submitButton.addEventListener("click", submitDetails);
            archiveModalCloseButtons.forEach(function (e) {
                e.addEventListener('click', removeError);
            });
        }
    }

    function submitDetails(e) {
        e.preventDefault();
        if (validValue(archiveReason.value) && validValue(cabId.value)) {
            var formData = new FormData();
            formData.append("ArchiveReason", archiveReason.value);
            formData.append("CABId", cabId.value);
            fetch("/search/cab-profile/archive/submit-js",
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
                        location.reload();
                    } else {
                        displayError(result.errorMessage);
                    }
                }).catch((error) => {
                    displayError(error);
                });

        } else {
            displayError("Enter the reason for archiving this CAB profile");
        }
    }

    function validValue(text) {
        return text && text.length > 0;
    }

    function displayError(message) {
        archiveReason.classList.add("govuk-input--error");
        archiveReasonFormGroup.classList.add("govuk-form-group--error");
        archiveReasonError.classList.remove("govuk-visually-hidden");
        archiveErrorMessage.innerText = message;
    }

    function removeError() {
        archiveReason.classList.remove("govuk-input--error");
        archiveReasonFormGroup.classList.remove("govuk-form-group--error");
        archiveReasonError.classList.add("govuk-visually-hidden");
        archiveErrorMessage.innerText = "";
    }

    return {
        init: init
    };
})();

cabProfile.init();