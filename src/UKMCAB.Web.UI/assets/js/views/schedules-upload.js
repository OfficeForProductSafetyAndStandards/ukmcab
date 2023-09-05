var scheduleUploadPage = (function () {
    'use strict';

    var form = document.querySelector('form'); //check if the right form is selected
    var formData = new FormData(form);

    var progressBar = document.getElementById('progress-bar');
    var progressText = document.getElementById('progress-text');
    var uploadProgress = document.getElementById('upload-progress'); // div
    var uploadButton = document.getElementById('schedule-upload-button');


    function uploadProgressUpdate(e) {
        //e.preventDefault();        

        var xhr = new XMLHttpRequest();
        xhr.open('POST', form.action, true);

        xhr.upload.onprogress = function (e) {
            if (e.lengthComputable) {
                var percent = (e.loaded / e.total) * 100;
                progressBar.value = percent;
                progressText.innerText = percent.toFixed() + '%';                
            }
        };

        xhr.onreadystatechange = function () {
            //if (xhr.readyState === XMLHttpRequest.DONE && xhr.status == 200) {
            if (xhr.readyState === XMLHttpRequest.DONE) {
                uploadProgress.style.display = 'none';                
            }
        };

        xhr.send(formData);
        uploadProgress.style.display = 'block';
                                                                  
    }
    function init() {
        uploadButton.addEventListener('click', uploadProgressUpdate);
    }

    return {
        init: init
    };

})();

scheduleUploadPage.init();