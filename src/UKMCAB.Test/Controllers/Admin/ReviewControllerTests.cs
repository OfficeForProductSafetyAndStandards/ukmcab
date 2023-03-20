//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using UKMCAB.Core.Models;
//using UKMCAB.Core.Services;
//using UKMCAB.Web.UI.Areas.Admin.Controllers;
//using UKMCAB.Web.UI.Models.ViewModels.Admin;

//namespace UKMCAB.Test.Controllers.Admin
//{
//    public class ReviewControllerTests
//    {
//        private ReviewController _sut;
//        private ICABAdminService _CABAdminService;
//        private Mock<ICABRepository> _ICABRepository = new();

//        [SetUp]
//        public void Setup()
//        {
//            _CABAdminService = new CABAdminService(_ICABRepository.Object);

//            _sut = new ReviewController(_CABAdminService);
//        }

//        [Test]
//        public async Task ReviewListReturnsView()
//        {
//            _ICABRepository.Setup(r => r.Query<Document>(d =>
//                d.State == State.ApprovedForPublishing)).ReturnsAsync(new List<Document>());

//            var result = await _sut.List();
//            var viewResult = result as ViewResult;

//            Assert.IsTrue(viewResult.Model is ReviewListViewModel);
//        }
//    }
//}
