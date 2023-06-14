(function () {
    window.addEventListener("load", function () {
        var set = document.querySelectorAll(".js-history-back");
        for (var i = 0; i < set.length; i++) {
            set[i].addEventListener("click", function () {
                window.history.back();
            });
        }
    })
})();
