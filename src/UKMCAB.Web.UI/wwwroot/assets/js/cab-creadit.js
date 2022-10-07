ko.bindingHandlers.enterkey = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var inputSelector = 'input,textarea,select';
        $(document).on('keypress', inputSelector, function (e) {
            var allBindings = allBindingsAccessor();
            $(element).on('keypress', 'input, textarea, select', function (e) {
                var keyCode = e.which || e.keyCode;
                if (keyCode !== 13) {
                    alert('a');
                    return true;
                }

                var target = e.target;
                target.blur();

                allBindings.enterkey.call(viewModel, viewModel, target, element);
                alert('b');
                return false;
            });
        });
    }
};




function createEditCabViewModel() {
    var self = this;
    self.name = ko.observable(null);
    self.address = ko.observable(null);
    self.website = ko.observable(null);
    self.email = ko.observable(null);
    self.phone = ko.observable(null);
    self.bodyNumber = ko.observable(null);
    self.bodyType = ko.observable(null);
    self.registeredOfficeLocation = ko.observable(null);
    self.testingLocations = ko.observable(null);
    self.regulations = ko.observableArray([]);

    self.addRegulation = function () {
        self.regulations.push(new regulationViewModel());
    };

    self.removeRegulation = function (regulation) {
        self.regulations.remove(regulation);
    };
}

function regulationViewModel() {
    var self = this;
    self.name = ko.observable(null); //name of the reg; probably going to be a select list.
    self.description = ko.observable(null);
    self.groups = ko.observableArray([]);

    self.addGroup = function () {
        self.groups.push(new groupViewModel());
    };

    self.removeGroup = function (group) {
        self.groups.remove(group);
    };

}


function groupViewModel() {
    var self = this;
    self.products = new productsViewModel();
    self.caps = new capsViewModel();
    self.standards = new standardsViewModel();
}

function productsViewModel() {
    var self = this;
    self.header = ko.observable();
    self.footer = ko.observable();
    self.newProduct = ko.observable();
    self.products = ko.observableArray([]); // list of text
    self.categories = ko.observableArray([]); // list of categoryViewModel
    self.onEnter = function (d, e) {
        e.keyCode === 13 && self.addProduct();
        return true;
    };
    self.addProduct = function () {
        self.products.push(self.newProduct());
        self.newProduct("");
    };
    self.remove = function (product) {
        self.products.remove(product);
    };
}

function categoryViewModel() {
    self.name = ko.observable();
    self.description = ko.observable();
    self.products = ko.observableArray([]); // list of text
}

function capsViewModel() {
    var self = this;
    self.header = ko.observable();
    self.footer = ko.observable();
    self.schedules = ko.observableArray([]); // list of scheduleViewModel
}

function scheduleViewModel() {
    var self = this;
    self.name = ko.observable();
    self.description = ko.observable();
    self.partsModules = ko.observableArray([]); // list of text
}

function standardsViewModel() {
    var self = this;
    self.header = ko.observable();
    self.footer = ko.observable();
    self.items = ko.observableArray([]); // list of text
}