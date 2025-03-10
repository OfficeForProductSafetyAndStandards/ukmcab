﻿using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Web.UI.Areas.Admin.Controllers;
using UKMCAB.Web.UI.Models.Builders;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Services;

#pragma warning disable CS8618

namespace UKMCAB.Web.UI.Tests.Areas.Admin.Controllers
{
    [TestFixture]
    internal class CabControllerTests
    {
        private Mock<ICABAdminService> _mockCabAdminService;
        private Mock<IUserService> _mockUserService;
        private Mock<IWorkflowTaskService> _mockWorkflowTaskService;
        private Mock<IAsyncNotificationClient> _mockNotificationClient;
        private Mock<IEditLockService> _mockEditLockService;
        private Mock<IUserNoteService> _mockUserNoteService;
        private Mock<ILegislativeAreaService> _mockLegislativeAreaService;
        private Mock<ICabSummaryViewModelBuilder> _mockCabSummaryViewModelBuilder;
        private Mock<ICabLegislativeAreasViewModelBuilder> _mockCabLegislativeAreasViewModelBuilder;
        private Mock<ICabSummaryUiService> _mockCabSummaryUiService;
        private Mock<HttpContext> _mockHttpContext;
        private CoreEmailTemplateOptions _templateOptions;

        private CABController _sut;
        private string _userId = "Test user id";

        [SetUp]
        public void Setup()
        {
            _mockCabAdminService = new Mock<ICABAdminService>(MockBehavior.Strict);
            _mockUserService = new Mock<IUserService>(MockBehavior.Strict);
            _mockWorkflowTaskService = new Mock<IWorkflowTaskService>(MockBehavior.Strict);
            _mockNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            _mockEditLockService = new Mock<IEditLockService>(MockBehavior.Strict);
            _mockUserNoteService = new Mock<IUserNoteService>(MockBehavior.Strict);
            _mockLegislativeAreaService = new Mock<ILegislativeAreaService>(MockBehavior.Strict);
            _mockCabSummaryViewModelBuilder = new Mock<ICabSummaryViewModelBuilder>(MockBehavior.Strict);
            _mockCabLegislativeAreasViewModelBuilder = new Mock<ICabLegislativeAreasViewModelBuilder>(MockBehavior.Strict);
            _mockCabSummaryUiService = new Mock<ICabSummaryUiService>(MockBehavior.Strict);
            
            _sut = new CABController(
                _mockCabAdminService.Object,
                _mockUserService.Object,
                _mockWorkflowTaskService.Object,
                _mockNotificationClient.Object,
                Options.Create(new CoreEmailTemplateOptions()),
                _mockEditLockService.Object,
                _mockUserNoteService.Object,
                _mockLegislativeAreaService.Object,
                _mockCabSummaryViewModelBuilder.Object,
                _mockCabLegislativeAreasViewModelBuilder.Object,
            _mockCabSummaryUiService.Object);            

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, _userId),
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
            var httpContext = new DefaultHttpContext
            {
                User = user,
            };
            httpContext.Request.Host = new HostString("test", 1);
            _sut.ControllerContext.HttpContext = httpContext;

