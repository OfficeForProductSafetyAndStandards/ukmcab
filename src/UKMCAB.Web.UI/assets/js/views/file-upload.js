var fileUpload = (function () {
    'use strict';

    var fileUploadInputCheckboxes = document.getElementsByClassName('file-upload-checkbox');

    function init() {
        if (fileUploadInputCheckboxes) {

            var checkboxesArray = Array.from(fileUploadInputCheckboxes);

            checkboxesArray.forEach(function (checkbox) {

                if (checkbox.type === 'checkbox') {
                    checkbox.checked = false;
                }
            });
        }
    }

    return {
        init: init
    };
})();

fileUpload.init();
