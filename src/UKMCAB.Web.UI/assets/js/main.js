const GOVUKFrontend = require('./govuk-frontend-all.js');
import "./option-select.js";
import "../scss/main.scss";


GOVUKFrontend.Tabs.prototype.setupResponsiveChecks = function () {
    this.setup();
};

GOVUKFrontend.initAll();

// overriding the GDS experimental accordion to not display the Show/Hide all section and to rename the accordion text

GOVUKFrontend.Accordion.prototype.setExpanded = function (expanded, $section) {
    var $icon = $section.querySelector('.' + this.upChevronIconClass);
    var $showHideText = $section.querySelector('.' + this.sectionShowHideTextClass);
    var $button = $section.querySelector('.' + this.sectionButtonClass);
    var $newButtonText = expanded ? 'Hide details of appointment' : 'Show details of appointment';

    // Build additional copy of "this section" for assistive technology and place inside toggle link
    var $visuallyHiddenText = document.createElement('span');
    $visuallyHiddenText.classList.add('govuk-visually-hidden');
    $visuallyHiddenText.innerHTML = ' this section';

    $showHideText.innerHTML = $newButtonText;
    $showHideText.appendChild($visuallyHiddenText);
    $button.setAttribute('aria-expanded', expanded);

    // Swap icon, change class
    if (expanded) {
        $section.classList.add(this.sectionExpandedClass);
        $icon.classList.remove(this.downChevronIconClass);
    } else {
        $section.classList.remove(this.sectionExpandedClass);
        $icon.classList.add(this.downChevronIconClass);
    }
};

GOVUKFrontend.Accordion.prototype.init = function () {
    // Check for module
    if (!this.$module) {
        return;
    }
    this.initSectionHeaders();
};

function nodeListForEach(nodes, callback) {
    if (window.NodeList.prototype.forEach) {
        return nodes.forEach(callback);
    }
    for (var i = 0; i < nodes.length; i++) {
        callback.call(window, nodes[i], i, nodes);
    }
};

var $accordions = document.querySelectorAll('[data-module="ukmcab-accordion"]');

nodeListForEach($accordions, function ($accordion) {
    new GOVUKFrontend.Accordion($accordion).init();
});


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