const GOVUKFrontend = require('./govuk-frontend-all.js');
import "./shared/feedback-form.js";
import "./shared/identity.js";
import "./shared/modal.js";
import "./shared/util.js";
import "./shared/feed-links";
import "./views/body-details.js";
import "./views/cab-management.js";
import "./views/cab-profile.js";
import "./views/search.js";
import "./views/schedules-upload.js";
import "../scss/main.scss";


// overriding to allow js on mobile, this must run before the initAll() method  
GOVUKFrontend.Tabs.prototype.setupResponsiveChecks = function () {
    this.setup();
};

GOVUKFrontend.initAll();