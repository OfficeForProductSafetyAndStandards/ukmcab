using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Tests.Models.ViewModels.Admin.CAB
{
    [TestFixture]
    public class CABDetailsViewModelValidatorTests
    {
        private CABDetailsViewModelValidator _validator;

        [SetUp]
        public void Setup()
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _validator = new CABDetailsViewModelValidator(httpContextAccessorMock.Object);
        }

        [Test]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var model = new CABDetailsViewModel { Name = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Enter a CAB name");
        }

        [Test]
        public void Should_Not_Have_Error_When_Name_Is_Not_Empty()
        {
            var model = new CABDetailsViewModel { Name = "CAB Name" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Test]
        public void Should_Have_Error_When_CABNumber_Is_Empty()
        {
            var model = new CABDetailsViewModel { CABNumber = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CABNumber).WithErrorMessage("Enter a CAB number");
        }

        [Test]
        public void Should_Not_Have_Error_When_CABNumber_Is_Not_Empty()
        {
            var model = new CABDetailsViewModel { CABNumber = "12345" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CABNumber);
        }

        [Test]
        public void Should_Have_Error_When_CabNumberVisibility_Is_Empty()
        {
            var model = new CABDetailsViewModel { CabNumberVisibility = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CabNumberVisibility).WithErrorMessage("Select who should see the CAB number");
        }

        [Test]
        public void Should_Not_Have_Error_When_CabNumberVisibility_Is_Not_Empty()
        {
            var model = new CABDetailsViewModel { CabNumberVisibility = "Public" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CabNumberVisibility);
        }
    }
}
