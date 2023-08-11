var copyUrlLink = (function () {
    'use strict';


    var copyLinkInput = document.getElementById("copy-link");
    var shareUrl = document.getElementById("share-url");

    function copyLink() {
        var urlToCopy = shareUrl.innerText;
        navigator.clipboard.writeText(urlToCopy)
            .then(() => {
                alert("successfully copied");
            })
            .catch((e) => {
                alert("something went wrong : " + e);
            });
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
