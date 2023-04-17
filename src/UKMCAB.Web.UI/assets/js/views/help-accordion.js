//$(document).ready(function () {
//    // Open the last accordion section
//    $('#contact-info').next('.help-accordion').slideDown();
//});



//var helpPage = (function ($) {
//    'use strict';
//    function init() {
//        $('.help-accordion:last-child').slideDown();
//    }

//    return {
//        init: init
//    };
//})(jQuery);

//helpPage.init();



var helpPage = (function ($) {
    'use strict';

    var contactAccordion = document.getElementById("accordion-default-heading-20");
    function init() {
        if (contactAccordion) {
            contactAccordion.slideDown();
        }
        
    }

    return {
        init: init
    };
})();

helpPage.init();

