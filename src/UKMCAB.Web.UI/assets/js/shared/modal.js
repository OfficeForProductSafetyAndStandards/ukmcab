var modal = (function () {
    'use strict';

    var modalButtons = document.querySelectorAll(".modal-button");
    var closeButtons = document.querySelectorAll(".modal-close");
    var activeModal;

    function init() {
        if (modalButtons) {
            modalButtons.forEach(function (e) {
                e.addEventListener('click', showModal);
            });

            closeButtons.forEach(function (e) {
                e.addEventListener('click', closeModal);
            });

            window.onclick = function (e) {
                if (e.target == activeModal) {
                    closeModal(e);
                }
            }
        }
    }

    function showModal(e) {
        e.preventDefault();
        var button = e.currentTarget;
        var modalId = button.dataset.modalId;
        activeModal = document.getElementById(modalId);
        activeModal.style.display = "block";
    }

    function closeModal(e) {
        e.preventDefault();
        activeModal.style.display = "none";
    }

    return {
        init: init
    };
})();

modal.init();