            var mockValidator = new Mock<IObjectModelValidator>();
            mockValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(), It.IsAny<ValidationStateDictionary>(), It.IsAny<string>(),It.IsAny<object>()));
            _sut.ObjectValidator = mockValidator.Object;
        }

        [Test]
        public async Task HttpGetSummary_LocksCabForUserAndReturnsCABSummaryViewModel()
        {
            // Arrange
            (var documentId, var cabSummaryViewModelId) = HttpGetSummary_ReturnsCabSummaryViewModel(true);
            var expectedResult = new CABSummaryViewModel
            {
                Id = cabSummaryViewModelId
            };

            _mockCabSummaryUiService.Setup(m => m.LockCabForUser(It.Is<CABSummaryViewModel>(m => m.Id == cabSummaryViewModelId))).Returns(Task.CompletedTask);

            _sut.Url = new Mock<IUrlHelper>().Object;

            // Act
            var result = await _sut.Summary(documentId, null, null, null) as ViewResult;

            // Assert
            result.Model.Should().BeEquivalentTo(expectedResult);
            _mockCabSummaryUiService.Verify(m => m.LockCabForUser(It.Is<CABSummaryViewModel>(m => m.Id == cabSummaryViewModelId)), Times.Once);
        }

        [Test]
        public async Task HttpGetSummary_DoesNotLockCabForUserAndReturnsCABSummaryViewModel()
        {
            // Arrange
            (var documentId, var cabSummaryViewModelId) = HttpGetSummary_ReturnsCabSummaryViewModel(false);
            var expectedResult = new CABSummaryViewModel
            {
                Id = cabSummaryViewModelId
            };

            _sut.Url = new Mock<IUrlHelper>().Object;

            // Act
            var result = await _sut.Summary(documentId, null, null, null) as ViewResult;

            // Assert
            result.Model.Should().BeEquivalentTo(expectedResult);
            _mockCabSummaryUiService.Verify(m => m.LockCabForUser(It.IsAny<CABSummaryViewModel>()), Times.Never);
        }

        private (string documentId, string cabSummaryViewModelId) HttpGetSummary_ReturnsCabSummaryViewModel(bool isEditLocked)
        {
            // Arrange
            var cabNameAlreadyExists = true;
            var bannerMessage = "Test banner message";
            var documentId = "Test document id";
            var cabId = "Test cab id";
            var cabSummaryViewModelId = "Test cab summary view model id";

            var documentLegislativeAreas = new List<DocumentLegislativeArea> { new() { Id = Guid.NewGuid() } };
            var scopeOfAppointment = new List<DocumentScopeOfAppointment> { new() { Id = Guid.NewGuid() } };
            var document = new Document
            {
                id = documentId,
                CABId = cabId,
                DocumentLegislativeAreas = documentLegislativeAreas,
                ScopeOfAppointments = scopeOfAppointment
            };

            var legislativeAreaModels = new List<LegislativeAreaModel> { new() { Id = Guid.NewGuid() } };
            var purposeOfAppointmentModels = new List<PurposeOfAppointmentModel> { new() { Id = Guid.NewGuid() } };
            var categoryModels = new List<CategoryModel> { new() { Id = Guid.NewGuid() } };
            var subCategoryModels = new List<SubCategoryModel> { new() { Id = Guid.NewGuid() } };
            var productModels = new List<ProductModel> { new() { Id = Guid.NewGuid() } };
            var procedureModels = new List<ProcedureModel> { new() { Id = Guid.NewGuid() } };
            var designatedStandardModels = new List<DesignatedStandardModel> { new(Guid.NewGuid(), "Test designated standard", Guid.NewGuid(), new List<string>(), "Test publication reference" ) };
            var ppeProductTypeModels = new List<PpeProductTypeModel> { new() { Id = Guid.NewGuid() } };
            var protectionAgainstRiskModels = new List<ProtectionAgainstRiskModel> { new() { Id = Guid.NewGuid() } };
            var areaOfCompetencyModels = new List<AreaOfCompetencyModel> { new() { Id = Guid.NewGuid() } };

            var expectedDocLaIds = documentLegislativeAreas.Select(l => l.Id);
            var expectedLaIds = legislativeAreaModels.Select(l => l.Id);
            var expectedScopeOfAppointmentIds = scopeOfAppointment.Select(l => l.Id);
            var expectedPurposeOfAppointmentIds = purposeOfAppointmentModels.Select(l => l.Id);
            var expectedCategoryIds = categoryModels.Select(l => l.Id);
            var expectedSubCategoryIds = subCategoryModels.Select(l => l.Id);
            var expectedProductsIds = productModels.Select(l => l.Id);
            var expectedProcedureIds = procedureModels.Select(l => l.Id);
            var expectedDesignatedStandardsIds = designatedStandardModels.Select(l => l.Id);
            var expectedPpeProductTypeIds = ppeProductTypeModels.Select(l => l.Id);
            var expectedProtectionAgainstRiskIds = protectionAgainstRiskModels.Select(l => l.Id);
            var expectedAreaOfCompetencyIds = areaOfCompetencyModels.Select(l => l.Id);

            _mockCabAdminService.Setup(m => m.GetLatestDocumentAsync(documentId)).ReturnsAsync(document);
            _mockCabSummaryUiService.Setup(m => m.CreateDocumentAsync(It.Is<Document>(d => d.id == document.id), null)).Returns(Task.CompletedTask);
            _mockLegislativeAreaService.Setup(m => m.GetLegislativeAreasForDocumentAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(legislativeAreaModels);
            _mockLegislativeAreaService.Setup(m => m.GetPurposeOfAppointmentsForDocumentAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(purposeOfAppointmentModels);
            _mockLegislativeAreaService.Setup(m => m.GetCategoriesForDocumentAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(categoryModels);
            _mockLegislativeAreaService.Setup(m => m.GetSubCategoriesForDocumentAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(subCategoryModels);
            _mockLegislativeAreaService.Setup(m => m.GetProductsForDocumentAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(productModels);
            _mockLegislativeAreaService.Setup(m => m.GetProceduresForDocumentAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(procedureModels);
            _mockLegislativeAreaService.Setup(m => m.GetDesignatedStandardsForDocumentAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(designatedStandardModels);
            _mockLegislativeAreaService.Setup(m => m.GetPpeProductTypesForDocumentAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(ppeProductTypeModels);
            _mockLegislativeAreaService.Setup(m => m.GetProtectionAgainstRisksForDocumentAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(protectionAgainstRiskModels);
            _mockLegislativeAreaService.Setup(m => m.GetAreaOfCompetenciesForDocumentAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(areaOfCompetencyModels);
            _mockCabAdminService.Setup(m => m.DocumentWithSameNameExistsAsync(It.Is<Document>(d => d.id == document.id)))
                .ReturnsAsync(cabNameAlreadyExists);
            _mockEditLockService.Setup(m => m.IsCabLockedForUser(cabId, _userId)).ReturnsAsync(isEditLocked);
            _mockCabSummaryUiService.Setup(m => m.GetSuccessBannerMessage()).Returns(bannerMessage);
            _mockCabLegislativeAreasViewModelBuilder.Setup(m => m.WithDocumentLegislativeAreas(
                It.Is<List<DocumentLegislativeArea>>(las => las.Any() && las.All(l => expectedDocLaIds.Contains(l.Id))),
                It.Is<List<LegislativeAreaModel>>(las => las.Any() && las.All(l => expectedLaIds.Contains(l.Id))),
                It.Is<List<DocumentScopeOfAppointment>>(soas => soas.Any() && soas.All(s => expectedScopeOfAppointmentIds.Contains(s.Id))),
                It.Is<List<PurposeOfAppointmentModel>>(poas => poas.Any() && poas.All(p => expectedPurposeOfAppointmentIds.Contains(p.Id))),
                It.Is<List<CategoryModel>>(categories => categories.Any() && categories.All(c => expectedCategoryIds.Contains(c.Id))),
                It.Is<List<SubCategoryModel>>(subCategories => subCategories.Any() && subCategories.All(s => expectedSubCategoryIds.Contains(s.Id))),
                It.Is<List<ProductModel>>(products => products.Any() && products.All(p => expectedProductsIds.Contains(p.Id))),
                It.Is<List<ProcedureModel>>(procedures => procedures.Any() && procedures.All(p => expectedProcedureIds.Contains(p.Id))),
                It.Is<List<DesignatedStandardModel>>(designatedStandards => designatedStandards.Any() && designatedStandards.All(p => expectedDesignatedStandardsIds.Contains(p.Id))),
                It.Is<List<PpeProductTypeModel>>(ppeProductTypes => ppeProductTypes.Any() && ppeProductTypes.All(p => expectedPpeProductTypeIds.Contains(p.Id))),
                It.Is<List<ProtectionAgainstRiskModel>>(protectionAgainstRisks => protectionAgainstRisks.Any() && protectionAgainstRisks.All(p => expectedProtectionAgainstRiskIds.Contains(p.Id))),
                It.Is<List<AreaOfCompetencyModel>>(areaOfCompetencies => areaOfCompetencies.Any() && areaOfCompetencies.All(a => expectedAreaOfCompetencyIds.Contains(a.Id)))
            )).Returns(_mockCabLegislativeAreasViewModelBuilder.Object);
            _mockCabLegislativeAreasViewModelBuilder.Setup(m => m.Build()).Returns(new CABLegislativeAreasViewModel());
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithRoleInfo(It.Is<Document>(d => d.id == document.id))).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithDocumentDetails(It.Is<Document>(d => d.id == document.id))).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithLegislativeAreasPendingApprovalCount(It.Is<Document>(d => d.id == document.id))).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithReturnUrl(null)).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithCabDetails(It.IsAny<CABDetailsViewModel>())).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithCabContactViewModel(It.IsAny<CABContactViewModel>())).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithCabBodyDetailsMRAViewModel(It.IsAny<CABBodyDetailsMRAViewModel>())).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithCabLegislativeAreasViewModel(It.IsAny<CABLegislativeAreasViewModel>())).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithProductScheduleDetailsViewModel(It.IsAny<CABProductScheduleDetailsViewModel>())).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithCabSupportingDocumentDetailsViewModel(It.IsAny<CABSupportingDocumentDetailsViewModel>())).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithCabGovernmentUserNotesViewModel(It.IsAny<CABGovernmentUserNotesViewModel>())).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithCabHistoryViewModel(It.IsAny<CABHistoryViewModel>())).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithCabPublishTypeViewModel(It.IsAny<CABPublishTypeViewModel>())).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithIsEditLocked(isEditLocked)).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithRevealEditActions(null)).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithRequestedFromCabProfilePage(null)).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.WithSuccessBannerMessage(bannerMessage)).Returns(_mockCabSummaryViewModelBuilder.Object);
            _mockCabSummaryViewModelBuilder.Setup(m => m.Build()).Returns(new CABSummaryViewModel { Id = cabSummaryViewModelId });

            return (documentId, cabSummaryViewModelId);
        }

        [Test]
        public async Task HttpGetSummary_DocumentNotFound_RedirectsToCABManagement()
        {
            // Arrange
            var documentId = "Test document id";
            var expectedResult = new RedirectToActionResult("CABManagement", "CabManagement", new { Area = "admin" });

            _mockCabAdminService.Setup(m => m.GetLatestDocumentAsync(documentId)).ReturnsAsync((Document?)null);

            // Act
            var result = await _sut.Summary(documentId, null, null, null) as RedirectToActionResult;

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}
