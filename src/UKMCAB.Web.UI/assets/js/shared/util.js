(function () {
    window.addEventListener("load", function () {
        var set = document.querySelectorAll(".js-history-back");
        for (var i = 0; i < set.length; i++) {
            set[i].addEventListener("click", function () {
                window.history.back();
            });
        }

        var set = document.querySelectorAll("form.js-submit-once");
        for (var i = 0; i < set.length; i++) {
            var el = set[i];
            var buttons = el.querySelectorAll(".js-submit-button");
            el.addEventListener("submit", function (e) {
                if (e.target.dataset.submitting) {
                    e.preventDefault();
                    return false;
                } else {
                    e.target.dataset["submitting"] = true;
                    buttons.forEach(function(btn) {
                        var t = btn.dataset.submittingText;
                        if (t) {
                            btn.innerHTML = t;
                        }
                        btn.disabled = true;
                    });
                }
            });
        }
    })
})();
