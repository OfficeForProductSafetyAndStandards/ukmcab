using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Services;
using UKMCAB.Data.Models;
using System;
using System.Collections.Generic;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Core.Services.Users;
using UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Data.Models.Users;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace UKMCAB.Web.UI.Tests.Areas.Admin.Controllers.LegislativeArea
{
    [TestFixture]
    public class LegislativeAreaDetailsControllerTests : ControllerBaseTestsBase
    {
        private Mock<IDistCache> _mockDistCache = null!;
        private Mock<ICABAdminService> _mockCabAdminService = null!;
        private Mock<ILegislativeAreaService> _mockLegislativeAreaService = null!;
        private Mock<ILegislativeAreaDetailService> _mockLegislativeAreaDetailService = null!;
        private Mock<IUserService> _mockUserService = null!;
        private LegislativeAreaDetailsController _sut = null!;


        [SetUp]
        public void Setup()
        {
            _mockDistCache = new Mock<IDistCache>();
            _mockCabAdminService = new Mock<ICABAdminService>();
            _mockLegislativeAreaService = new Mock<ILegislativeAreaService>();
            _mockLegislativeAreaDetailService = new Mock<ILegislativeAreaDetailService>();
            _mockUserService = new Mock<IUserService>();

            var mockBbjectValidator = new Mock<IObjectModelValidator>();
            mockBbjectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<object>()));

            _sut = new LegislativeAreaDetailsController(_mockCabAdminService.Object, _mockLegislativeAreaService.Object, _mockLegislativeAreaDetailService.Object, _mockUserService.Object, _mockDistCache.Object)
            {
                ControllerContext = GetControllerContextWithUser(),
                ObjectValidator = mockBbjectValidator.Object,
            };

        }

        [Test]
        public async Task ShouldAddProceduresToSOA_When_TheyAreLinkedDirectlyToPurposeofAppointment()
        {
            //Arrange
            var id = Guid.NewGuid();
            var scopeId = Guid.NewGuid();
            var compareScopeId = Guid.NewGuid();
            var procedureId1 = Guid.NewGuid();
            var procedureId2 = Guid.NewGuid();
            var procedureIds = new List<Guid> { procedureId1, procedureId2 };
            var vm = new ProcedureViewModel
            {
                SelectedProcedureIds = procedureIds
            };
            var submitType = Constants.SubmitType.Continue;
            var userAccount = new UserAccount
            {
                Id = "Test id",
                FirstName = "Test",
                Surname = "User",
                Role = "Test role"
            };

            var scopeOfAppointment = new DocumentScopeOfAppointment { Id = Guid.NewGuid(), CategoryIdAndProcedureIds = new() };
            var lastestDocument = new Document
            {
                ScopeOfAppointments = new List<DocumentScopeOfAppointment> { scopeOfAppointment }
            };

            _mockDistCache.Setup(x => x.GetAsync<DocumentScopeOfAppointment>(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(scopeOfAppointment);
            _mockDistCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<DocumentScopeOfAppointment>(), It.IsAny<TimeSpan>(), -1)).ReturnsAsync(scopeOfAppointment.Id.ToString());
            _mockCabAdminService.Setup(ca => ca.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync(lastestDocument);

            _mockLegislativeAreaService.Setup(la => la.GetAllLegislativeAreasAsync()).ReturnsAsync(new List<LegislativeAreaModel>());

            _mockUserService.Setup(u => u.GetAsync(It.IsAny<string>())).ReturnsAsync(userAccount);
            _mockCabAdminService.Setup(c => c.UpdateOrCreateDraftDocumentAsync(userAccount, lastestDocument, false)).ReturnsAsync(lastestDocument);

            //Act
            var result = await _sut.AddProcedure(id, scopeId, 0, 0, vm, compareScopeId, submitType);

            //Assert
            Assert.That(scopeOfAppointment.CategoryIdAndProcedureIds.Count, Is.EqualTo(1));
            Assert.Contains(procedureId1, scopeOfAppointment.CategoryIdAndProcedureIds[0].ProcedureIds);
            Assert.Contains(procedureId2, scopeOfAppointment.CategoryIdAndProcedureIds[0].ProcedureIds);
        }

        [Test]
        public async Task ShouldAddProceduresToSOA_When_TheyAreLinkedDirectlyToProducts()
        {
            //Arrange
            var id = Guid.NewGuid();
            var scopeId = Guid.NewGuid();
            var compareScopeId = Guid.NewGuid();
            var procedureId1 = Guid.NewGuid();
            var procedureId2 = Guid.NewGuid();
            var procedureIds = new List<Guid> { procedureId1, procedureId2 };
            var vm = new ProcedureViewModel
            {
                CurrentProductId = Guid.NewGuid(),
                SelectedProcedureIds = procedureIds
            };
            var submitType = Constants.SubmitType.Continue;
            var userAccount = new UserAccount
            {
                Id = "Test id",
                FirstName = "Test",
                Surname = "User",
                Role = "Test role"
            };

            var scopeOfAppointment = new DocumentScopeOfAppointment { Id = Guid.NewGuid(), CategoryIdAndProcedureIds = new() };
            var lastestDocument = new Document
            {
                ScopeOfAppointments = new List<DocumentScopeOfAppointment> { scopeOfAppointment }
            };

            _mockDistCache.Setup(x => x.GetAsync<DocumentScopeOfAppointment>(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(scopeOfAppointment);
            _mockDistCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<DocumentScopeOfAppointment>(), It.IsAny<TimeSpan>(), -1)).ReturnsAsync(scopeOfAppointment.Id.ToString());
            _mockCabAdminService.Setup(ca => ca.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync(lastestDocument);

            _mockLegislativeAreaService.Setup(la => la.GetAllLegislativeAreasAsync()).ReturnsAsync(new List<LegislativeAreaModel>());

            _mockUserService.Setup(u => u.GetAsync(It.IsAny<string>())).ReturnsAsync(userAccount);
            _mockCabAdminService.Setup(c => c.UpdateOrCreateDraftDocumentAsync(userAccount, lastestDocument, false)).ReturnsAsync(lastestDocument);

            //Act
            var result = await _sut.AddProcedure(id, scopeId, 0, 0, vm, compareScopeId, submitType);

            //Assert
            Assert.That(scopeOfAppointment.ProductIdAndProcedureIds.Count, Is.AtLeast(1));
            Assert.Contains(procedureId1, scopeOfAppointment.ProductIdAndProcedureIds[0].ProcedureIds);
            Assert.Contains(procedureId2, scopeOfAppointment.ProductIdAndProcedureIds[0].ProcedureIds);
        }

        [Test]
        public async Task ShouldAddProceduresToSOA_When_TheyAreLinkedDirectlyToProductCategories()
        {
            //Arrange
            var id = Guid.NewGuid();
            var scopeId = Guid.NewGuid();
            var compareScopeId = Guid.NewGuid();
            var procedureId1 = Guid.NewGuid();
            var procedureId2 = Guid.NewGuid();
            var procedureIds = new List<Guid> { procedureId1, procedureId2 };
            var vm = new ProcedureViewModel
            {
                CurrentCategoryId = Guid.NewGuid(),
                SelectedProcedureIds = procedureIds
            };
            var submitType = Constants.SubmitType.Continue;
            var userAccount = new UserAccount
            {
                Id = "Test id",
                FirstName = "Test",
                Surname = "User",
                Role = "Test role"
            };

            var scopeOfAppointment = new DocumentScopeOfAppointment { Id = Guid.NewGuid(), CategoryIdAndProcedureIds = new() };
            var lastestDocument = new Document
            {
                ScopeOfAppointments = new List<DocumentScopeOfAppointment> { scopeOfAppointment }
            };

            _mockDistCache.Setup(x => x.GetAsync<DocumentScopeOfAppointment>(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(scopeOfAppointment);
            _mockDistCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<DocumentScopeOfAppointment>(), It.IsAny<TimeSpan>(), -1)).ReturnsAsync(scopeOfAppointment.Id.ToString());
            _mockCabAdminService.Setup(ca => ca.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync(lastestDocument);

            _mockLegislativeAreaService.Setup(la => la.GetAllLegislativeAreasAsync()).ReturnsAsync(new List<LegislativeAreaModel>());

            _mockUserService.Setup(u => u.GetAsync(It.IsAny<string>())).ReturnsAsync(userAccount);
            _mockCabAdminService.Setup(c => c.UpdateOrCreateDraftDocumentAsync(userAccount, lastestDocument, false)).ReturnsAsync(lastestDocument);

            //Act
            var result = await _sut.AddProcedure(id, scopeId, 0, 0, vm, compareScopeId, submitType);

            //Assert
            Assert.That(scopeOfAppointment.CategoryIdAndProcedureIds.Count, Is.AtLeast(1));
            Assert.Contains(procedureId1, scopeOfAppointment.CategoryIdAndProcedureIds[0].ProcedureIds);
            Assert.Contains(procedureId2, scopeOfAppointment.CategoryIdAndProcedureIds[0].ProcedureIds);
        }

        [Test]
        public async Task AddDesignatedStandard_NoDocumentScopeOfAppointment_RedirectsToReviewLegislativeAreas()
        {
            // Arrange 
            var scopeId = Guid.NewGuid();
            var cabId = Guid.NewGuid();
            var expectedResult = new RedirectToRouteResult(LegislativeAreaReviewController.Routes.ReviewLegislativeAreas,
                new
                {
                    CABId = cabId,
                    fromSummary = false
                });

            //Act
            var result = await _sut.AddDesignatedStandard(string.Empty,
                new DesignatedStandardsViewModel
                {
                    ScopeId = scopeId,
                    CABId = cabId,
                    IsFromSummary = false
                });

            //Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task AddDesignatedStandard_SubmitTypeSearch_ReturnsAddDesignatedStandardView()
        {
            // Arrange 
            var scopeId = Guid.NewGuid();
            var cabId = Guid.NewGuid();
            var legislativeAreaId = Guid.NewGuid();
            var pageNumber = 1;
            var searchTerm = "Test keywords";
            var designatedStandardIds = new List<Guid>() { Guid.NewGuid() };

            var expectedResult = new ViewResult()
            {
                ViewName = "~/Areas/Admin/views/CAB/LegislativeArea/AddDesignatedStandard.cshtml",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new DesignatedStandardsViewModel
                    {
                        ScopeId = scopeId,
                        CABId = cabId,
                        IsFromSummary = false,
                        PageNumber = pageNumber,
                        SearchTerm = searchTerm,
                        SelectedDesignatedStandardIds = designatedStandardIds,
                        PaginationSearchTerm = searchTerm
                    }
                }
            };

            _mockDistCache.Setup(m => m.GetAsync<DocumentScopeOfAppointment>($"soa_create_{scopeId}", -1))
                .ReturnsAsync(new DocumentScopeOfAppointment
                {
                    LegislativeAreaId = legislativeAreaId,
                }).Verifiable();
            _mockLegislativeAreaService.Setup(m => m.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(legislativeAreaId, pageNumber, searchTerm, 20, designatedStandardIds))
                .ReturnsAsync(new ScopeOfAppointmentOptionsModel()).Verifiable();

            //Act
            var result = await _sut.AddDesignatedStandard(Constants.SubmitType.Search, 
                new DesignatedStandardsViewModel
                {
                    ScopeId = scopeId,
                    CABId = cabId,
                    IsFromSummary = false,
                    PageNumber = pageNumber,
                    SearchTerm = searchTerm,
                    SelectedDesignatedStandardIds = designatedStandardIds,
                });

            //Assert
            _mockDistCache.VerifyAll();
            _mockLegislativeAreaService.VerifyAll();
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task AddDesignatedStandard_SubmitTypePaginatedQueryShowAllSelectedTrue_ReturnsAddDesignatedStandardView()
        {
            // Arrange 
            var scopeId = Guid.NewGuid();
            var cabId = Guid.NewGuid();
            var legislativeAreaId = Guid.NewGuid();
            var pageNumber = 1;
            var designatedStandardIds = new List<Guid>() { Guid.NewGuid() };
            var paginationSearchTerm = "Test keywords";

            var expectedResult = new ViewResult()
            {
                ViewName = "~/Areas/Admin/views/CAB/LegislativeArea/AddDesignatedStandard.cshtml",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new DesignatedStandardsViewModel
                    {
                        ScopeId = scopeId,
                        CABId = cabId,
                        IsFromSummary = false,
                        PageNumber = pageNumber,
                        SelectedDesignatedStandardIds = designatedStandardIds,
                        PaginationSearchTerm = paginationSearchTerm,
                        ShowAllSelected = true
                    }
                }
            };

            _mockDistCache.Setup(m => m.GetAsync<DocumentScopeOfAppointment>($"soa_create_{scopeId}", -1))
                .ReturnsAsync(new DocumentScopeOfAppointment
                {
                    LegislativeAreaId = legislativeAreaId,
                }).Verifiable();
            _mockLegislativeAreaService.Setup(m => m.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(legislativeAreaId, pageNumber, paginationSearchTerm, 20, designatedStandardIds))
                .ReturnsAsync(new ScopeOfAppointmentOptionsModel()).Verifiable();

            //Act
            var result = await _sut.AddDesignatedStandard(Constants.SubmitType.PaginatedQuery,
                new DesignatedStandardsViewModel
                {
                    ScopeId = scopeId,
                    CABId = cabId,
                    IsFromSummary = false,
                    PageNumber = pageNumber,
                    SelectedDesignatedStandardIds = designatedStandardIds,
                    PaginationSearchTerm = paginationSearchTerm,
                    ShowAllSelected = true
                });

            //Assert
            _mockDistCache.VerifyAll();
            _mockLegislativeAreaService.VerifyAll();
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task AddDesignatedStandard_SubmitTypePaginatedQueryShowAllSelectedFalse_ReturnsAddDesignatedStandardView()
        {
            // Arrange 
            var scopeId = Guid.NewGuid();
            var cabId = Guid.NewGuid();
            var legislativeAreaId = Guid.NewGuid();
            var pageNumber = 1;
            var designatedStandardIds = new List<Guid>() { Guid.NewGuid() };
            var paginationSearchTerm = "Test keywords";

            var expectedResult = new ViewResult()
            {
                ViewName = "~/Areas/Admin/views/CAB/LegislativeArea/AddDesignatedStandard.cshtml",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new DesignatedStandardsViewModel
                    {
                        ScopeId = scopeId,
                        CABId = cabId,
                        IsFromSummary = false,
                        PageNumber = pageNumber,
                        SelectedDesignatedStandardIds = designatedStandardIds,
                        PaginationSearchTerm = paginationSearchTerm,
                        ShowAllSelected = false
                    }
                }
            };

            _mockDistCache.Setup(m => m.GetAsync<DocumentScopeOfAppointment>($"soa_create_{scopeId}", -1))
                .ReturnsAsync(new DocumentScopeOfAppointment
                {
                    LegislativeAreaId = legislativeAreaId,
                }).Verifiable();
            _mockLegislativeAreaService.Setup(m => m.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(legislativeAreaId, pageNumber, paginationSearchTerm, 20, null))
                .ReturnsAsync(new ScopeOfAppointmentOptionsModel()).Verifiable();

            //Act
            var result = await _sut.AddDesignatedStandard(Constants.SubmitType.PaginatedQuery,
                new DesignatedStandardsViewModel
                {
                    ScopeId = scopeId,
                    CABId = cabId,
                    IsFromSummary = false,
                    PageNumber = pageNumber,
                    SelectedDesignatedStandardIds = designatedStandardIds,
                    PaginationSearchTerm = paginationSearchTerm,
                    ShowAllSelected = false
                });

            //Assert
            _mockDistCache.VerifyAll();
            _mockLegislativeAreaService.VerifyAll();
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task AddDesignatedStandard_SubmitTypeShowAllSelected_ReturnsAddDesignatedStandardView()
        {
            // Arrange 
            var scopeId = Guid.NewGuid();
            var cabId = Guid.NewGuid();
            var legislativeAreaId = Guid.NewGuid();
            var pageNumber = 1;
            var designatedStandardIds = new List<Guid>() { Guid.NewGuid() };

            var expectedResult = new ViewResult()
            {
                ViewName = "~/Areas/Admin/views/CAB/LegislativeArea/AddDesignatedStandard.cshtml",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new DesignatedStandardsViewModel
                    {
                        ScopeId = scopeId,
                        CABId = cabId,
                        IsFromSummary = false,
                        PageNumber = pageNumber,
                        SelectedDesignatedStandardIds = designatedStandardIds,
                    }
                }
            };

            _mockDistCache.Setup(m => m.GetAsync<DocumentScopeOfAppointment>($"soa_create_{scopeId}", -1))
                .ReturnsAsync(new DocumentScopeOfAppointment
                {
                    LegislativeAreaId = legislativeAreaId,
                }).Verifiable();
            _mockLegislativeAreaService.Setup(m => m.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(legislativeAreaId, pageNumber, null, 20, designatedStandardIds))
                .ReturnsAsync(new ScopeOfAppointmentOptionsModel()).Verifiable();

            //Act
            var result = await _sut.AddDesignatedStandard(Constants.SubmitType.ShowAllSelected,
                new DesignatedStandardsViewModel
                {
                    ScopeId = scopeId,
                    CABId = cabId,
                    IsFromSummary = false,
                    PageNumber = pageNumber,
                    SelectedDesignatedStandardIds = designatedStandardIds,
                });

            //Assert
            _mockDistCache.VerifyAll();
            _mockLegislativeAreaService.VerifyAll();
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task AddDesignatedStandard_SubmitTypeClearShowAllSelected_ReturnsAddDesignatedStandardView()
        {
            // Arrange 
            var scopeId = Guid.NewGuid();
            var cabId = Guid.NewGuid();
            var legislativeAreaId = Guid.NewGuid();
            var pageNumber = 1;
            var designatedStandardIds = new List<Guid>() { Guid.NewGuid() };
            var paginationSearchTerm = "Test keywords";

            var expectedResult = new ViewResult()
            {
                ViewName = "~/Areas/Admin/views/CAB/LegislativeArea/AddDesignatedStandard.cshtml",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new DesignatedStandardsViewModel
                    {
                        ScopeId = scopeId,
                        CABId = cabId,
                        IsFromSummary = false,
                        PageNumber = pageNumber,
                        SelectedDesignatedStandardIds = designatedStandardIds,
                        PaginationSearchTerm = paginationSearchTerm,
                    }
                }
            };

            _mockDistCache.Setup(m => m.GetAsync<DocumentScopeOfAppointment>($"soa_create_{scopeId}", -1))
                .ReturnsAsync(new DocumentScopeOfAppointment
                {
                    LegislativeAreaId = legislativeAreaId,
                }).Verifiable();
            _mockLegislativeAreaService.Setup(m => m.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(legislativeAreaId, pageNumber, paginationSearchTerm, 20, null))
                .ReturnsAsync(new ScopeOfAppointmentOptionsModel()).Verifiable();

            //Act
            var result = await _sut.AddDesignatedStandard(Constants.SubmitType.ClearShowAllSelected,
                new DesignatedStandardsViewModel
                {
                    ScopeId = scopeId,
                    CABId = cabId,
                    IsFromSummary = false,
                    PageNumber = pageNumber,
                    SelectedDesignatedStandardIds = designatedStandardIds,
                    PaginationSearchTerm = paginationSearchTerm,
                });

            //Assert
            _mockDistCache.VerifyAll();
            _mockLegislativeAreaService.VerifyAll();
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task AddDesignatedStandard_ValidModel_RedirectToLegislativeAreaAdditionalInformation()
        {
            // Arrange 
            var scopeId = Guid.NewGuid();
            var cabId = Guid.NewGuid();
            var legislativeAreaId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var documentId = Guid.NewGuid().ToString();

            var expectedResult = new RedirectToRouteResult(
                LegislativeAreaAdditionalInformationController.Routes.LegislativeAreaAdditionalInformation,
                new
                {
                    id = cabId,
                    laId = legislativeAreaId,
                    fromSummary = false
                });

            _mockDistCache.Setup(m => m.GetAsync<DocumentScopeOfAppointment>($"soa_create_{scopeId}", -1))
                .ReturnsAsync(new DocumentScopeOfAppointment
                {
                    LegislativeAreaId = legislativeAreaId,
                }).Verifiable();
            _mockCabAdminService.Setup(m => m.GetLatestDocumentAsync(cabId.ToString()))
                .ReturnsAsync(new Document
                {
                    id = documentId,
                }).Verifiable();
            _mockUserService.Setup(m => m.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new UserAccount
                {
                    Id = userId,
                }).Verifiable();
            _mockCabAdminService.Setup(m => m.UpdateOrCreateDraftDocumentAsync(It.Is<UserAccount>(u => u.Id == userId), It.Is<Document>(u => u.id == documentId), false)).ReturnsAsync(new Document()).Verifiable();

            //Act
            var result = await _sut.AddDesignatedStandard(Constants.SubmitType.Continue,
                new DesignatedStandardsViewModel
                {
                    ScopeId = scopeId,
                    CABId = cabId,
                    IsFromSummary = false,
                });

            //Assert
            _mockDistCache.VerifyAll();
            _mockCabAdminService.VerifyAll();
            _mockUserService.VerifyAll();

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task AddDesignatedStandard_DuplicateScopeOfAppointment_RedirectToReviewLegislativeAreas()
        {
            // Arrange 
            var scopeId = Guid.NewGuid();
            var cabId = Guid.NewGuid();
            var legislativeAreaId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var documentId = Guid.NewGuid().ToString();

            var expectedResult = new RedirectToRouteResult(
                LegislativeAreaReviewController.Routes.ReviewLegislativeAreas,
                new
                {
                    id = cabId,
                    fromSummary = false,
                    bannerContent = Constants.ErrorMessages.DuplicateEntry
                });

            _mockDistCache.Setup(m => m.GetAsync<DocumentScopeOfAppointment>($"soa_create_{scopeId}", -1))
                .ReturnsAsync(new DocumentScopeOfAppointment
                {
                    LegislativeAreaId = legislativeAreaId,
                }).Verifiable();
            _mockCabAdminService.Setup(m => m.GetLatestDocumentAsync(cabId.ToString()))
                .ReturnsAsync(new Document
                {
                    id = documentId,
                    ScopeOfAppointments = new List<DocumentScopeOfAppointment> {
                        new DocumentScopeOfAppointment
                        {
                            LegislativeAreaId = legislativeAreaId,
                        }
                    }
                }).Verifiable();
            _mockUserService.Setup(m => m.GetAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<UserAccount>()).Verifiable();
            _mockCabAdminService.Setup(m => m.UpdateOrCreateDraftDocumentAsync(It.IsAny<UserAccount>(), It.IsAny<Document>(), false))
                .ReturnsAsync(new Document()).Verifiable();

            //Act
            var result = await _sut.AddDesignatedStandard(Constants.SubmitType.Continue,
                new DesignatedStandardsViewModel
                {
                    ScopeId = scopeId,
                    CABId = cabId,
                    IsFromSummary = false,
                });

            //Assert
            _mockDistCache.VerifyAll();
            _mockCabAdminService.Verify(m => m.GetLatestDocumentAsync(cabId.ToString()));
            _mockCabAdminService.VerifyNoOtherCalls();
            _mockUserService.VerifyNoOtherCalls();

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public Task AddDesignatedStandard_InvalidSubmitType_ThrowsInvalidOperationException()
        {
            // Arrange 
            var scopeId = Guid.NewGuid();
            var cabId = Guid.NewGuid();
            var legislativeAreaId = Guid.NewGuid();

            _mockDistCache.Setup(m => m.GetAsync<DocumentScopeOfAppointment>($"soa_create_{scopeId}", -1))
                .ReturnsAsync(new DocumentScopeOfAppointment
                {
                    LegislativeAreaId = legislativeAreaId,
                }).Verifiable();

            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    var result = await _sut.AddDesignatedStandard("InvalidSubmitType",
                                    new DesignatedStandardsViewModel
                                    {
                                        ScopeId = scopeId,
                                        CABId = cabId,
                                        IsFromSummary = false,
                                    });
                });

                return Task.CompletedTask;

        }
    }
}
