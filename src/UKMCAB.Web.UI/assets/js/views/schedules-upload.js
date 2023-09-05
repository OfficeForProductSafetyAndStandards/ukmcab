//var scheduleUploadPage = (function () {
//    'use strict';

//    var form = document.querySelector('#schedule-upload-form');
//    var uploadInput = document.querySelector('input[type="file"]');
//    var progressBar = document.getElementById('progress-bar');
//    var progressText = document.getElementById('progress-text');
//    var uploadProgress = document.getElementById('upload-progress');
//    var totalFileSize = 0; // Variable to store the total file size
//    var uploadedSize = 0; // Variable to store the cumulative uploaded size

//    function uploadProgressUpdate(e) {
//        var files = uploadInput.files; // Get the selected files

//        // Reset the cumulative progress variables for each upload
//        totalFileSize = 0;
//        uploadedSize = 0;

//        // Calculate the total file size
//        for (var i = 0; i < files.length; i++) {
//            totalFileSize += files[i].size;
//        }

//        var xhr = new XMLHttpRequest();

//        // Loop through the selected files and send separate requests for each file
//        for (var i = 0; i < files.length; i++) {
//            var file = files[i];
//            var formData = new FormData();
//            formData.append('file', file); // Assuming your server expects a parameter named 'file'

//            //var xhr = new XMLHttpRequest();
//            xhr.open('POST', form.action, true);

//            xhr.upload.onprogress = function (event) {
//                if (event.lengthComputable) {
//                    uploadedSize += event.loaded; // Update the cumulative uploaded size
//                    var percent = (uploadedSize / totalFileSize) * 100;
//                    progressBar.value = percent;
//                    progressText.innerText = percent.toFixed(2) + '%';
//                }
//            };

//            xhr.onreadystatechange = function () {
//                if (xhr.readyState === XMLHttpRequest.DONE) {
//                    // Handle the response for each file, e.g., update a list of uploaded files
//                    alert(file.name + 'uploaded');
//                }
//            };

//            xhr.send(formData);
//        }

//        uploadProgress.style.display = 'block';
//    }

//    function init() {
//        form.addEventListener('submit', function (e) {
//            //e.preventDefault(); // Prevent the form from submitting normally
//            uploadProgressUpdate(e); // Trigger the upload progress update
//        });
//    }

//    return {
//        init: init
//    };
//})();

//scheduleUploadPage.init();




















//var scheduleUploadPage = (function () {
//    'use strict';

//    var form = document.querySelector('#schedule-upload-form'); //check if the right form is selected
//    var formData = new FormData(form);

//    var progressBar = document.getElementById('progress-bar');
//    var progressText = document.getElementById('progress-text');
//    var uploadProgress = document.getElementById('upload-progress'); // div
//    //var uploadButton = document.getElementById('schedule-upload-button');
//    var uploadButton = document.querySelector('#schedule-upload-button-group > button' )


//    function uploadProgressUpdate(e) {
//        //e.preventDefault();        

//        var xhr = new XMLHttpRequest();
//        xhr.open('POST', form.action, true);

//        //xhr.send(formData);

//        xhr.onloadstart = (e) => {
//            //uploadProgress.style.display = 'block';
//            //if (e.lengthComputable) {
//            //    var percent = (e.loaded / e.total) * 100;
//            //    progressBar.value = percent;
//            //    progressText.innerText = percent.toFixed() + '%';
//            //};
//            //alert(e.loaded + ' out of ' + e.total);
//        };

//        xhr.upload.onprogress = function (e) {
//            var percent = (e.loaded / e.total) * 100;

//            if (e.lengthComputable) {
//                alert(e.loaded + ' out of ' + e.total);
//                //var percent = (e.loaded / e.total) * 100;
//                progressBar.value = percent;
//                progressText.innerText = percent.toFixed() + '%';                
//            };
//        };

//        xhr.upload.onload = function () {
//        //xhr.onreadystatechange = function () {
//            //if (xhr.readyState === XMLHttpRequest.DONE && xhr.status == 200) {
//            if (xhr.readyState === XMLHttpRequest.DONE) {
//                uploadProgress.style.display = 'none';                
//            };
//        };

//        xhr.send(formData);
//        uploadProgress.style.display = 'block';
                                                                  
//    }
//    function init() {
//        uploadButton.addEventListener('click', uploadProgressUpdate);
//    }

//    return {
//        init: init
//    };

//})();

//scheduleUploadPage.init();