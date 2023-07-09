var sessionTimer = (function () {
    'use strict'

    var newSignIn = document.getElementById("new-sign-in");

    function init() {
        if (newSignIn) {
            startSessionTimer();
        }        
    }


    function startSessionTimer() {
        
        var sessionTimeInMinutes = 20;
        var timerInSeconds = sessionTimeInMinutes * 60; 

        var interval = setInterval(function () {

            var minutes = Math.floor(timerInSeconds / 60);
            var seconds = timerInSeconds % 60;

            timerInSeconds--;

            if (timerInSeconds < 0) {

                clearInterval(interval);


                var banner = document.getElementById("login-session-expired");
                banner.style.display = "block";


                setTimeout(function () {
                    banner.style.display = "none";
                }, 5000);

                
            }
        }, 1000); 
        
    }

    return {
        init: init
    };

})();

sessionTimer.init();