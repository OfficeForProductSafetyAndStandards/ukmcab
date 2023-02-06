const GOVUKFrontend = require('./govuk-frontend-all.js');
import "./option-select.js";
import "../scss/main.scss";

// overriding to allow js on mobile, this must run before the initAll() method  
GOVUKFrontend.Tabs.prototype.setupResponsiveChecks = function () {
    this.setup();
};

GOVUKFrontend.initAll();

// Results page

var nodes = document.querySelectorAll('.app-c-option-select');

for (var i = 0; i < nodes.length; i++) {
    new GOVUK.Modules.OptionSelect(nodes[i]).init();
}
var submitForm = document.querySelector("form");
var checkBoxes = document.querySelectorAll('.filter__input');
checkBoxes.forEach(function (cbx) {
    cbx.onchange = function () {
        submitForm.submit();
    };
});

var clearFilters = document.querySelector("#clear-filters-action");
if (clearFilters) {
    clearFilters.addEventListener('click',
        function(e) {
            e.preventDefault();
            checkBoxes.forEach(function(cbx) {
                cbx.checked = false;
            });
            submitForm.submit();
        });
}

var logoutLink = document.querySelector("#logoutLink");
if (logoutLink) {
    logoutLink.addEventListener('click',
        function (e) {
            e.preventDefault();
            document.querySelector("#logoutForm").submit();
        });
}