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




class CreateEditCabViewModel {
    constructor() {
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

        self.addRegulation = function() {
            self.regulations.push(new RegulationViewModel());
        };

        self.removeRegulation = function(regulation) {
            self.regulations.remove(regulation);
        };
    }
}

class RegulationViewModel {
    constructor() {
        var self = this;
        self.name = ko.observable(null); //name of the reg; probably going to be a select list.
        self.products = ko.observableArray([]);
        self.regs = [
            "Cableway installations",
            "Construction products",
            "Ecodesign",
            "Electromagnetic compatibility",
            "Equipment and protective systems for use in potentially explosive atmospheres",
            "Explosives",
            "Gas appliances and related",
            "Lifts",
            "Machinery",
            "Marine equipment",
            "Measuring instruments",
            "Medical devices",
            "Noise emissions in the environment by equipment for use outdoors",
            "Non-automatic weighing instruments",
            "Personal protective equipment",
            "Pressure equipment",
            "Pyrotechnics",
            "Radio equipment",
            "Railway interoperability",
            "Recreational craft",
            "Simple pressure vessels",
            "Toys",
            "Transportable pressure equipment"
        ];
        self.product = ko.observable(null);

        self.addProduct = function () {
            var p = new ProductViewModel();
            self.product(p);
            self.products.push(p);
        };

        self.removeProduct = function(product) {
            self.products.remove(product);
        };

        self.saveProduct = function () {
            self.product(null);
        };

        self.removeProduct = function (product) {
            self.product(null);
            self.products.remove(product);
        };

        self.editProduct = function (product) {
            self.product(product);
        };
    }
}

class ProductViewModel { 
    constructor() {
        var self = this;
        self.name = ko.observable();
        self.productCode = ko.observable();
        self.partName = ko.observable();
        self.moduleName = ko.observable();
        self.scheduleName = ko.observable();
        self.standardsNumber = ko.observable();
    }
}
