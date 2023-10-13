var copyUrlLink = (function () {
    'use strict';

    var copyLinkInput = document.getElementById("copy-link");
    var shareUrl = document.getElementById("share-url");

    function copyLink() {
        var urlToCopy = shareUrl.innerText;
        navigator.clipboard.writeText(urlToCopy)
            .then(() => {
                successfullyCopied();               
            })
            .catch((e) => {
                console.log("something went wrong : " + e);
            });
    }

    function successfullyCopied() {
        var popup = document.getElementById("copyPopup");
        popup.classList.toggle("show");

        setTimeout(function () {
            popup.classList.remove("show");
        }, 2000);
    }

    function init() {
        if (shareUrl) {
            copyLinkInput.addEventListener('click', copyLink);
        }
    }

    return {
        init: init
    };

})();

copyUrlLink.init();
