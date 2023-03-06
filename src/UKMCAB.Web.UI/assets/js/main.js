const GOVUKFrontend = require('./govuk-frontend-all.js');
import "./option-select.js";
import "./shared/feedback-form.js";
import "./views/search.js";
import "../scss/main.scss";


// overriding to allow js on mobile, this must run before the initAll() method  
GOVUKFrontend.Tabs.prototype.setupResponsiveChecks = function () {
    this.setup();
};

GOVUKFrontend.initAll();


var logoutLink = document.querySelector("#logoutLink");
if (logoutLink) {
    logoutLink.addEventListener('click',
        function (e) {
            e.preventDefault();
            document.querySelector("#logoutForm").submit();
        });
}