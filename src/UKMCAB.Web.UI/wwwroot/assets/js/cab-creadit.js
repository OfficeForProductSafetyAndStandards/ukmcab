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
    constructor(data, editUrlTemplate) {
        var self = this;
        self.id = data ? data.id : null;
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
            self.regulations.push(new RegulationViewModel());
        };

        self.removeRegulation = function (regulation) {
            self.regulations.remove(regulation);
        };

        self.save = async function () {
            var unmapped = ko.mapping.toJS(self);
            if (self.id) {
                var result = await axios.put("/api/cabs", unmapped);
            } else {
                var result = await axios.post("/api/cabs", unmapped);
                self.id = result.data;
                var url = editUrlTemplate.replace("guid", self.id);
                window.location.href = url;
            }
        };

        if (data) {
            self.id  = data.id;
            self.name(data.name);
            self.address(data.address);
            self.website(data.website);
            self.email(data.email);
            self.phone(data.phone);
            self.bodyNumber(data.bodyNumber);
            self.bodyType(data.bodyType);
            self.registeredOfficeLocation(data.registeredOfficeLocation);
            self.testingLocations(data.testingLocations);

            for (var i = 0; i < data.regulations.length; i++) {
                var r = data.regulations[i];
                var vm = new RegulationViewModel();
                vm.name(r.name);

                for (var j = 0; j < r.products.length; j++) {
                    var p = r.products[j];
                    var pvm = new ProductViewModel();
                    pvm.name(p.name);
                    pvm.productCode(p.productCode);
                    pvm.partName(p.partName);
                    pvm.moduleName(p.moduleName);
                    pvm.scheduleName(p.scheduleName);
                    pvm.standardsNumber(p.standardsNumber);
                    vm.products.push(pvm);
                }

                self.regulations.push(vm);
            }

        }
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
