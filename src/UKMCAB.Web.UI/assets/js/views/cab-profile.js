var cabProfile = (function () {
    'use strict';

    var archiveModal = document.getElementById('archive-modal');
    var archiveSubmitButton = document.getElementById("archive-submit-button");
    var archiveModalCloseButtons = document.querySelectorAll("#archive-modal .modal-close");

    var unarchiveModal = document.getElementById('unarchive-modal');
    var unarchiveSubmitButton = document.getElementById("unarchive-submit-button");
    var unarchiveModalCloseButtons = document.querySelectorAll("#unarchive-modal .modal-close");

    var cabId = document.getElementById("CABId");




    function init() {
        if (archiveModal) {
            var archiveModalGroup = {
                reason: document.getElementById("archive-reason"),
                reasonError: document.getElementById("archive-reason-error"),
                reasonErrorMessage: "Enter the reason for archiving this CAB profile",
                errorMessage: document.getElementById("archive-error-message"),
                reasonFormGroup: document.getElementById("archive-reason-formgroup"),
                reasonId: "ArchiveReason",
                url: "/search/cab-profile/archive/submit-js",
                redirect: ""
            };
            archiveSubmitButton.addEventListener("click", (e) => {
               e.preventDefault();
               submitDetails(archiveModalGroup);
            });
            archiveModalCloseButtons.forEach(function (e) { 
                e.addEventListener('click', () => {
                    removeError(archiveModalGroup);
                });
            });
        }
        if (unarchiveModal) {
            var unarchiveModalGroup = {
                reason: document.getElementById("unarchive-reason"),
                reasonError: document.getElementById("unarchive-reason-error"),
                reasonErrorMessage: "Enter the reason for unarchiving this CAB profile",
                errorMessage: document.getElementById("unarchive-error-message"),
                reasonFormGroup: document.getElementById("unarchive-reason-formgroup"),
                reasonId: "UnarchiveReason",
                url: "/search/cab-profile/unarchive/submit-js",
                redirect: "/admin/cab/summary/" + cabId.value
            };
            unarchiveSubmitButton.addEventListener("click", (e) => {
                e.preventDefault();
                submitDetails(unarchiveModalGroup);
            });
            unarchiveModalCloseButtons.forEach(function (e) {
                e.addEventListener('click', () => {
                    removeError(unarchiveModalGroup);
                });
            });
        }
    }

    function submitDetails(modalGroup) {
        if (validValue(modalGroup.reason.value) && validValue(cabId.value)) {
            var formData = new FormData();
            formData.append(modalGroup.reasonId, modalGroup.reason.value);
            formData.append("CABId", cabId.value);
            fetch(modalGroup.url,
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
                        if (modalGroup.redirect) {
                            location.href = modalGroup.redirect;
                        }
                        else {
                            location.reload();
                        }
                    } else {
                        displayError(modalGroup, result.errorMessage);
                    }
                }).catch((error) => {
                    displayError(modalGroup, error);
                });

        } else {
            displayError(modalGroup, modalGroup.reasonErrorMessage);
        }
    }

    function validValue(text) {
        return text && text.length > 0;
    }

    function displayError(modalGroup, message) {
        modalGroup.reason.classList.add("govuk-input--error");
        modalGroup.reasonFormGroup.classList.add("govuk-form-group--error");
        modalGroup.reasonError.classList.remove("govuk-visually-hidden");
        modalGroup.errorMessage.innerText = message;
    }

    function removeError(modalGroup) {
        modalGroup.reason.classList.remove("govuk-input--error");
        modalGroup.reasonFormGroup.classList.remove("govuk-form-group--error");
        modalGroup.reasonError.classList.add("govuk-visually-hidden");
        modalGroup.errorMessage.innerText = "";
    }

    return {
        init: init
    };
})();

cabProfile.init();